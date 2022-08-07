﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CToolkit.v1_2Core.Threading
{
    public class CtkMonitorAutoResetEventBlockerM : IDisposable
    {

        protected CtkMonitorAutoResetEvent _mare;
        public CtkMonitorAutoResetEventBlockerM(CtkMonitorAutoResetEvent mare)
        {
            this._mare = mare;
        }


        public void Close()
        {
            this._mare.TryExit();
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
            this.Close();
        }

        #endregion

    }





}
