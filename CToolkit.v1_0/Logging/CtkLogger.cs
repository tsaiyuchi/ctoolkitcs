using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace CToolkit.v1_0.Logging
{
    public class CtkLogger
    {
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

        public virtual void Verbose(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Verbose); }
        public virtual void Debug(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Debug); }
        public virtual void Info(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Info); }
        public virtual void Warn(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Warn); }
        public virtual void Error(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Error); }
        public virtual void Fatal(string msg, params object[] args) { this.Write(string.Format(msg, args), CtkLoggerEnumLevel.Fatal); }




        public virtual void WriteSyn(CtkLoggerEventArgs ea, CtkLoggerEnumLevel _level = CtkLoggerEnumLevel.Info)
        {
            this.OnLogWrite(ea);
            OnEveryLogWrite(this, ea);
        }

        public virtual void WriteAsyn(CtkLoggerEventArgs ea, CtkLoggerEnumLevel _level = CtkLoggerEnumLevel.Info)
        {
            CtkUtil.RunWorkerAsyn(delegate (object sender, DoWorkEventArgs e)
           {
               this.OnLogWrite(ea);
           });
        }


        #region Event
        public event EventHandler<CtkLoggerEventArgs> evtLogWrite;
        void OnLogWrite(CtkLoggerEventArgs ea)
        {
            if (this.evtLogWrite == null) return;
            this.evtLogWrite(this, ea);
        }
        #endregion


        #region Static


        public static event EventHandler<CtkLoggerEventArgs> evtEveryLogWrite;
        static void OnEveryLogWrite(object sender, CtkLoggerEventArgs ea)
        {
            if (evtEveryLogWrite == null) return;
            evtEveryLogWrite(sender, ea);
        }

        #endregion

    }
}
