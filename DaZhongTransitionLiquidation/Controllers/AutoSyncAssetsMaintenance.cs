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
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;

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
            string getTime = ConfigSugar.GetAppString("Vehicle_GetTime");
            while (true)
            {
                if (LastDayOfMonth(DateTime.Now) == DateTime.Now && DateTime.Now.ToString("HH:ss") == getTime)
                {
                    List<Api_ModifyVehicleAsset> assetModifyFlowList = new List<Api_ModifyVehicleAsset>();      
                    List<Api_ScrapVehicleAsset> assetScrapFlowList = new List<Api_ScrapVehicleAsset>();
                    try
                    {
                        var apiReaultModify = AssetMaintenanceAPI.GetModifyVehicleAsset();
                        var resultApiModifyModel = apiReaultModify
                            .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
                        {
                            var resultColumn = resultApiModifyModel.data[0].COLUMNS;
                            var resultData = resultApiModifyModel.data[0].DATA;
                            foreach (var item in resultData)
                            {
                                var nv = new Api_ModifyVehicleAsset();
                                var t = nv.GetType();
                                var obj = Activator.CreateInstance(t);
                                for (var k = 0; k < resultColumn.Count; k++)
                                {
                                    var pi = t.GetProperty(resultColumn[k]);
                                    if (pi != null) pi.SetValue(nv, item[k], null);
                                }
                                assetModifyFlowList.Add(nv);
                            }
                            WirterSyncModifyAssetFlow(assetModifyFlowList);
                        }
                        {
                            var YearMonth = DateTime.Now.Year + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0');
                            var apiReaultScrap = AssetMaintenanceAPI.GetScrapVehicleAsset(YearMonth);
                            var resultApiScrapModel = apiReaultScrap
                                .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
                            var scrapVehicleList = new List<Api_ScrapVehicleAsset>();
                            var resultColumn = resultApiScrapModel.data[0].COLUMNS;
                            var resultData = resultApiScrapModel.data[0].DATA;
                            foreach (var item in resultData)
                            {
                                var nv = new Api_ScrapVehicleAsset();
                                var t = nv.GetType();
                                var obj = Activator.CreateInstance(t);
                                for (var k = 0; k < resultColumn.Count; k++)
                                {
                                    var pi = t.GetProperty(resultColumn[k]);
                                    if (pi != null) pi.SetValue(nv, item[k], null);
                                }
                                scrapVehicleList.Add(nv);
                            }
                            WirterScrapSyncAssetFlow(assetScrapFlowList);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(string.Format("result:{0}", ex.ToString()));
                    }
                }
                double timeSpan = ConfigSugar.GetAppString("TimeSpan").TryToInt();
                Thread.Sleep(1000);
            }
        }
        public static int WirterSyncModifyAssetFlow(List<Api_ModifyVehicleAsset> assetFlowList)
        {
            var list = new List<Business_ModifyVehicle>();
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();

            //获取所有的经营模式
            var manageModelList = _db.Queryable<Business_ManageModel>().ToList();
            //获取所有的公司
            var ssList = _db.Queryable<Business_SevenSection>().Where(x =>
                x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
            int success = 0;
            var MODIFY_TYPE = "";
            foreach (var item in assetFlowList)
            {
                var assetMaintenanceInfo = _db.Queryable<Business_AssetMaintenanceInfo>()
                    .Where(x => x.ORIGINALID == item.ORIGINALID).First();
                if (assetMaintenanceInfo != null)
                {
                    //Code转名称
                    item.BELONGTO_COMPANY =
                        ssList.First(x => x.OrgID == item.BELONGTO_COMPANY).Descrption;
                    item.MANAGEMENT_COMPANY =
                        ssList.First(x => x.OrgID == item.MANAGEMENT_COMPANY).Abbreviation;
                    //判断变更类型 MODIFY_TYPE
                    if (assetMaintenanceInfo.PLATE_NUMBER != item.PLATE_NUMBER)
                    {
                        //车牌号变更
                        MODIFY_TYPE = "PLATE_NUMBER";
                        list.Add(getModel(manageModelList,item, assetMaintenanceInfo, MODIFY_TYPE));
                    }
                    if (assetMaintenanceInfo.MANAGEMENT_COMPANY != item.MANAGEMENT_COMPANY)
                    {
                        //管理公司
                        MODIFY_TYPE = "FA_LOC_1";
                        list.Add(getModel(manageModelList, item, assetMaintenanceInfo, MODIFY_TYPE));
                    }
                    #region 注释
                    //if (assetMaintenanceInfo.VEHICLE_SHORTNAME != item.VEHICLE_SHORTNAME)
                    //{
                    //    //车辆简称变更
                    //    MODIFY_TYPE = "车辆简称变更";
                    //    list.Add(getModel(manageModelList, item, MODIFY_TYPE));
                    //}
                    //if (assetMaintenanceInfo.BELONGTO_COMPANY != item.BELONGTO_COMPANY)
                    //{
                    //    //资产所属公司
                    //    MODIFY_TYPE = "资产所属公司";
                    //    list.Add(getModel(manageModelList, item, MODIFY_TYPE));
                    //}
                    //if (assetMaintenanceInfo.VEHICLE_STATE != item.VEHICLE_STATE)
                    //{
                    //    //车辆状态
                    //    MODIFY_TYPE = "车辆状态";
                    //    list.Add(getModel(manageModelList, item, MODIFY_TYPE));
                    //}
                    //if (assetMaintenanceInfo.OPERATING_STATE != item.OPERATING_STATE)
                    //{
                    //    //营运状态
                    //    MODIFY_TYPE = "营运状态";
                    //    list.Add(getModel(manageModelList, item, MODIFY_TYPE));
                    //}
                    #endregion
                    if (!item.MODEL_MINOR.IsNullOrEmpty())
                    {
                        //经营模式子类 传过来的经营模式上级
                        var minor = manageModelList.FirstOrDefault(x => x.BusinessName == item.MODEL_MINOR);
                        //如果经营模式子类有多个
                        if (minor != null && manageModelList.Count(x => x.VGUID == minor.ParentVGUID) > 1)
                        {
                            //计算出车龄，并根据车龄判断经营模式子类
                            //车龄 月末时间减去上牌时间（计算两个时间的月数，可能有小数点，保留整位）
                            var months = ((DateTime.Now.Year - assetMaintenanceInfo.LISENSING_DATE.TryToDate().Year) * 12) + (DateTime.Now.Month - assetMaintenanceInfo.LISENSING_DATE.TryToDate().Month);
                            item.MODEL_MINOR = manageModelList.Where(x => x.VGUID == minor.ParentVGUID && x.VehicleAge <= months).OrderByDescending(x => x.VehicleAge).First().BusinessName;
                        }
                        else if (minor != null && manageModelList.Count(x => x.VGUID == minor.ParentVGUID) == 1)
                        {
                            item.MODEL_MINOR = manageModelList
                                .First(x => x.VGUID == minor.ParentVGUID).BusinessName;
                        }
                        //经营模式主类 传过来的经营模式上上级
                        var major = manageModelList.FirstOrDefault(x => x.BusinessName == item.MODEL_MINOR);
                        item.MODEL_MAJOR = manageModelList
                            .First(x => major != null && x.VGUID == major.ParentVGUID).BusinessName;
                        if (assetMaintenanceInfo.MODEL_MINOR != item.MODEL_MINOR || assetMaintenanceInfo.MODEL_MAJOR != item.MODEL_MAJOR)
                        {
                            //经营模式
                            MODIFY_TYPE = "BUSINESS_MODEL";
                            list.Add(getModel(manageModelList, item, assetMaintenanceInfo, MODIFY_TYPE));
                        }
                    }
                }
            }
            success = _db.Insertable<Business_ModifyVehicle>(list).ExecuteCommand();
            return success;
        }
        public static int WirterScrapSyncAssetFlow(List<Api_ScrapVehicleAsset> assetFlowList)
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
                model.BACK_CAR_DATE = item.BACK_CAR_DATE.TryToDate();
                model.ISVERIFY = false;
                model.CREATE_DATE = DateTime.Now;
                model.CREATE_USER = "System";
                list.Add(model);
            }
            success = _db.Insertable<Business_ScrapVehicle>(list).ExecuteCommand();
            return success;
        }
        public static Business_ModifyVehicle getModel(List<Business_ManageModel> manageModelList, Api_ModifyVehicleAsset item, Business_AssetMaintenanceInfo info, string MODIFY_TYPE)
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
            if (MODIFY_TYPE == "BUSINESS_MODEL")
            {
                model.MODEL_MAJOR = item.MODEL_MAJOR;
                model.MODEL_MINOR = item.MODEL_MINOR;
            }
            else
            {
                model.MODEL_MAJOR = info.MODEL_MAJOR;
                model.MODEL_MINOR = info.MODEL_MINOR;
            }
            model.MODIFY_TYPE = MODIFY_TYPE;
            model.ISVERIFY = false;
            model.CREATE_DATE = DateTime.Now;
            model.CREATE_USER = "System";
            return model;
        }
        private static DateTime LastDayOfMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddMonths(1).AddDays(-1);
        }
    }
}