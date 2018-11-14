using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.HomePage
{
    public class HomePageAreaRegistration: AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "HomePage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HomePage_default",
                "HomePage/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
