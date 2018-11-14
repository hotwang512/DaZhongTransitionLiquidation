using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.ReportManagement
{
    public class ReportManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ReportManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ReportManagement_default",
                "ReportManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}