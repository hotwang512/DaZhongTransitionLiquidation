using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.ReimbursementCenter
{
    public class ReimbursementCenterAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ReimbursementCenter";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ReimbursementCenter_default",
                "ReimbursementCenter/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}