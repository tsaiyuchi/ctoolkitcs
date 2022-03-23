using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Data;
using System.IO.Compression;
using System.IO;
using CToolkit.v1_1.Compress;
using System.Threading.Tasks;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CToolkit.v1_1.Net.WebTx
{
    public class CtkNetWebTransaction
    {
        public HttpWebRequest HwRequest;
        public string HwRequestData;
        public Encoding HwRequestEncoding;
        public Encoding HwResponseEncoding;







        #region === Static === === ===

        public static String HttpGet(string uri, System.Net.Cache.RequestCacheLevel cachePolicy) { return HttpGet(new Uri(uri), cachePolicy); }
        public static String HttpGet(Uri uri, System.Net.Cache.RequestCacheLevel cachePolicy)
        {
            WebRequest wreq = WebRequest.Create(uri);
            wreq.CachePolicy = new System.Net.Cache.RequestCachePolicy(cachePolicy);
            using (var wresp = wreq.GetResponse())
            using (var wrespStream = wresp.GetResponseStream())
            using (var reader = new System.IO.StreamReader(wrespStream))
                return reader.ReadToEnd();
        }
        public static String HttpGet(string uri, Encoding encoding = null) { return HttpGet(new Uri(uri), encoding); }
        public static String HttpGet(Uri uri, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var wreq = WebRequest.Create(uri);
            using (var wresp = wreq.GetResponse())
            using (var wrespStream = wresp.GetResponseStream())
            using (var reader = new System.IO.StreamReader(wrespStream, encoding))
                return reader.ReadToEnd();

        }

        public static String HttpPost(String uri, Dictionary<string, object> postData, Encoding reqEncoding = null)
        {
            var list = new List<string>();
            foreach (var kv in postData)
            {
                var param = string.Format("{0}={1}", kv.Key, Uri.EscapeDataString(Convert.ToString(kv.Value)));
                list.Add(param);
            }
            var post = string.Join("&", list.ToArray());

            return HttpPost(uri, post, reqEncoding);

        }
        public static String HttpPost(String uri, String post, Encoding reqEncoding = null)
        {
            if (reqEncoding == null) reqEncoding = Encoding.UTF8;
            byte[] byteData = reqEncoding.GetBytes(post);

            HttpWebRequest wreq = null;
            System.IO.Stream reqstm = null;
            HttpWebResponse wresp = null;
            System.IO.StreamReader reader = null;
            try
            {
                wreq = (HttpWebRequest)WebRequest.Create(uri);
                wreq.Method = "POST";
                wreq.ContentType = "application/x-www-form-urlencoded";
                wreq.ContentLength = byteData.Length;
                //request.Credentials = new NetworkCredential("xx", "xx"); 
                reqstm = wreq.GetRequestStream();
                reqstm.Write(byteData, 0, byteData.Length);
                wresp = (HttpWebResponse)wreq.GetResponse();
                //string responseStatus = response.StatusDescription;

                using (var wrespStream = wresp.GetResponseStream())
                {
                    reader = new System.IO.StreamReader(wrespStream);
                    return reader.ReadToEnd();
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); reader.Dispose(); }
                if (wresp != null) { wresp.Close(); }
                if (reqstm != null) { reqstm.Close(); }
            }
        }

        public static string HttpRequest(HttpWebRequest wreq, string reqData = null, Encoding reqEncoding = null, Encoding respEncoding = null)
        {
            if (reqEncoding == null) reqEncoding = Encoding.UTF8;


            if (string.Compare(wreq.Method, "POST", true) == 0)
            {
                if (reqData == null) reqData = "";
                var byteData = reqEncoding.GetBytes(reqData);
                wreq.ContentLength = byteData.Length;
                using (var reqstm = wreq.GetRequestStream())
                    reqstm.Write(byteData, 0, byteData.Length);
            }

            

            using (var wresp = (HttpWebResponse)wreq.GetResponse())
            {
                if (respEncoding == null && !string.IsNullOrEmpty(wresp.CharacterSet))
                {
                    try { respEncoding = Encoding.GetEncoding(wresp.CharacterSet); }
                    catch (Exception) { }
                }
                if (respEncoding == null) { respEncoding = Encoding.UTF8; }

                using (var wrespStream = wresp.GetResponseStream())
                using (var reader = new System.IO.StreamReader(wrespStream, respEncoding))
                    return reader.ReadToEnd();
            }
        }
        public static string HttpRequestGZip(HttpWebRequest wreq, string reqData = null, Encoding reqEncoding = null, Encoding respEncoding = null)
        {
            if (reqEncoding == null) reqEncoding = Encoding.UTF8;


            if (string.Compare(wreq.Method, "POST", true) == 0)
            {
                if (reqData == null) reqData = "";
                var byteData = reqEncoding.GetBytes(reqData);
                wreq.ContentLength = byteData.Length;
                using (var reqstm = wreq.GetRequestStream())
                    reqstm.Write(byteData, 0, byteData.Length);
            }

            using (var wresp = (HttpWebResponse)wreq.GetResponse())
            {
                if (respEncoding == null && !string.IsNullOrEmpty(wresp.CharacterSet))
                {
                    try { respEncoding = Encoding.GetEncoding(wresp.CharacterSet); }
                    catch (Exception) { }
                }
                if (respEncoding == null) { respEncoding = Encoding.UTF8; }




                using (var wrespStream = wresp.GetResponseStream())
                using (var memStream = new MemoryStream())
                using (var fs = File.Open("d:/temp/abc.xls", FileMode.Create))
                {


                    var buffer = new byte[1024];
                    var cnt = 0;
                    do
                    {
                        cnt = wrespStream.Read(buffer, 0, buffer.Length);
                        if (cnt == 0) break;
                        memStream.Write(buffer, 0, cnt);
                        fs.Write(buffer, 0, cnt);
                    } while (cnt > 0);


                    memStream.Position = 0;
                    memStream.Read(buffer, 0, 8);
                    memStream.Position = 0;



                    if (CtkFileFormat.IsGZip(buffer))
                    {
                        using (var gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
                        using (var reader = new StreamReader(gzipStream, respEncoding))
                            return reader.ReadToEnd();
                    }
                    else
                    {
                        using (var reader = new StreamReader(memStream, respEncoding))
                            return reader.ReadToEnd();

                    }


                }

            }
        }

        public static CtkNetWebTransaction HttpRequestTx(HttpWebRequest wreq, string reqData = null, Encoding reqEncoding = null, Encoding respEncoding = null)
        {
            var rs = new CtkNetWebTransaction();
            rs.HwRequest = wreq;
            rs.HwRequestData = reqData;
            rs.HwRequestEncoding = reqEncoding;
            rs.HwResponseEncoding = respEncoding;

            return rs;
        }

        public static Regex RegexUrl() { return new Regex(@"^(?<proto>\w+)://[^/]+?(?<port>:\d+)?/", RegexOptions.Compiled); }




        #region Selenium


        public static async Task<CtkNetHttpGetRtn<IWebDriver>> SeleniumChromeHttpGetAsyn(String uri,
            Func<IWebDriver, bool> callback = null,
            int timeout = 30 * 1000, int delayBrowserOpen = 5 * 1000)
        {
            var rtn = new CtkNetHttpGetRtn<IWebDriver>();

            var start = DateTime.Now;

            ChromeOptions options = new ChromeOptions();
            options.AddArgument(string.Format("user-agent={0}", CtkNetUserAgents.Random().UserAgent));
            using (var driver = rtn.Driver = new ChromeDriver(options))
            {
                //開啟網頁
                driver.Navigate().GoToUrl(uri);
                //隱式等待 - 直到畫面跑出資料才往下執行, 只需宣告一次, 之後找元件都等待同樣秒數.
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(delayBrowserOpen);





                rtn.Html = await Task.Run<string>(() =>
                {
                    var interval = 500;

                    for (int idx = 0; (DateTime.Now - start).TotalMilliseconds < timeout; idx++)
                    {
                        if (string.IsNullOrEmpty(driver.PageSource))
                        {//等頁面載入完成
                            Thread.Sleep(interval);
                            continue;
                        }
                        if (callback != null && !callback(driver))
                        {//有callback 要等callback完成
                            Thread.Sleep(interval);
                            continue;
                        }

                        return driver.PageSource;
                    }
                    return null;
                });




                driver.Quit();
            }
            return rtn;


            ////輸入帳號
            //IWebElement inputAccount = driver.FindElement(By.Name("Account"));
            //Thread.Sleep(2000);
            ////清除按鈕
            //inputAccount.Clear();
            //Thread.Sleep(2000);
            //inputAccount.SendKeys("20180513");
            //Thread.Sleep(2000);

            ////輸入密碼
            //IWebElement inputPassword = driver.FindElement(By.Name("Passwrod"));

            //inputPassword.Clear();
            //Thread.Sleep(2000);
            //inputPassword.SendKeys("123456");
            //Thread.Sleep(2000);

            ////點擊執行
            //IWebElement submitButton = driver.FindElement(By.XPath("/html/body/div[2]/form/table/tbody/tr[4]/td[2]/input"));
            //Thread.Sleep(2000);
            //submitButton.Click();
            //Thread.Sleep(2000);


        }





        #endregion

        #endregion

    }
}
