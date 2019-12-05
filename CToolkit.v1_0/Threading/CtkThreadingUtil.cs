using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Threading
{
    public class CtkThreadingUtil
    {
        public static BackgroundWorker RunWorkerAsyn(Action dlgt)
        {
            return RunWorkerAsyn(delegate (object sender, DoWorkEventArgs e)
            {
                dlgt.DynamicInvoke();
            });
        }

        public static BackgroundWorker RunWorkerAsyn(DoWorkEventHandler work)
        {
            var bgworker = new BackgroundWorker();
            bgworker.WorkerSupportsCancellation = true;
            bgworker.DoWork += work;
            bgworker.RunWorkerAsync();
            return bgworker;
        }


    }
}
