using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter
{
    public class AnalysisManagementCenterAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "AnalysisManagementCenter";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "AnalysisManagementCenter_default",
                "AnalysisManagementCenter/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}