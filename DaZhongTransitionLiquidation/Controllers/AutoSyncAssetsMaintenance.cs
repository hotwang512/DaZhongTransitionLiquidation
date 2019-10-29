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
                    //List<Api_ScrapVehicleAsset> assetScrapFlowList = new List<Api_ScrapVehicleAsset>();
                    try
                    {
                        var YearMonth = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0');
                        var apiReaultModify = AssetMaintenanceAPI.GetModifyVehicleAsset(YearMonth);
                        var resultApiModifyModel = apiReaultModify
                            .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
                        //全量获取车辆信息
                        {
                            var resultColumn = resultApiModifyModel.data[0].COLUMNS;
                            var resultData = resultApiModifyModel.data[0].DATA;
                            foreach (var item in resultData)
                            {
                                var nv = new Api_ModifyVehicleAsset();
                                var t = nv.GetType();
                                for (var k = 0; k < resultColumn.Count; k++)
                                {
                                    var pi = t.GetProperty(resultColumn[k]);
                                    if (pi != null) pi.SetValue(nv, item[k], null);
                                }
                                assetModifyFlowList.Add(nv);
                            }
                            WirterSyncModifyAssetFlow(assetModifyFlowList);
                        }
                        //退车
                        //去掉退车自动获取 20190917
                        //{
                        //    var YearMonth = DateTime.Now.Year + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0');
                        //    var apiReaultScrap = AssetMaintenanceAPI.GetScrapVehicleAsset(YearMonth);
                        //    var resultApiScrapModel = apiReaultScrap
                        //        .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
                        //    var scrapVehicleList = new List<Api_ScrapVehicleAsset>();
                        //    var resultColumn = resultApiScrapModel.data[0].COLUMNS;
                        //    var resultData = resultApiScrapModel.data[0].DATA;
                        //    foreach (var item in resultData)
                        //    {
                        //        var nv = new Api_ScrapVehicleAsset();
                        //        var t = nv.GetType();
                        //        for (var k = 0; k < resultColumn.Count; k++)
                        //        {
                        //            var pi = t.GetProperty(resultColumn[k]);
                        //            if (pi != null) pi.SetValue(nv, item[k], null);
                        //        }
                        //        scrapVehicleList.Add(nv);
                        //    }
                        //    WirterScrapSyncAssetFlow(assetScrapFlowList);
                        //}
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
            //var alist = assetFlowList.GroupBy(x => x.ORIGINALID).ToList();
            //var vlist = assetFlowList.Where(x => x.MODEL_MINOR.IsNullOrEmpty()).ToList();
            //var slist = assetFlowList.Where(x => x.CHASSIS_NUMBER == "LSVVL41T2J2010240").ToList();
            //var test1 = assetFlowList.Where(x => x.MANAGEMENT_COMPANY == "37").ToList();
            //var test2 = assetFlowList.Where(x => x.BELONGTO_COMPANY == "37").ToList();
            var list = new List<Business_ModifyVehicle>();
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            int success = 0;
            var result = _db.Ado.UseTran(() =>
            {
                var assetMaintenanceInfoList = _db.Queryable<Business_AssetMaintenanceInfo>().Where(x => x.GROUP_ID == "出租车").ToList();
                //获取所有的经营模式
                var manageModelList = _db.Queryable<Business_ManageModel>().ToList();
                //获取所有的公司
                var ssList = _db.Queryable<Business_SevenSection>().Where(x =>
                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                var modifyVehicleList = _db.Queryable<Business_ModifyVehicle>().Where(x => !x.ISVERIFY).With(SqlWith.TabLockX).ToList();
                var MODIFY_TYPE = "";
                var OLDDATA = "";
                foreach (var item in assetFlowList)
                {
                    if (assetMaintenanceInfoList.Any(x => x.ORIGINALID == item.ORIGINALID))
                    {
                        try
                        {
                            var assetMaintenanceInfo = assetMaintenanceInfoList.Where(x => x.ORIGINALID == item.ORIGINALID).First();
                            //车龄 月末时间减去上牌时间（计算两个时间的月数，可能有小数点，保留整位）
                            var months = ((DateTime.Now.Year - assetMaintenanceInfo.LISENSING_DATE.TryToDate().Year) * 12) + (DateTime.Now.Month - assetMaintenanceInfo.LISENSING_DATE.TryToDate().Month);
                            //Code转名称
                            if (ssList.Any(x => x.OrgID == item.MANAGEMENT_COMPANY))
                            {
                                item.MANAGEMENT_COMPANY =
                                    ssList.First(x => x.OrgID == item.MANAGEMENT_COMPANY).Abbreviation;
                            }
                            if (ssList.Any(x => x.OrgID == item.BELONGTO_COMPANY))
                            {
                                item.BELONGTO_COMPANY =
                                    ssList.First(x => x.OrgID == item.BELONGTO_COMPANY).Abbreviation;
                            }
                            //判断变更类型 MODIFY_TYPE
                            if (assetMaintenanceInfo.PLATE_NUMBER != item.PLATE_NUMBER)
                            {
                                //车牌号变更
                                MODIFY_TYPE = "PLATE_NUMBER";
                                OLDDATA = assetMaintenanceInfo.PLATE_NUMBER;
                                if (!modifyVehicleList.Any(x => x.ORIGINALID == item.ORIGINALID))
                                {
                                    list.Add(getModel(item, assetMaintenanceInfo, MODIFY_TYPE, OLDDATA));
                                }
                            }
                            if (assetMaintenanceInfo.BELONGTO_COMPANY != item.BELONGTO_COMPANY)
                            {
                                //所属公司
                                MODIFY_TYPE = "FA_LOC_1";
                                OLDDATA = assetMaintenanceInfo.BELONGTO_COMPANY;
                                if (!modifyVehicleList.Any(x => x.ORIGINALID == item.ORIGINALID))
                                {
                                    list.Add(getModel(item, assetMaintenanceInfo, MODIFY_TYPE, OLDDATA));
                                }
                            }
                            if (assetMaintenanceInfo.MANAGEMENT_COMPANY != item.MANAGEMENT_COMPANY)
                            {
                                //管理公司
                                MODIFY_TYPE = "FA_LOC_2";
                                OLDDATA = assetMaintenanceInfo.MANAGEMENT_COMPANY;
                                if (!modifyVehicleList.Any(x => x.ORIGINALID == item.ORIGINALID))
                                {
                                    list.Add(getModel(item, assetMaintenanceInfo, MODIFY_TYPE, OLDDATA));
                                }
                            }
                            #region 注释
                            //if (assetMaintenanceInfo.VEHICLE_STATE != item.VEHICLE_STATE)
                            //{
                            //    //车辆状态
                            //    MODIFY_TYPE = "车辆状态";
                            //    list.Add(getModel(manageModelList, item, MODIFY_TYPE,OLDDATA));
                            //}
                            //if (assetMaintenanceInfo.OPERATING_STATE != item.OPERATING_STATE)
                            //{
                            //    //营运状态
                            //    MODIFY_TYPE = "营运状态";
                            //    list.Add(getModel(manageModelList, item, MODIFY_TYPE,OLDDATA));
                            //}
                            #endregion
                            var minorModel = new Business_ManageModel();
                            if (!item.MODEL_MINOR.IsNullOrEmpty())
                            {
                                var minor = manageModelList.Where(x => x.LevelNum == 2 && x.BusinessName == item.MODEL_MINOR).ToList();
                                //如果经营模式第三级有多个
                                if (minor.Count > 1)
                                {
                                    //计算出车龄，并根据车龄判断经营模式子类
                                    //reviewItem.MODEL_MINOR = manageModelList.Where(x => x.VGUID == minor.ParentVGUID && x.VehicleAge > months).OrderBy(x => x.VehicleAge).First().BusinessName;
                                    var level3 = manageModelList
                                        .Where(x => x.LevelNum == 2 && x.BusinessName == item.MODEL_MINOR && x.VehicleAge > months)
                                        .OrderBy(x => x.VehicleAge).First();
                                    minorModel = manageModelList.First(x => x.VGUID == level3.ParentVGUID);
                                    item.MODEL_MINOR = minorModel.BusinessName;
                                }
                                else if (minor.Count == 1)
                                {
                                    minorModel = manageModelList
                                        .First(x => x.VGUID == minor.First().ParentVGUID);
                                    item.MODEL_MINOR = minorModel.BusinessName;
                                }
                                //经营模式主类 传过来的经营模式上上级
                                var major = manageModelList.FirstOrDefault(x => x.VGUID == minorModel.ParentVGUID);
                                item.MODEL_MAJOR = major.BusinessName;
                                //根据经营模式和车型获取到资产主类子类，通过主类子类取到折旧方法维护中的财务信息
                                var modelCategory = _db.Queryable<Business_ManageModel_AssetsCategory>()
                                    .Where(x => x.GoodsModel == assetMaintenanceInfo.VEHICLE_SHORTNAME && x.ManageModelVGUID == minorModel.VGUID).First();
                                var assetCategory = _db.Queryable<Business_AssetsCategory>()
                                    .Where(x => x.VGUID == modelCategory.AssetsCategoryVGUID).First();
                                assetMaintenanceInfo.ASSET_CATEGORY_MAJOR = assetCategory.ASSET_CATEGORY_MAJOR;
                                assetMaintenanceInfo.ASSET_CATEGORY_MINOR = assetCategory.ASSET_CATEGORY_MINOR;
                            }
                            else
                            {
                                if (item.OPERATING_STATE == "停运")
                                {
                                    item.MODEL_MAJOR = "停运模式";
                                    item.MODEL_MINOR = "旧车停运";
                                }
                                else if (item.OPERATING_STATE == "")
                                {
                                    item.MODEL_MAJOR = "停运模式";
                                    item.MODEL_MINOR = "新车停运";
                                }
                            }
                            if ((assetMaintenanceInfo.MODEL_MINOR != item.MODEL_MINOR || assetMaintenanceInfo.MODEL_MAJOR != item.MODEL_MAJOR))
                            {
                                //经营模式
                                MODIFY_TYPE = "BUSINESS_MODEL";
                                OLDDATA = assetMaintenanceInfo.MODEL_MAJOR + "-" + assetMaintenanceInfo.MODEL_MINOR;
                                if (!modifyVehicleList.Any(x => x.ORIGINALID == item.ORIGINALID))
                                {
                                    list.Add(getModel(item, assetMaintenanceInfo, MODIFY_TYPE, OLDDATA));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                    }
                }
                if (list.Count > 0)
                {
                    success = _db.Insertable<Business_ModifyVehicle>(list).ExecuteCommand();
                }
            });
            return success;
        }
        public static int WirterScrapSyncAssetFlow(List<Api_ScrapVehicleAsset> assetFlowList)
        {
            var list = new List<Business_ScrapVehicle>();
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            int success = 0;
            var result = _db.Ado.UseTran(() =>
            {
                var ids = assetFlowList.Select(x => x.ORIGINALID).ToList();
                var assetList = _db.Queryable<Business_AssetMaintenanceInfo>().ToList();
                //获取所有的经营模式
                var manageModelList = _db.Queryable<Business_ManageModel>().ToList();
                var scrapVehicleList = _db.Queryable<Business_ScrapVehicle>().Where(x => !x.ISVERIFY).With(SqlWith.TabLockX).ToList();
                foreach (var item in assetFlowList)
                {
                    if (assetList.Any(x => x.ORIGINALID == item.ORIGINALID))
                    {
                        var model = new Business_ScrapVehicle();
                        model.VGUID = Guid.NewGuid();
                        model.ORIGINALID = item.ORIGINALID;
                        model.PLATE_NUMBER = item.PLATE_NUMBER;
                        model.OPERATING_STATE = item.OPERATING_STATE;
                        model.VEHICLE_STATE = item.VEHICLE_STATE;
                        model.BACK_CAR_DATE = item.BACK_CAR_DATE.TryToDate();
                        var lisensingDate = assetList.First(x => x.ORIGINALID == item.ORIGINALID).LISENSING_DATE.TryToDate();
                        model.VEHICLE_AGE = ((DateTime.Now.Year - lisensingDate.Year) * 12) + (DateTime.Now.Month - lisensingDate.Month);
                        model.ISVERIFY = false;
                        model.CREATE_DATE = DateTime.Now;
                        model.CREATE_USER = "System";
                        var minorModel = new Business_ManageModel();
                        if (!item.MODEL_MINOR.IsNullOrEmpty())
                        {
                            var minor = manageModelList.Where(x => x.LevelNum == 2 && x.BusinessName == item.MODEL_MINOR).ToList();
                            //如果经营模式第三级有多个
                            if (minor.Count > 1)
                            {
                                //计算出车龄，并根据车龄判断经营模式子类
                                //reviewItem.MODEL_MINOR = manageModelList.Where(x => x.VGUID == minor.ParentVGUID && x.VehicleAge > months).OrderBy(x => x.VehicleAge).First().BusinessName;
                                var level3 = manageModelList
                                    .Where(x => x.LevelNum == 2 && x.BusinessName == item.MODEL_MINOR && x.VehicleAge > model.VEHICLE_AGE)
                                    .OrderBy(x => x.VehicleAge).First();
                                minorModel = manageModelList.First(x => x.VGUID == level3.ParentVGUID);
                                item.MODEL_MINOR = minorModel.BusinessName;
                            }
                            else if (minor.Count == 1)
                            {
                                minorModel = manageModelList
                                    .First(x => x.VGUID == minor.First().ParentVGUID);
                                item.MODEL_MINOR = minorModel.BusinessName;
                            }
                            //经营模式主类 传过来的经营模式上上级
                            var major = manageModelList.FirstOrDefault(x => x.VGUID == minorModel.ParentVGUID);
                            model.MODEL_MAJOR = major.BusinessName;
                        }
                        else
                        {
                            if (item.OPERATING_STATE == "停运")
                            {
                                model.MODEL_MAJOR = "停运模式";
                                model.MODEL_MINOR = "旧车停运";
                            }
                            else if (item.OPERATING_STATE == "")
                            {
                                model.MODEL_MAJOR = "停运模式";
                                model.MODEL_MINOR = "新车停运";
                            }
                        }
                        if (!scrapVehicleList.Any(x => x.ORIGINALID == model.ORIGINALID))
                        {
                            list.Add(model);
                        }
                    }
                }
                if (list.Count > 0)
                {
                    success = _db.Insertable<Business_ScrapVehicle>(list).ExecuteCommand();
                }
            });
            return success;
        }
        public static Business_ModifyVehicle getModel(Api_ModifyVehicleAsset item, Business_AssetMaintenanceInfo info, string MODIFY_TYPE, string OLDDATA)
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
                //model.ASSET_CATEGORY_MAJOR = info.ASSET_CATEGORY_MAJOR;
                //model.ASSET_CATEGORY_MINOR = info.ASSET_CATEGORY_MINOR;
            }
            else
            {
                model.MODEL_MAJOR = info.MODEL_MAJOR;
                model.MODEL_MINOR = info.MODEL_MINOR;
            }
            model.ASSET_CATEGORY_MAJOR = info.ASSET_CATEGORY_MAJOR;
            model.ASSET_CATEGORY_MINOR = info.ASSET_CATEGORY_MINOR;
            model.MODIFY_TYPE = MODIFY_TYPE;
            model.OLDDATA = OLDDATA;
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