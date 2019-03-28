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


        public bool IsEnd() { return this.Task == null ? true : this.Task.IsCanceled || this.Task.IsFaulted || this.Task.IsCanceled; }


        public void Start()
        {
            if (this.Task == null) throw new InvalidOperationException("Task尚未設定");
            this.Task.Start();
        }


        public static CtkTask Run(Action act)
        {
            var task = new CtkTask();
            task.Task = Task.Factory.StartNew(act);
            return task;
        }


        public bool Wait(int milliseconds) { return this.Task.Wait(milliseconds); }


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
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();
            this.DisposeSelf();
            disposed = true;
        }



        protected virtual void DisposeManaged()
        {
        }
        protected virtual void DisposeUnmanaged()
        {

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
