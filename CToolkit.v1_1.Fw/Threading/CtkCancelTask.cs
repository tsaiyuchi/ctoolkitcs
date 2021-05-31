using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_1.Threading
{
    public class CtkCancelTask : CtkTask
    {
        public CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        public CancellationToken CancelToken { get { return this.CancelTokenSource.Token; } }

        public void Cancel() { this.CancelTokenSource.Cancel(); }

        #region IDisposable

        protected override void DisposeSelf()
        {
            if (this.Task != null)
            {
                //只增加Cancel的呼叫, 剩的用父類別的
                if (this.Status < TaskStatus.RanToCompletion)
                    this.CancelTokenSource.Cancel();
                base.DisposeSelf();
            }
        }

        #endregion


        #region Static

        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcIsContinue">if return ture then continue</param>
        /// <returns></returns>
        public static CtkCancelTask RunLoop(Func<bool> funcIsContinue)
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

        public static CtkCancelTask RunLoop(Func<bool> funcIsContinue, string name)
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
            task.Name = name;
            return task;
        }



        public static CtkCancelTask RunOnce(Action<CancellationToken> act)
        {
            var task = new CtkCancelTask();
            var ct = task.CancelTokenSource.Token;
            task.Task = Task.Factory.StartNew(() =>
            {
                act(ct);
            }, ct);

            return task;
        }

        #endregion


    }
}
