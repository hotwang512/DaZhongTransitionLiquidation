using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Common.Pub
{
    public class PubGet
    {
        public static string GetUserKey => CookiesManager<string>.GetInstance()[PubConst.CostCache] + "--" + PubConst.CostCache;
        public static string GetVehicleCheckMangeCompanyReportKey => CookiesManager<string>.GetInstance()[PubConst.GetVehicleCheckMangeCompanyReportCostCache] + "--" + PubConst.GetVehicleCheckMangeCompanyReportCostCache;
        public static string GetVehicleCheckBelongToCompanyReportKey => CookiesManager<string>.GetInstance()[PubConst.GetVehicleCheckBelongToCompanyReportCostCache] + "--" + PubConst.GetVehicleCheckBelongToCompanyReportCostCache;
    }
}