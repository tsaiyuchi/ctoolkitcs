using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Diagnostics
{
    public class CtkStopwatchMapper : Dictionary<string, CtkStopwatch>
    {


        public void Restart(string key = "")
        {
            var sw = this.Get(key);
            sw.Restart();
        }


        public void Stop(string key = "")
        {
            var sw = this.Get(key);
            sw.Stop();
        }


        public CtkStopwatch Get(string key = "")
        {
            lock (this)
                if (!this.ContainsKey(key))
                    this[key] = new CtkStopwatch();

            return this[key];
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