using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Text;

namespace CToolkitCs.v1_2.Net.HttpTx
{
    public class CtkNetHttpSession : IDisposable
    {
        public CookieContainer CookieContainer = new CookieContainer();
        public List<CtkNetHttpTransaction> Transaction = new List<CtkNetHttpTransaction>();




        public CtkNetHttpTransaction CreateTx(String url)
        {
            var tx = CtkNetHttpTransaction.Create(url);
            tx.HwRequest.CookieContainer = this.CookieContainer;
            this.Transaction.Add(tx);
            return tx;
        }
        public CtkNetHttpTransaction CreateTx(String url, RequestCacheLevel cachePolicy, String httpMethod = "GET")
        {
            var tx = CtkNetHttpTransaction.Create(url, cachePolicy, httpMethod);
            var hwreq = tx.HwRequest;
            hwreq.CookieContainer = this.CookieContainer;

            this.Transaction.Add(tx);
            return tx;
        }


        public CtkNetHttpTransaction CreateTxHttpGet(String url, RequestCacheLevel cachePolicy = RequestCacheLevel.Default) { return this.CreateTx(url, cachePolicy, "GET"); }
        public CtkNetHttpTransaction CreateTxHttpPost(String url, RequestCacheLevel cachePolicy = RequestCacheLevel.Default) { return this.CreateTx(url, cachePolicy, "POST"); }

        public String HttpGet(String url, RequestCacheLevel cachePolicy = RequestCacheLevel.Default)
        {
            using (var tx = this.CreateTx(url, cachePolicy, "GET"))
                return tx.GetHwResponseData();
        }
        public String HttpPost(String url, RequestCacheLevel cachePolicy = RequestCacheLevel.Default)
        {
            using (var tx = this.CreateTx(url, cachePolicy, "POST"))
                return tx.GetHwResponseData();
        }





        #region IDisposable
        // Flag: Has Dispose already been called?
        protected bool disposed = false;
        ~CtkNetHttpSession() { this.Dispose(false); }
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void DisposeClose()
        {
            try
            {
                foreach (var tx in this.Transaction)
                {
                    try { tx.Dispose(); }
                    catch (Exception) { }
                }
            }
            catch (Exception ex) { CtkLog.Warn(ex); }

            //斷線不用清除Event, 但Dispsoe需要, 因為即使斷線此物件仍存活著
            CtkEventUtil.RemoveSubscriberOfObjectByFilter(this, (dlgt) => true);
        }

        // Protected implementation of Dispose pattern.
        protected void Dispose(bool disposing)
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

            this.DisposeClose();

            disposed = true;
        }
        #endregion

    }
}
