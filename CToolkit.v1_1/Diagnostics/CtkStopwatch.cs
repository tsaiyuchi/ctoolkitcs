using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Diagnostics
{
    public class CtkStopwatch : System.Diagnostics.Stopwatch
    {
        public List<String> HistoryMessage = new List<string>();


        public CtkStopwatch() { }
        ~CtkStopwatch() { this.Clear(); }
        public CtkStopwatch(bool restart) { if (restart) this.Restart(); }

        public String RestartMsg(string format)
        {
            this.Stop();
            var msg = string.Format(format, this.ElapsedMilliseconds);
            this.HistoryMessage.Add(msg);
            this.Reset();
            this.Start();
            return msg;
        }
        public String StopMsg(string format)
        {
            this.Stop();
            var msg = string.Format(format, this.ElapsedMilliseconds);
            this.HistoryMessage.Add(msg);
            return msg;
        }




        public void AppendMessage(string format) { this.HistoryMessage.Add(string.Format(format, this.ElapsedMilliseconds)); }

        public void Clear()
        {
            this.Reset();
            this.HistoryMessage.Clear();
        }
        public string GetMessage(String separator = "\r\n") { return String.Join(separator, this.HistoryMessage); }

        public static CtkStopwatch Singleton { get { return CtkStopwatchMapper.Singleton.Get(); } }
    }


}
