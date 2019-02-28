using DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetsMaintenance;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Models;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class AutoSyncAssetsMaintenance : Controller
    {
        // GET: AutoSyncAssetsMaintenance
        public static void AutoSyncSeavice()
        {
            Thread LogThread = new Thread(new ThreadStart(DoSyncAssetsMaintenance));
            //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
            LogThread.IsBackground = true;
            LogThread.Start();//起线程  
        }
        public static void DoSyncAssetsMaintenance()
        {
            while (true)
            {
                List<AssetMaintenanceInfoFlowData> assetFlowList = new List<AssetMaintenanceInfoFlowData>();
                var success = 0;
                try
                {
                    assetFlowList = AssetMaintenanceAPI.GetAssetMaintenanceInfoData();
                    success = WirterSyncAssetFlow(assetFlowList);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", success, ex.ToString()));
                }
                double timeSpan = ConfigSugar.GetAppString("TimeSpan").TryToInt();
                Thread.Sleep((int)(timeSpan * 1000 * 60 * 60));
            }
        }

        public static int WirterSyncAssetFlow(List<AssetMaintenanceInfoFlowData> assetFlowList)
        {
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            int success = 0;
            foreach (var item in assetFlowList)
            {
                var model = _db.Queryable<Business_AssetMaintenanceInfo>().Where(x => x.TAG_NUMBER == item.TAG_NUMBER && x.ASSET_CATEGORY_MINOR == item.ASSET_CATEGORY_MINOR).First();
                //同步数据需要更新的字段（待修改）
                model.ENGINE_NUMBER = item.ENGINE_NUMBER;
                model.CHASSIS_NUMBER = item.CHASSIS_NUMBER;
                model.BOOK_TYPE_CODE = item.BOOK_TYPE_CODE;
                model.TAG_NUMBER = item.TAG_NUMBER;
                model.DESCRIPTION = item.DESCRIPTION;
                model.QUANTITY = item.QUANTITY;
                model.ASSET_CATEGORY_MAJOR = item.ASSET_CATEGORY_MAJOR;
                model.ASSET_CATEGORY_MINOR = item.ASSET_CATEGORY_MINOR;
                model.ASSET_CREATION_DATE = item.ASSET_CREATION_DATE;
                model.ASSET_COST = item.ASSET_COST;
                model.SALVAGE_TYPE = item.SALVAGE_TYPE;
                model.SALVAGE_PERCENT = item.SALVAGE_PERCENT;
                model.SALVAGE_VALUE = item.SALVAGE_VALUE;
                model.YTD_DEPRECIATION = item.YTD_DEPRECIATION;
                model.ACCT_DEPRECIATION = item.ACCT_DEPRECIATION;
                model.METHOD = item.METHOD;
                model.LIFE_MONTHS = item.LIFE_MONTHS;
                model.AMORTIZATION_FLAG = item.AMORTIZATION_FLAG;
                model.EXP_ACCOUNT_SEGMENT1 = item.EXP_ACCOUNT_SEGMENT1;
                model.EXP_ACCOUNT_SEGMENT2 = item.EXP_ACCOUNT_SEGMENT2;
                model.EXP_ACCOUNT_SEGMENT3 = item.EXP_ACCOUNT_SEGMENT3;
                model.EXP_ACCOUNT_SEGMENT4 = item.EXP_ACCOUNT_SEGMENT4;
                model.EXP_ACCOUNT_SEGMENT5 = item.EXP_ACCOUNT_SEGMENT5;
                model.EXP_ACCOUNT_SEGMENT6 = item.EXP_ACCOUNT_SEGMENT6;
                model.EXP_ACCOUNT_SEGMENT7 = item.EXP_ACCOUNT_SEGMENT7;
                model.FA_LOC_1 = item.FA_LOC_1;
                model.FA_LOC_2 = item.FA_LOC_2;
                model.FA_LOC_3 = item.FA_LOC_3;
                model.RETIRE_FLAG = item.RETIRE_FLAG;
                model.RETIRE_QUANTITY = item.RETIRE_QUANTITY;
                model.RETIRE_COST = item.RETIRE_COST;
                model.RETIRE_DATE = item.RETIRE_DATE;
                model.TRANSACTION_ID = item.TRANSACTION_ID;
                model.LAST_UPDATE_DATE = item.LAST_UPDATE_DATE;
                model.LISENSING_FEE = item.LISENSING_FEE;
                model.OUT_WAREHOUSE_FEE = item.OUT_WAREHOUSE_FEE;
                model.DOME_LIGHT_FEE = item.DOME_LIGHT_FEE;
                model.ANTI_ROBBERY_FEE = item.ANTI_ROBBERY_FEE;
                model.LOADING_FEE = item.LOADING_FEE;
                model.INNER_ROOF_FEE = item.INNER_ROOF_FEE;
                model.TAXIMETER_FEE = item.TAXIMETER_FEE;
                model.OBD_FEE = item.OBD_FEE;
                model.CHANGE_DATE = DateTime.Now;
                model.CHANGE_USER = "System";
                model.STATUS = item.STATUS;
                success = _db.Updateable(model).ExecuteCommand();
            }
            return success;
        }
    }
}