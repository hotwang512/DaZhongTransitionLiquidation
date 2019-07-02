using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.FinancialStatementsManagement
{
    public class FinancialStatementsManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "FinancialStatementsManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "FinancialStatementsManagement_default",
                "FinancialStatementsManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}