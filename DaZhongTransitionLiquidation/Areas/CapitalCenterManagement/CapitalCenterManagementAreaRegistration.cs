using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement
{
    public class CapitalCenterManagementAreaRegistration: AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "CapitalCenterManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CapitalCenterManagement_default",
                "CapitalCenterManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
