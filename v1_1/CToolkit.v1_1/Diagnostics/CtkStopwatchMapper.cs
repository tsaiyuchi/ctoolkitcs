using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Diagnostics
{
    public class CtkStopwatchMapper : Dictionary<string, CtkStopwatch>
    {


        public CtkStopwatch Restart(string key = "")
        {
            var sw = this.Get(key);
            sw.Restart();
            return sw;
        }


        public CtkStopwatch Stop(string key = "")
        {
            var sw = this.Get(key);
            sw.Stop();
            return sw;
        }


        public CtkStopwatch Get(string key = "")
        {
            lock (this)
                if (!this.ContainsKey(key))
                    this[key] = new CtkStopwatch();

            return this[key];
        }



        public static CtkStopwatch SRestart(string key = "")
        {
            var sw = SGet(key);
            sw.Restart();
            return sw;
        }
        public static CtkStopwatch SStop(string key = "")
        {
            var sw = SGet(key);
            sw.Stop();
            return sw;
        }
        public static String SMsgWithRestart(String format, bool isRecordHist = false, string key = "")
        {
            var sw = SGet(key);
            return sw.MsgWithRestart(format, isRecordHist);
        }
        public static String SMsgWithStop(String format, bool isRecordHist = false, string key = "")
        {
            var sw = SGet(key);
            return sw.MsgWithStop(format, isRecordHist);
        }
        public static CtkStopwatch SGet(string key = "")
        {
            var me = CtkStopwatchMapper.Singleton;
            lock (me)
                if (!me.ContainsKey(key))
                    me[key] = new CtkStopwatch();

            return me[key];
        }



        static CtkStopwatchMapper m_singleton;
        public static CtkStopwatchMapper Singleton
        {
            get
            {
                if (m_singleton == null)
                    m_singleton = new CtkStopwatchMapper();
                return m_singleton;
            }
        }
    }
}