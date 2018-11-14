using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement
{
    public class VoucherManageManagementAreaRegistration: AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "VoucherManageManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "VoucherManageManagement_default",
                "VoucherManageManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}