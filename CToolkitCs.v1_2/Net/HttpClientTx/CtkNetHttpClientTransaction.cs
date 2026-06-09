using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CToolkitCs.v1_2.Net.HttpClientTx
{
    /* Descrition
     * 官方建議 HttpClient 物件要重複使用，不用每次都 new 一個新的
     */


    public class CtkNetHttpClientTransaction : IDisposable
    {

        public CtkNetHttpClientTransaction(HttpClientHandler handler = null, bool disposeHandler = true)
        {
            this.HttpClient = new HttpClient(handler, disposeHandler);
        }

        ~CtkNetHttpClientTransaction() { this.Dispose(false); }

        public HttpClient HttpClient { get; protected set; }


        public string HttpGetResp(string uri, Dictionary<string, string> parameters = null)
        {
            var builder = new UriBuilder(uri);
            var query = System.Web.HttpUtility.ParseQueryString(builder.Query);
            if (parameters != null)
                foreach (var kvp in parameters)
                    query[kvp.Key] = kvp.Value;
            builder.Query = query.ToString();
            var url = builder.ToString();

            var httpResp = this.HttpClient.GetStringAsync(url);
            httpResp.Wait();
            return httpResp.Result;
        }

        public string HttpPostResp(string uri, string formData)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(formData)) return null;
            var pairs = formData.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', 2); // 只分成兩段，避免 value 裡面還有 '=' 的情況
                var key = HttpUtility.UrlDecode(kv[0]);
                var value = kv.Length > 1 ? HttpUtility.UrlDecode(kv[1]) : "";
                dict[key] = value;
            }
            return this.HttpPostResp(uri, dict);
        }
        public string HttpPostResp(string uri, Dictionary<string, string> formData)
        {
            var content = new FormUrlEncodedContent(formData); //application/x-www-form-urlencoded
            var httpResp = this.HttpClient.PostAsync(uri, content);
            httpResp.Wait();
            var result = httpResp.Result;

            result.EnsureSuccessStatusCode();
            var httpContent = result.Content.ReadAsStringAsync();
            httpContent.Wait();

            // 若伺服器明確指定 charset，可用此處理方式確保解碼正確
            var mediaType = result.Content.Headers.ContentType?.MediaType ?? "";
            var charset = result.Content.Headers.ContentType?.CharSet ?? "utf-8";
            if (!string.IsNullOrEmpty(mediaType) && mediaType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                var bytes = result.Content.ReadAsByteArrayAsync().Result;
                return Encoding.GetEncoding(charset).GetString(bytes);
            }

            return httpContent.Result;
        }





        public string HttpPostRespJson(string uri, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpResp = this.HttpClient.PostAsync(uri, content);
            httpResp.Wait();
            var result = httpResp.Result;

            result.EnsureSuccessStatusCode(); // 確保 HTTP 200-299

            var httpContent = result.Content.ReadAsStringAsync();
            httpContent.Wait();
            return httpContent.Result;
        }






        #region Header Setting
        public void HeaderSetAccept(params string[] values)
        {
            var client = this.HttpClient;
            client.DefaultRequestHeaders.Accept.Clear();
            foreach (var val in values)
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(val));
        }
        public void HeaderSetAcceptEncoding(params string[] values)
        {
            var client = this.HttpClient;
            client.DefaultRequestHeaders.AcceptEncoding.Clear();
            foreach (var val in values)
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(val));
        }
        public void HeaderSetAllAcceptEncoding(string value)
        {
            var client = this.HttpClient;
            client.DefaultRequestHeaders.AcceptEncoding.Clear();

            foreach (var part in value.Split(','))
            {
                if (StringWithQualityHeaderValue.TryParse(part.Trim(), out var encoding))
                    client.DefaultRequestHeaders.AcceptEncoding.Add(encoding);
            }
        }
        public void HeaderSetAllAcceptLanguage(string value)
        {
            var client = this.HttpClient;
            client.DefaultRequestHeaders.AcceptLanguage.Clear();
            foreach (var part in value.Split(','))
            {
                if (StringWithQualityHeaderValue.TryParse(part.Trim(), out var lang))
                {
                    client.DefaultRequestHeaders.AcceptLanguage.Add(lang);
                }
            }
        }
        /// <summary> ConnectionClose = false</summary>
        public void HeaderSetConnectionClose(bool value = false)
        {
            var client = this.HttpClient;
            client.DefaultRequestHeaders.ConnectionClose = value;
        }
        public void HeaderSetHost(string host)
        {
            var client = this.HttpClient;
            client.DefaultRequestHeaders.Host = host;
        }
        /// <summary> 強制使用 keep-alive (預設就是 keep-alive) </summary>
        public void HeaderSetKeepAlive(bool value = true) { this.HeaderSetConnectionClose(!value); }
        public void HeaderSetOrigin(string origin)
        {
            var client = this.HttpClient;
            client.DefaultRequestHeaders.Add("Origin", origin);
        }
        public void HeaderSetUserAgent(string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36")
        {
            var client = this.HttpClient;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        public void HeaderSetReferer(string referer)
        {
            var client = this.HttpClient;
            client.DefaultRequestHeaders.Referrer =  new Uri(referer);
        }

        #endregion



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
            CtkUtil.DisposeObjTry(this.HttpClient);
            CtkEventUtil.RemoveSubscriberOfObjectByFilter(this, (dlgt) => true);
        }

        #endregion




    }
}
