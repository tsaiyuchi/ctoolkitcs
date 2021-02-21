using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CToolkit.v1_1.Threading
{
    public class CtkTask : IDisposable
    {
        public string Name;
        public Task Task;

        public TaskStatus Status { get { return this.Task.Status; } }

        public static CtkTask Run(Action act)
        {
            var task = new CtkTask();
            task.Task = Task.Factory.StartNew(act);
            return task;
        }

        public bool IsEnd() { return this.Task == null ? true : this.Task.IsCompleted || this.Task.IsFaulted || this.Task.IsCanceled; }


        public void Start()
        {
            if (this.Task == null) throw new InvalidOperationException("Task尚未設定");
            this.Task.Start();
        }
        public bool Wait(int milliseconds) { return this.Task.Wait(milliseconds); }
        public void Wait() { this.Task.Wait(); }


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
                //統一Dispose的方法, 有例外仍舊扔出, 確保在預期內
                CtkUtilFw.DisposeTask(this.Task);
                this.Task = null;
            }
        }

        #endregion

    }
}
