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

namespace CToolkit.v1_1.Net
{
    public class CtkWebTransaction
    {
        public HttpWebRequest HwRequest;
        public string HwRequestData;
        public Encoding HwRequestEncoding;
        public Encoding HwResponseEncoding;







        #region Static

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

        public static CtkWebTransaction HttpRequestTx(HttpWebRequest wreq, string reqData = null, Encoding reqEncoding = null, Encoding respEncoding = null)
        {
            var rs = new CtkWebTransaction();
            rs.HwRequest = wreq;
            rs.HwRequestData = reqData;
            rs.HwRequestEncoding = reqEncoding;
            rs.HwResponseEncoding = respEncoding;

            return rs;
        }

        public static Regex RegexUrl() { return new Regex(@"^(?<proto>\w+)://[^/]+?(?<port>:\d+)?/", RegexOptions.Compiled); }

        #endregion



    }
}
