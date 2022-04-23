using CToolkit.v1_1.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace CToolkit.v1_1.Logging
{
    public class CtkLogger
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
                if (this.task != null)
                {
                    //若還沒結束執行, 先return
                    if (!this.task.IsEnd()) return;
                    //若之前有, 把它清乾淨
                    using (var obj = this.task)
                        if (!obj.IsEnd())
                            obj.Cancel();
                }


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
