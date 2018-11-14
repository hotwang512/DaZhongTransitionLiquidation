using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Common.Pub
{
    public class PubGet
    {
        public static string GetUserKey => CookiesManager<string>.GetInstance()[PubConst.CostCache] + "--" + PubConst.CostCache;
    }
}