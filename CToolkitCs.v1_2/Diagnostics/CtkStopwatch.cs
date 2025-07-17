using System;
using System.Collections.Generic;

namespace CToolkitCs.v1_2.Diagnostics
{
    public class CtkStopwatch : System.Diagnostics.Stopwatch
    {

        public CtkStopwatch() { }
        ~CtkStopwatch() { }








        #region Singleton


        protected static Dictionary<String, CtkStopwatch> _mapSingleton = new Dictionary<string, CtkStopwatch>();

        public static CtkStopwatch SGetOrCreate(String key = "")
        {
            if (_mapSingleton.ContainsKey(key)) return _mapSingleton[key];
            return _mapSingleton[key] = new CtkStopwatch();
        }

        public static CtkStopwatch SRestart(String key = "")
        {
            var rtn = SGetOrCreate(key);
            rtn.Restart();
            return rtn;
        }
        public static CtkStopwatch SRestart(String key, Action<CtkStopwatch> act)
        {
            var rtn = SGetOrCreate(key);
            rtn.Stop();
            if (act != null) act(rtn);
            rtn.Restart();
            return rtn;
        }



        public static CtkStopwatch SStop(String key = "", Action<CtkStopwatch> act = null)
        {
            var rtn = SGetOrCreate(key);
            rtn.Stop();
            if (act != null) act(rtn);
            return rtn;
        }





        #endregion




    }


}
