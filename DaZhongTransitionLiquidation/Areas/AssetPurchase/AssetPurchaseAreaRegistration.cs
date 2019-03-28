using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase
{
    public class AssetPurchaseAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "AssetPurchase";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "AssetPurchase_default",
                "AssetPurchase/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}