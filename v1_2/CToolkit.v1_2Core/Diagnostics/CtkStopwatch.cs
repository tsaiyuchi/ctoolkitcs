using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_2Core.Diagnostics
{
    public class CtkStopwatch : System.Diagnostics.Stopwatch
    {
        public List<String> HistoryMessage = new List<string>();


        public CtkStopwatch() { }
        ~CtkStopwatch() { this.Clear(); }
        public CtkStopwatch(bool restart) { if (restart) this.Restart(); }

        public String MsgWithRestart(string format, bool isRecordToHist = false)
        {
            this.Stop();
            var msg = string.Format(format, this.ElapsedMilliseconds);
            if (isRecordToHist) this.HistoryMessage.Add(msg);
            this.Reset();
            this.Start();
            return msg;
        }
        public String MsgWithStop(string format, bool isRecordToHist = false)
        {
            this.Stop();
            var msg = string.Format(format, this.ElapsedMilliseconds);
            if (isRecordToHist) this.HistoryMessage.Add(msg);
            return msg;
        }




        public void AppendMessage(string format) { this.HistoryMessage.Add(string.Format(format, this.ElapsedMilliseconds)); }

        public void Clear()
        {
            this.Reset();
            this.HistoryMessage.Clear();
        }
        public string GetMessage(String separator = "\r\n") { return String.Join(separator, this.HistoryMessage); }

    }


}
