using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http.Routing;

namespace System.Web.Mvc
{
    public static class MyUrlHelper
    {
        public static string CusContent(this UrlHelper helper, string value)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return helper.Content(string.Format(value + "?_v={0}", version));
        }
    }
}