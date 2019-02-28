using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyntacticSugar;
using System.Net;

namespace DaZhongTransitionLiquidation.Common
{
    public class AssetMaintenanceAPI
    {
        public static List<AssetMaintenanceInfoFlowData> GetAssetMaintenanceInfoData()
        {
            List<AssetMaintenanceInfoFlowData> assetFlowList = new List<AssetMaintenanceInfoFlowData>();
            var url = ConfigSugar.GetAppString("AssetMaintenanceTradingFlowUrl");
            var data = "{" +
                            "\"Data\":\"{Data}\"".Replace("{Data}", "") +
                            "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), data);
                var modelData = resultData.JsonToModel<AssetMaintenanceInfoFlowResult>();
                if (modelData.success)
                {
                    assetFlowList = modelData.data;
                }
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
            }
            return assetFlowList;
        }
        public static string GetASSET_CATEGORY_MINOR(string ORGANIZATION_NUM, string GROUP_ID, string ENGINE_NUMBER, string CHASSIS_NUMBER)
        {
            var AssetMinor = "";
            var url = ConfigSugar.GetAppString("GetAssetMinorUrl");
            var data = "{" +
                            "\"Data\":\"{Data}\"".Replace("{Data}", "") +
                            "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), data);
                var modelData = resultData.JsonToModel<AssetMinorResult>();
                if (modelData.success)
                {
                    AssetMinor = modelData.AssetMinor;
                }
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
            }
            return AssetMinor;
        }
    }
}
