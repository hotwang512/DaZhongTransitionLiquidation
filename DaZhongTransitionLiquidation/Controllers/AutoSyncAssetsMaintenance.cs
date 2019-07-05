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
                List<ModifyVehicleApiModel> assetModifyFlowList = new List<ModifyVehicleApiModel>();
                List<ScrapVehicleApiModel> assetScrapFlowList = new List<ScrapVehicleApiModel>();
                var success = 0;
                try
                {
                    assetModifyFlowList = AssetMaintenanceAPI.GetModifyVehicleAsset();
                    WirterSyncModifyAssetFlow(assetModifyFlowList);
                    var YearMonth = DateTime.Now.Year + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0');
                    assetScrapFlowList = AssetMaintenanceAPI.GetScrapVehicleAsset(YearMonth);
                    WirterScrapSyncAssetFlow(assetScrapFlowList);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("result:{0}", ex.ToString()));
                }
                double timeSpan = ConfigSugar.GetAppString("TimeSpan").TryToInt();
                Thread.Sleep((int)(timeSpan * 1000 * 60 * 60));
            }
        }

        public static int WirterSyncModifyAssetFlow(List<ModifyVehicleApiModel> assetFlowList)
        {
            var list = new List<Business_ModifyVehicle>();
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            int success = 0;
            var MODIFY_TYPE = "";
            foreach (var item in assetFlowList)
            {
                var assetMaintenanceInfo = _db.Queryable<Business_AssetMaintenanceInfo>()
                    .Where(x => x.ORIGINALID == item.ORIGINALID).First();
                if (assetMaintenanceInfo != null)
                {
                    //判断变更类型 MODIFY_TYPE
                    if (assetMaintenanceInfo.PLATE_NUMBER != item.PLATE_NUMBER)
                    {
                        //车牌号变更
                        MODIFY_TYPE = "车牌号变更";
                        list.Add(getModel(item, MODIFY_TYPE));
                    }
                    if (assetMaintenanceInfo.VEHICLE_SHORTNAME != item.VEHICLE_SHORTNAME)
                    {
                        //车辆简称变更
                        MODIFY_TYPE = "车辆简称变更";
                        list.Add(getModel(item, MODIFY_TYPE));
                    }
                    if (assetMaintenanceInfo.MANAGEMENT_COMPANY != item.MANAGEMENT_COMPANY)
                    {
                        //管理公司
                        MODIFY_TYPE = "管理公司";
                        list.Add(getModel(item, MODIFY_TYPE));
                    }
                    if (assetMaintenanceInfo.BELONGTO_COMPANY != item.BELONGTO_COMPANY)
                    {
                        //资产所属公司
                        MODIFY_TYPE = "资产所属公司";
                        list.Add(getModel(item, MODIFY_TYPE));
                    }
                    if (assetMaintenanceInfo.VEHICLE_STATE != item.VEHICLE_STATE)
                    {
                        //车辆状态
                        MODIFY_TYPE = "车辆状态";
                        list.Add(getModel(item, MODIFY_TYPE));
                    }
                    if (assetMaintenanceInfo.OPERATING_STATE != item.OPERATING_STATE)
                    {
                        //营运状态
                        MODIFY_TYPE = "营运状态";
                        list.Add(getModel(item, MODIFY_TYPE));
                    }
                    if (assetMaintenanceInfo.MODEL_MINOR != item.MODEL_MINOR)
                    {
                        //经营模式
                        MODIFY_TYPE = "经营模式";
                        list.Add(getModel(item, MODIFY_TYPE));
                    }
                    if (assetMaintenanceInfo.ENGINE_NUMBER != item.ENGINE_NUMBER)
                    {
                        //发动机号
                        MODIFY_TYPE = "发动机号";
                        list.Add(getModel(item, MODIFY_TYPE));
                    }
                    if (assetMaintenanceInfo.CHASSIS_NUMBER != item.CHASSIS_NUMBER)
                    {
                        //车架号
                        MODIFY_TYPE = "车架号";
                        list.Add(getModel(item, MODIFY_TYPE));
                    }
                }
            }
            success = _db.Insertable<Business_ModifyVehicle>(list).ExecuteCommand();
            return success;
        }
        public static int WirterScrapSyncAssetFlow(List<ScrapVehicleApiModel> assetFlowList)
        {
            var list = new List<Business_ScrapVehicle>();
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            int success = 0;
            foreach (var item in assetFlowList)
            {
                var model = new Business_ScrapVehicle();
                model.VGUID = Guid.NewGuid();
                model.ORIGINALID = item.ORIGINALID;
                model.PLATE_NUMBER = item.PLATE_NUMBER;
                model.BACK_CAR_DATE = item.BACK_CAR_DATE;
                model.ISVERIFY = false;
                model.CREATE_DATE = DateTime.Now;
                model.CREATE_USER = "System";
                list.Add(model);
            }
            success = _db.Insertable<Business_ScrapVehicle>(list).ExecuteCommand();
            return success;
        }
        public static Business_ModifyVehicle getModel(ModifyVehicleApiModel item ,string MODIFY_TYPE)
        {
            var model = new Business_ModifyVehicle();
            model.VGUID = Guid.NewGuid();
            model.ORIGINALID = item.ORIGINALID;
            model.PLATE_NUMBER = item.PLATE_NUMBER;
            model.TAG_NUMBER = item.TAG_NUMBER;
            model.VEHICLE_SHORTNAME = item.VEHICLE_SHORTNAME;
            model.MANAGEMENT_COMPANY = item.MANAGEMENT_COMPANY;
            model.BELONGTO_COMPANY = item.BELONGTO_COMPANY;
            model.VEHICLE_STATE = item.VEHICLE_STATE;
            model.OPERATING_STATE = item.OPERATING_STATE;
            model.ENGINE_NUMBER = item.ENGINE_NUMBER;
            model.CHASSIS_NUMBER = item.CHASSIS_NUMBER;
            //model.MODEL_MAJOR = item.MODEL_MAJOR;
            //model.MODEL_MINOR = item.MODEL_MINOR;

            model.MODIFY_TYPE = MODIFY_TYPE;
            model.ISVERIFY = false;
            model.CREATE_DATE = DateTime.Now;
            model.CREATE_USER = "System";
            return model;
        }
    }
}