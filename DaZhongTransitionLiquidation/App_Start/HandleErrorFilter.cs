using DaZhongTransitionLiquidation.Common;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation
{
    /// <summary>
    /// 自定义异常过滤器
    /// </summary>
    public class HandleErrorFilter : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            LogHelper.WriteLog(filterContext.Exception.ToString());
            filterContext.HttpContext.Response.Redirect("/Shared/Error404");
        }
    }
}