using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_1.Threading
{
    public class CtkMonitorResetEvent
    {
        protected ManualResetEvent Mre = new ManualResetEvent(false);


        public bool WaitOne()
        {
            return this.Mre.WaitOne();
        }
        public bool WaitOne(int millisecondsTimeout)
        {
            return this.Mre.WaitOne(millisecondsTimeout);
        }



        public bool TryEnterWaitReset(int monitorMillisecond = 0, int waitMillisecondsTimeout = 10)
        {
            try
            {
                if (!Monitor.TryEnter(this, monitorMillisecond)) return false;//進不去先離開
                if (!this.Mre.WaitOne(waitMillisecondsTimeout)) return false;//有人處理中, 先離開
                this.Mre.Reset();//先卡住, 不讓後面的再次進行
            }
            finally { if (Monitor.IsEntered(this)) Monitor.Exit(this); }
            return true;
        }

        public bool TryEnterSet(int monitorMillisecond = 0)
        {
            try
            {
                if (!Monitor.TryEnter(this, monitorMillisecond)) return false;//進不去先離開
                this.Mre.Set();
            }
            finally { if (Monitor.IsEntered(this)) Monitor.Exit(this); }
            return true;
        }


    }
}
