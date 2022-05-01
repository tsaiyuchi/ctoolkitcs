using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_2Core.Threading
{
    public class CtkMonitorResetEvent : IDisposable
    {
        /*[d20220424] Monitor.Xxx 盡量不要卡太久.
         * 一是影響效能, 二是容易有死鎖.
         * 因此利用 Monitor 去 Set/Reset Event
         */
        /*[d20220501] 
         * 若使用ManualResetEvent, 才需要搭配Monitor去阻擋同時通過兩個以上.
         * 若使用AutoResetEvent, 本身就只會放行一個, 不用搭配Monitor. 若以防萬一, 還是可用一下Monitor
         */
        /*[d20220502]
         * 若使用ManualResetEvent, WaitOne以後要自己Reset, 避免後面Thread跟著過去.
         * 若使用AutoResetEvent, WaitOne以後會自動Reset, 只放行一個
         */



        /// <summary> 使用AutoResetEvent, 放了一個Thread後會自動再堵起來 </summary>
        protected AutoResetEvent resetEvent = new AutoResetEvent(true);
        public virtual CtkMonitorResetEventLocker OnceLocker()
        {
            if (!this.TryEnter()) return null;
            return new CtkMonitorResetEventLocker(this);
        }

        public virtual bool TryEnter(int waitTimeMs = 0, int tryGetLockTimeMs = 10, int tryResetTimeMs = 10, int restTimeMs = 10)
        {
            var entryTime = DateTime.Now;

            //waitTimeMs <= 0 代表無限等待
            while (waitTimeMs <= 0 || (DateTime.Now - entryTime).TotalMilliseconds < waitTimeMs)
            {
                try
                {//不做大範圍Lock, 只對關鍵的
                    var isGetLock = false;
                    if (tryGetLockTimeMs > 0)
                        isGetLock = Monitor.TryEnter(this, tryGetLockTimeMs);
                    else
                        isGetLock = Monitor.TryEnter(this);


                    if (isGetLock)
                    {
                        if (tryResetTimeMs <= 0)
                            throw new ArgumentException("if you can get locker then another cannot, reset time <= 0 will cause dead lock");

                        if (this.resetEvent.WaitOne(tryResetTimeMs))
                        {
                            //AutoResetEvent會自動Reset, 此行不一定要執行, 只是以防萬一
                            this.resetEvent.Reset();//先堵住, 不讓後面的再次進行
                            return true;
                        }
                    }
                }
                finally { if (Monitor.IsEntered(this)) Monitor.Exit(this); }
                //解鎖後再等待
                Thread.Sleep(restTimeMs);
            }
            return false;
        }
        public virtual bool TryExit(int waitTimeMs = 0, int tryGetLockTimeMs = 10)
        {
            var entryTime = DateTime.Now;

            //waitTimeMs <= 0 代表無限等待
            while (waitTimeMs <= 0 || (DateTime.Now - entryTime).TotalMilliseconds < waitTimeMs)
            {
                try
                {
                    //不做大範圍Lock, 只對關鍵的
                    var isGetLock = false;
                    if (tryGetLockTimeMs > 0)
                        isGetLock = Monitor.TryEnter(this, tryGetLockTimeMs);
                    else
                        isGetLock = Monitor.TryEnter(this);

                    if (isGetLock)
                        this.resetEvent.Set();
                }
                finally { if (Monitor.IsEntered(this)) Monitor.Exit(this); }
            }
            return false;
        }



        #region IDisposable

        // Flag: Has Dispose already been called?
        protected bool disposed = false;
        // Public implementation of Dispose pattern callable by consumers.
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            this.DisposeSelf();
            disposed = true;
        }

        protected virtual void DisposeSelf()
        {
            if (this.resetEvent != null)
                this.resetEvent.Dispose();
        }

        #endregion




    }
}
