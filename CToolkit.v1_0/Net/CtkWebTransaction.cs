using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Data;

namespace CToolkit.v1_0.Net
{
    public class CtkWebTransaction
    {



        //=== Static =================================================================================





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
            System.Net.WebRequest wreq = WebRequest.Create(uri);
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

        public static DataSet HttpPostToDataSet(String uri, String post)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteData = encoding.GetBytes(post);

            HttpWebRequest request = null;
            System.IO.Stream requestStream = null;
            HttpWebResponse wresp = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteData.Length;
                //request.Credentials = new NetworkCredential("xx", "xx"); 
                requestStream = request.GetRequestStream();
                requestStream.Write(byteData, 0, byteData.Length);
                wresp = (HttpWebResponse)request.GetResponse();
                //string responseStatus = response.StatusDescription;

                var ds = new DataSet();
                using (var wrespStream = wresp.GetResponseStream())
                    ds.ReadXml(wrespStream);
                return ds;
            }
            finally
            {
                if (wresp != null) { wresp.Close(); }
                if (requestStream != null) { requestStream.Close(); }
            }
        }

        public static string HttpRequest(HttpWebRequest wreq, string reqData, Encoding reqEncoding, Encoding respEncoding)
        {

            if (string.Compare(wreq.Method, "POST", true) == 0)
            {
                if (reqData == null) reqData = "";
                var byteData = reqEncoding.GetBytes(reqData);
                wreq.ContentLength = byteData.Length;
                using (var reqstm = wreq.GetRequestStream())
                    reqstm.Write(byteData, 0, byteData.Length);
            }

            using (var wresp = (HttpWebResponse)wreq.GetResponse())
            using (var wrespStream = wresp.GetResponseStream())
            using (var reader = new System.IO.StreamReader(wrespStream, respEncoding))
                return reader.ReadToEnd();
        }


        public static Regex RegexUrl() { return new Regex(@"^(?<proto>\w+)://[^/]+?(?<port>:\d+)?/", RegexOptions.Compiled); }
    }
}
