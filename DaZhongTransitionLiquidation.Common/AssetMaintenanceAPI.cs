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
        //每月全量取营收系统、获取车辆固定资产
        public static string GetModifyVehicleAsset()
        {
            var resultData = "";
            var url = ConfigSugar.GetAppString("ModifyVehicleAssetUrl");
            var data = "{" +
                       //"\"YearMonth\":\"{YearMonth}\"".Replace("{YearMonth}", DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2,'0')) +
                       "\"YearMonth\":\"{YearMonth}\"".Replace("{YearMonth}", "201908") +
                       "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                resultData = wc.UploadString(new Uri(url),"POST", data);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", "", ex.ToString()));
            }
            return resultData;
        }
        //根据月份获取退车车辆固定资产
        public static string GetScrapVehicleAsset(string YearMonth)
        {
            var resultData = "";
            var url = ConfigSugar.GetAppString("ScrapVehicleAssetUrl");
            var data = "{" +
                       "\"YearMonth\":\"{YearMonth}\"".Replace("{YearMonth}", YearMonth) +
                       "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                resultData = wc.UploadString(new Uri(url), "POST", data);
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
            }
            return resultData;
        }
        public static string GetASSET_CATEGORY_MINOR(string ORGANIZATION_NUM, string GROUP_ID, string ENGINE_NUMBER, string CHASSIS_NUMBER)
        {
            //发送参数只需要发动机号和车架号
            var AssetMinor = "";
            var url = ConfigSugar.GetAppString("GetAssetMinorUrl");
            var data = "{" +
                            "\"engine_number\":\"{engine_number}\"".Replace("{engine_number}", ENGINE_NUMBER) +
                            "\"chassis_number\":\"{chassis_number}\"".Replace("{chassis_number}", CHASSIS_NUMBER) +
                            "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), "GET", data);
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

        public static bool SendAssetInfo(AssetInfoData assetData)
        {
            var result = false;
            var url = ConfigSugar.GetAppString("");
            var data = "{" +
                            "\"VGUID\":\"{VGUID}\"".Replace("{VGUID}", assetData.VGUID.ToString()) +
                            "\"ORGANATION_NUM\":\"{ORGANATION_NUM}\"".Replace("{ORGANATION_NUM}", assetData.ORGANATION_NUM) +
                            "\"ENGINE_NUMBER\":\"{ENGINE_NUMBER}\"".Replace("{ENGINE_NUMBER}", assetData.ENGINE_NUMBER) +
                            "\"CHASSIS_NUMBER\":\"{CHASSIS_NUMBER}\"".Replace("{CHASSIS_NUMBER}", assetData.CHASSIS_NUMBER) +
                            "\"BOOK_TYPE_CODE\":\"{BOOK_TYPE_CODE}\"".Replace("{BOOK_TYPE_CODE}", assetData.BOOK_TYPE_CODE) +
                            "\"TAG_NUMBER\":\"{TAG_NUMBER}\"".Replace("{TAG_NUMBER}", assetData.TAG_NUMBER) +
                            "\"DESCRIPTION\":\"{DESCRIPTION}\"".Replace("{DESCRIPTION}", assetData.DESCRIPTION) +
                            "\"QUANITIY\":\"{QUANITIY}\"".Replace("{QUANITIY}", assetData.QUANITIY.ToString()) +
                            "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), "POST", data);
                var modelData = resultData.JsonToModel<AssetMaintenanceInfoFlowResult>();
                result = modelData.success;
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
            }
            return result;
        }
    }
}
