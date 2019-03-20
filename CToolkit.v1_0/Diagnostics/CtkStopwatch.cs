using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Diagnostics
{
    public class CtkStopwatch : System.Diagnostics.Stopwatch
    {
        string message;


        public CtkStopwatch() { }
        public CtkStopwatch(bool restart) { if (restart) this.Restart(); }

        public void RestartMsg(string format)
        {
            this.Stop();
            message += string.Format(format, this.ElapsedMilliseconds);
            this.Reset();
            this.Start();
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
