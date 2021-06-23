using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Diagnostics
{
    public class CtkStopwatch : System.Diagnostics.Stopwatch
    {
        public string message;


        public CtkStopwatch() { }
        public CtkStopwatch(bool restart) { if (restart) this.Restart(); }

        public String RestartMsg(string format)
        {
            this.Stop();
            var msg = string.Format(format, this.ElapsedMilliseconds);
            message += msg;
            this.Reset();
            this.Start();
            return msg;
        }
        public void StopMsg(string format)
        {
            this.Stop();
            message += string.Format(format, this.ElapsedMilliseconds);
        }




        public void AppendMessage(string format) { this.message += string.Format(format, this.ElapsedMilliseconds); }

        public void Clear() { this.Reset(); this.message = ""; }
        public string GetMessage() { return this.message; }

        public static CtkStopwatch Singleton { get { return CtkStopwatchMapper.Singleton.Get(); } }
    }


}
