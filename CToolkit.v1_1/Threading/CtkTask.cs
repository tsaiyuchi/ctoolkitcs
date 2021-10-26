using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CToolkit.v1_1.Threading
{
    public class CtkTask : IDisposable, IAsyncResult
    {
        public string Name;
        public Task Task;
        public CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        public CancellationToken CancelToken { get { return this.CancelTokenSource.Token; } }
        public int Sleep = 0;

        public void Cancel() { this.CancelTokenSource.Cancel(); }


        public TaskStatus Status { get { return this.Task.Status; } }


        public TaskAwaiter GetAwaiter() { return this.Task.GetAwaiter(); }
        public bool IsEnd() { return this.Task == null ? true : this.Task.IsCompleted || this.Task.IsFaulted || this.Task.IsCanceled; }


        public void Start()
        {
            if (this.Task == null) throw new InvalidOperationException("Task尚未設定");
            this.Task.Start();
        }
        public bool Wait(int milliseconds) { return this.Task.Wait(milliseconds); }
        public void Wait() { this.Task.Wait(); }


        #region --- IAsyncResult --- --- ---
        public bool IsCompleted { get { return this.Task.IsCompleted; } }
        public WaitHandle AsyncWaitHandle { get { throw new NotImplementedException(); } }
        public object AsyncState { get { return this.Task.AsyncState; } }
        public bool CompletedSynchronously { get { throw new NotImplementedException(); } }
        #endregion--- --- ---

        #region --- Static --- --- ---

        /// <summary> 執行一次沒機會用 CancelToken </summary>
        public static CtkTask RunOnce(Action act, String name = null)
        {
            var task = new CtkTask();
            task.Task = Task.Factory.StartNew(act);
            task.Name = name;
            return task;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcIsContinue">if return ture then continue</param>
        public static CtkTask RunLoop(Func<bool> funcIsContinue, int sleep = 0)
        {
            var task = new CtkTask();
            task.Sleep = sleep;

            var ct = task.CancelTokenSource.Token;
            task.Task = Task.Factory.StartNew(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!funcIsContinue()) break;
                    if (task.Sleep > 0) Thread.Sleep(task.Sleep);
                }
            }, ct);

            return task;
        }
        public static CtkTask RunLoop(Func<bool> funcIsContinue, string name, int sleep = 0)
        {
            var task = new CtkTask();
            task.Sleep = sleep;

            var ct = task.CancelTokenSource.Token;
            task.Task = Task.Factory.StartNew(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!funcIsContinue()) break;
                    if (task.Sleep > 0) Thread.Sleep(task.Sleep);
                }
            }, ct);
            task.Name = name;
            return task;
        }

        public static CtkTask RunLoop(Func<CancellationToken, bool> funcIsContinue, int sleep = 0)
        {
            var task = new CtkTask();
            task.Sleep = sleep;

            var ct = task.CancelTokenSource.Token;
            task.Task = Task.Factory.StartNew(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!funcIsContinue(ct)) break;
                    if (task.Sleep > 0) Thread.Sleep(task.Sleep);
                }
            }, ct);
            return task;
        }
        public static CtkTask RunLoop(Func<CancellationToken, bool> funcIsContinue, string name, int sleep = 0)
        {
            var task = new CtkTask();
            task.Sleep = sleep;

            var ct = task.CancelTokenSource.Token;
            task.Task = Task.Factory.StartNew(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!funcIsContinue(ct)) break;
                    if (task.Sleep > 0) Thread.Sleep(task.Sleep);
                }
            }, ct);
            task.Name = name;
            return task;
        }
        public static CtkTask RunOnce(Action<CancellationToken> act, int sleep = 0)
        {
            var task = new CtkTask();
            task.Sleep = sleep;

            var ct = task.CancelTokenSource.Token;
            task.Task = Task.Factory.StartNew(() =>
            {
                act(ct);
                if (task.Sleep > 0) Thread.Sleep(task.Sleep);
            }, ct);

            return task;
        }

        #endregion --- --- ---


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
            /* [d20210220]
             * 沒人參考它時,  有機率被釋放
             * 但其參考的 Task, 其實還沒完工
             * 1. 若此 Task 只是沒人參考, 不應強制Dispose
             * 2. 若此 Task 是應用程式結束時, 應被強制關閉
             * 原生Task選擇不關閉, 應自主結束
             * 但這邊考量的是應正確釋放資源
             * 
             * 或許原生才是對的, 你不應強制關閉Task, 
             * (1) 該Task要有自主判斷停止的功能,
             * (2) 應用程式強制關閉時, 其實Task也會被關閉
             * 所以, 若你要用一個不受控的Task, 那不如用原生的
             * 
             * 結論: 其實原生Task 你也沒辦法強制關閉它, 你也只能直接Try/Catch起來
             */



            if (this.Task != null)
            {
                //只增加Cancel的呼叫, 剩的用父類別的
                if (this.Status < TaskStatus.RanToCompletion)
                    this.CancelTokenSource.Cancel();
                //統一Dispose的方法, 有例外仍舊扔出, 確保在預期內
                CtkUtil.DisposeTask(this.Task);
                this.Task = null;
            }
        }

        #endregion

    }
}
