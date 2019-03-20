using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_0.Threading
{
    public class CtkCancelTask : CtkTask
    {
        public CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        public CancellationToken CancelToken { get { return this.CancelTokenSource.Token; } }

        public void Cancel() { this.CancelTokenSource.Cancel(); }

        public static CtkCancelTask RunLoopUntilCancel(Func<bool> funcIsContinue)
        {
            var task = new CtkCancelTask();
            var ct = task.CancelTokenSource.Token;
            task.Task = Task.Factory.StartNew(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!funcIsContinue()) break;
                }
            }, ct);

            return task;
        }


        public static CtkCancelTask Run(Action<CancellationToken> act)
        {
            var task = new CtkCancelTask();
            var ct = task.CancelTokenSource.Token;
            task.Task = Task.Factory.StartNew(() =>
            {
                act(ct);
            }, ct);

            return task;
        }



        #region IDisposable

        protected override void DisposeSelf()
        {
            this.CancelTokenSource.Cancel();
            this.Task.Wait(1000);
            base.DisposeSelf();
        }
        #endregion

    }
}
