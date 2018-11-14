using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement
{
    public class PaymentManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PaymentManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PaymentManagement_default",
                "PaymentManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}