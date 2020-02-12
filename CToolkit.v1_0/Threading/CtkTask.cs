using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CToolkit.v1_0.Threading
{
    public class CtkTask : IDisposable
    {
        public string Name;
        public Task Task;


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
            if (this.Task != null)
            {
                try { using (var obj = this.Task) { } }
                catch (InvalidOperationException) { }
            }
        }
        #endregion

    }
}
