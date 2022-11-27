﻿using CToolkitCs.v1_2Core.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace CToolkitCs.v1_2Core.Logging
{
    public class CtkLogger : IDisposable
    {

        protected ConcurrentQueue<CtkLoggerEventArgs> queue = new ConcurrentQueue<CtkLoggerEventArgs>();
        CtkTask task;

        public virtual void Debug(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Debug); }
        public virtual void Error(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Error); }
        public virtual void Fatal(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Fatal); }
        public virtual void Info(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Info); }
        public virtual void Verbose(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Verbose); }
        public virtual void Warn(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Warn); }

        /// <summary>
        /// 預設寫Log是用非同步
        /// </summary>
        /// <param name="ea"></param>
        public virtual void Write(CtkLoggerEventArgs ea) { this.WriteAsyn(ea); }
        public virtual void Write(CtkLoggerEventArgs ea, CtkLoggerEnumLevel _level)
        {
            ea.Level = _level;
            this.WriteAsyn(ea);
        }
        protected virtual void WriteAsyn(CtkLoggerEventArgs ea)
        {
            this.queue.Enqueue(ea);
            if (!Monitor.TryEnter(this, 1000)) return;
            try
            {
                //若還沒結束執行, 先return
                if (this.task != null && !this.task.IsEnd()) return;
                this.CloseTask();

                this.task = CtkTask.RunLoop(() =>
                {
                    CtkLoggerEventArgs myea;
                    lock (this)
                    {
                        if (!this.queue.TryDequeue(out myea)) return true;//取不出來就下次再取
                    }
                    this.WriteSyn(myea);

                    //若Count等於零, 這個task會結束, IsEnd() = true
                    return this.queue.Count > 0;
                });
            }
            finally { Monitor.Exit(this); }
        }
        protected virtual void WriteSyn(CtkLoggerEventArgs ea)
        {
            this.OnLogWrite(ea);
            OnEveryLogWrite(this, ea);
        }


        public void CloseTask()
        {
            CtkUtil.DisposeTaskTry(this.task);
        }




        #region Event

        /// <summary>
        /// Event Naming: Prepare, Eh, After
        /// </summary>
        public event EventHandler<CtkLoggerEventArgs> EhLogWrite;
        void OnLogWrite(CtkLoggerEventArgs ea)
        {
            if (this.EhLogWrite == null) return;
            this.EhLogWrite(this, ea);
        }
        #endregion



        #region IDisposable
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //

            this.DisposeClose();

            disposed = true;
        }


        public void DisposeClose()
        {
            try { this.CloseTask(); }
            catch (Exception ex) { CtkLog.Write(ex); }
            //斷線不用清除Event, 但Dispsoe需要, 因為即使斷線此物件仍存活著
            CtkEventUtil.RemoveEventHandlersOfOwnerByFilter(this, (dlgt) => true);
        }

        #endregion




        #region Static


        public static event EventHandler<CtkLoggerEventArgs> EhEveryLogWrite;
        static void OnEveryLogWrite(object sender, CtkLoggerEventArgs ea)
        {
            if (EhEveryLogWrite == null) return;
            EhEveryLogWrite(sender, ea);
        }

        #endregion

    }


}
