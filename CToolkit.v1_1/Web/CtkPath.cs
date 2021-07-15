using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Web
{
    public class CtkPath
    {

        public static string ResolveClientUrl(string url)
        { return System.Web.VirtualPathUtility.ToAbsolute(url); }

        public static string ResolveUrl(string url)
        { return "http://" + System.Web.HttpContext.Current.Request.Url.DnsSafeHost + System.Web.VirtualPathUtility.ToAbsolute(url); }

    }
}
