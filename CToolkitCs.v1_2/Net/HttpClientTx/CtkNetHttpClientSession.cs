using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CToolkitCs.v1_2.Threading;

namespace CToolkitCs.v1_2.Net.HttpClientTx
{
    public class CtkNetHttpClientSession:IDisposable
    {
        CookieContainer cookieContainer;
        HttpClientHandler httpClienHandler;
        List<CtkNetHttpClientTransaction> httpClients;


        public CtkNetHttpClientSession()
        {
            this.cookieContainer = new CookieContainer();
            this.httpClienHandler = new HttpClientHandler
            {
                CookieContainer = this.cookieContainer,
                UseCookies = true,
            };
            this.httpClients = new List<CtkNetHttpClientTransaction>();
        }
        ~CtkNetHttpClientSession() { this.Dispose(false); }


        public CtkNetHttpClientTransaction CreateTx()
        {
            var tx = new CtkNetHttpClientTransaction(this.httpClienHandler);
            this.httpClients.Add(tx);
            return tx;
        }


        #region IDisposable
        // Flag: Has Dispose already been called?
        protected bool disposed = false;
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
        void DisposeClose()
        {
            CtkUtil.DisposeObjTry(this.httpClients);
            CtkEventUtil.RemoveSubscriberOfObjectByFilter(this, (dlgt) => true);
        }

        #endregion


    }
}
