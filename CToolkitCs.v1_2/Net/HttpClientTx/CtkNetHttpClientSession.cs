using CToolkitCs.v1_2.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace CToolkitCs.v1_2.Net.HttpClientTx
{
    public class CtkNetHttpClientSession : IDisposable
    {
        CookieContainer cookieContainer;
        HttpClientHandler httpClienHandler;
        List<CtkNetHttpClientTransaction> httpClients;


        public CtkNetHttpClientSession(
            DecompressionMethods AutomaticDecompression = DecompressionMethods.None,
            SslProtocols sslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            bool ignoreSsl = false)
        {
            this.cookieContainer = new CookieContainer();
            this.httpClienHandler = new HttpClientHandler
            {
                CookieContainer = this.cookieContainer,
                UseCookies = true,

                AutomaticDecompression = AutomaticDecompression,
                // 指定使用 TLS 1.2 或 TLS 1.3
                SslProtocols = sslProtocols,
                // 忽略所有的安全憑證錯誤
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => ignoreSsl,
            };
            this.httpClients = new List<CtkNetHttpClientTransaction>();
        }
        ~CtkNetHttpClientSession() { this.Dispose(false); }


        public CtkNetHttpClientTransaction CreateTx()
        {
            var tx = new CtkNetHttpClientTransaction(this.httpClienHandler, false);
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
            CtkUtil.DisposeObjTry(this.httpClienHandler);
            CtkEventUtil.RemoveSubscriberOfObjectByFilter(this, (dlgt) => true);
        }

        #endregion


    }
}
