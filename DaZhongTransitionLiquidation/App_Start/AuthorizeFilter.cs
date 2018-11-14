using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation
{
    /// <summary>
    /// 自定义权限过滤器
    /// </summary>
    public class AuthorizeFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var cm = CookiesManager<string>.GetInstance();
            if (!cm.ContainsKey(PubConst.CostCache) || cm[PubConst.CostCache].IsNullOrEmpty())
            {
                cm.Add(PubConst.CostCache, Guid.NewGuid().ToString(), cm.Day * 365);
            }
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
            var noCheckControllers = new List<string>()  //无需验证的控制器
            {
                "login","shared","api"
            };
            if (noCheckControllers.Contains(controllerName))
            {
                return;
            }
           CheckAuthorizaiton(filterContext);
        }

        /// <summary>
        /// 防止从地址栏输入url直接访问无权限的页面
        /// </summary>
        /// <param name="filterContext"></param>
        private void CheckAuthorizaiton(AuthorizationContext filterContext)
        {
           // string control = filterContext.RouteData.Values["controller"].ToString();
           // if (control.ToLower()!= "newapi")
           // {
                var cache = CacheManager<Sys_User>.GetInstance();
                //验证是否登录
                if (!cache.ContainsKey(PubGet.GetUserKey) || cache[PubGet.GetUserKey].IsNullOrEmpty())
                {
                    filterContext.Result = new RedirectResult("/Login/Index");
                    return;
                }
                if (cache[PubGet.GetUserKey].LoginName.ToLower() == "sysadmin")  //系统管理员不做验证
                {
                    return;
                }
                var cmRole = CacheManager<List<U_RoleModule>>.GetInstance();
                var roleModules = cmRole[cache[PubGet.GetUserKey].Vguid.ToString()];
                cmRole.Remove(cache[PubGet.GetUserKey].Vguid.ToString());
                var collerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.TryToEnum<PageEnum>();
                bool hasRight = roleModules.Any(i => i.PageID == (int)collerName);
                if (!hasRight)
                {
                    filterContext.Result = new RedirectResult("/Shared/Error503");
                }
          //  }

           
        }
    }
}