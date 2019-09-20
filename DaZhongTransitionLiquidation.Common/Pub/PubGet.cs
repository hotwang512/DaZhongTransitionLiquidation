using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Common.Pub
{
    public class PubGet
    {
        public static string GetUserKey => CookiesManager<string>.GetInstance()[PubConst.CostCache] + "--" + PubConst.CostCache;
        public static string GetVehicleCheckReportKey => CookiesManager<string>.GetInstance()[PubConst.VehicleCheckReportCostCache] + "--" + PubConst.VehicleCheckReportCostCache;
    }
}