using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetReport
{
    public class AssetReportController : BaseController
    {
        public AssetReportController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("f0ed636a-5001-4393-9bbc-5c9dd195f64b");
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="YearMonth">年月</param>
        /// <param name="PeriodType">期初期末</param>
        /// <param name="para"></param>
        /// <returns></returns>
        //public JsonResult GetAssetReportPeriodListDatas(string YearMonth, string PeriodType, GridParams para)
        //{
        //    var date = YearMonth.TryToDate();
        //    if (PeriodType == "StartPeriod")
        //    {
        //        date = date.AddMonths(-1);
        //        YearMonth = date.Year.ToString() + date.Month.ToString().PadLeft(2,'0');
        //    }
        //    else
        //    {
        //        YearMonth = YearMonth.Replace("-", "");
        //    }
        //    var cache = CacheManager<Sys_User>.GetInstance();
        //    List<Api_ModifyVehicleAsset> assetModifyFlowList = new List<Api_ModifyVehicleAsset>();
        //    var reportList = new List<Business_VehicleCheckReport>();
        //    //var reportShowList = new List<Business_VehicleCheckReport>();
        //    DbBusinessDataService.Command(db =>
        //    {
        //        if (!db.Queryable<Business_VehicleCheckReport>().Any(x => x.YearMonth == YearMonth && x.PeriodType == "期初"))
        //        {
        //            YearMonth = YearMonth.Replace("-", "");
        //            var apiReaultModify = AssetMaintenanceAPI.GetModifyVehicleAsset(YearMonth);
        //            var resultApiModifyModel = apiReaultModify
        //                .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
        //            //全量获取车辆信息
        //            var resultColumn = resultApiModifyModel.data[0].COLUMNS;
        //            var resultData = resultApiModifyModel.data[0].DATA;
        //            foreach (var item in resultData)
        //            {
        //                var nv = new Api_ModifyVehicleAsset();
        //                var t = nv.GetType();
        //                for (var k = 0; k < resultColumn.Count; k++)
        //                {
        //                    var pi = t.GetProperty(resultColumn[k]);
        //                    if (pi != null) pi.SetValue(nv, item[k], null);
        //                }
        //                assetModifyFlowList.Add(nv);
        //            }
        //            //获取所有的公司
        //            var ssList = db.Queryable<Business_SevenSection>().Where(x =>
        //                x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
        //            foreach (var item in assetModifyFlowList)
        //            {
        //                if (item.BELONGTO_COMPANY.TryToInt() != 37 && item.MANAGEMENT_COMPANY.TryToInt() != 37 && item.BELONGTO_COMPANY.TryToInt() < 100 && item.MANAGEMENT_COMPANY.TryToInt() < 100)
        //                {
        //                    //Code转名称
        //                    if (ssList.Any(x => x.OrgID == item.MANAGEMENT_COMPANY))
        //                    {
        //                        item.MANAGEMENT_COMPANY =
        //                            ssList.First(x => x.OrgID == item.MANAGEMENT_COMPANY).Abbreviation;
        //                    }
        //                    if (ssList.Any(x => x.OrgID == item.BELONGTO_COMPANY))
        //                    {
        //                        item.BELONGTO_COMPANY =
        //                            ssList.First(x => x.OrgID == item.BELONGTO_COMPANY).Abbreviation;
        //                    }
        //                    var report = new Business_VehicleCheckReport();
        //                    report.YearMonth = YearMonth;
        //                    
        //                    report.VGUID = Guid.NewGuid();
        //                    report.VehicleModel = item.VEHICLE_SHORTNAME;
        //                    report.ManageCompany = item.MANAGEMENT_COMPANY;
        //                    report.BelongToCompany = item.BELONGTO_COMPANY;
        //                    report.Quantity = 1;
        //                    report.CreateDate = DateTime.Now;
        //                    report.CreateUser = cache[PubGet.GetUserKey].LoginName;
        //                    report.PeriodType = PeriodType == "StartPeriod" ? "期初" : "期末";
        //                    reportList.Add(report);
        //                }
        //            }
        //            var reportCache = CacheManager<List<Business_VehicleCheckReport>>.GetInstance();
        //            reportCache.Remove(PubGet.GetVehicleCheckReportKey);
        //            reportCache.Add(PubGet.GetVehicleCheckReportKey, reportList, 8 * 60 * 60);
        //        }
        //        else
        //        {
        //            reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth)
        //                .ToList();
        //        }
        //    });
        //    var dataShow = reportList.GroupBy(x => new { x.PeriodType, x.ManageCompany, x.VehicleModel })
        //        .Select(x => new
        //        {
        //            ManageCompany = x.Key.ManageCompany,
        //            VehicleModel = x.Key.VehicleModel,
        //            Quantity = x.Sum(s => s.Quantity),
        //            PeriodType = x.Key.PeriodType
        //        }).OrderBy(x => x.VehicleModel).ToList();
        //    var listCompany = new List<string>();
        //    var vehicleModelList = reportList.GroupBy(x => new { x.ManageCompany })
        //        .Select(x => new { x.Key }).OrderBy(x => x.Key.ManageCompany).ToList();
        //    foreach (var item in vehicleModelList)
        //    {
        //        if (!item.Key.ManageCompany.IsNullOrEmpty())
        //        {
        //            listCompany.Add(item.Key.ManageCompany);
        //        }
        //    }
        //    var listReport = new List<VehicleCheckShowReport>();
        //    for (var i = 0; i < listCompany.Count; i++)
        //    {
        //        var report = new VehicleCheckShowReport();
        //        report.PeriodType = "期初";
        //        report.Company = listCompany[i];
        //        var showList = dataShow.Where(x => x.ManageCompany == report.Company).GroupBy(x => new { x.VehicleModel}).Select(x => new {x.Key.VehicleModel}).ToList();
        //        foreach (var item in showList)
        //        {
        //            var mdata = new VehicleModelData();
        //            mdata.VehicleModel = item.VehicleModel;
        //            mdata.Quantity = dataShow
        //                .Where(x => x.ManageCompany == report.Company && x.VehicleModel == item.VehicleModel)
        //                .Sum(s => s.Quantity);
        //            report.ResultVehicleModelList.Add(mdata);
        //        }
        //        listReport.Add(report);
        //    }
        //    //return Json(listReport, JsonRequestBehavior.AllowGet);
        //    return Json(reportList.OrderBy(x => x.VehicleModel).ToList(), JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetAssetReportVehicleModel(string YearMonth)
        {
            var listType = new List<string>();
            var date = YearMonth.TryToDate();
            YearMonth = YearMonth.Replace("-", "");
            List<Api_ModifyVehicleAsset> assetModifyFlowList = new List<Api_ModifyVehicleAsset>();
            DbBusinessDataService.Command(db =>
            {
                if (!db.Queryable<Business_VehicleCheckReport>().Any(x => x.YearMonth == YearMonth))
                {
                    YearMonth = YearMonth.Replace("-", "");
                    var apiReaultModify = AssetMaintenanceAPI.GetModifyVehicleAsset(YearMonth);
                    var resultApiModifyModel = apiReaultModify
                        .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
                    //全量获取车辆信息
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
                    var vehicleModelList = assetModifyFlowList.GroupBy(x => new {x.VEHICLE_SHORTNAME})
                        .Select(x => new {x.Key}).OrderBy(x => x.Key.VEHICLE_SHORTNAME).ToList();
                    foreach (var item in vehicleModelList)
                    {
                        if (!item.Key.VEHICLE_SHORTNAME.IsNullOrEmpty())
                        {
                            listType.Add(item.Key.VEHICLE_SHORTNAME);
                        }
                    }
                }
                else
                {
                    var vehicleModelList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth).GroupBy(x => new { x.VehicleModel}).Select(x => new {x.VehicleModel}).ToList();
                    foreach (var item in vehicleModelList)
                    {
                        listType.Add(item.VehicleModel);
                    }
                }
            });
            return Json(listType.ToList(), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="YearMonth">年月</param>
        /// <param name="ShowType">增加减少</param>
        /// <param name="para"></param>
        /// <returns></returns>
        //public JsonResult GetAssetReportChangedListDatas(string YearMonth, string ShowType, GridParams para)
        //{
        //    YearMonth = YearMonth.Replace("-", "");
        //    var cache = CacheManager<Sys_User>.GetInstance();
        //    List<Api_ModifyVehicleAsset> assetModifyFlowList = new List<Api_ModifyVehicleAsset>();
        //    var reportAddedList = new List<Business_VehicleCheckChangeReport>();
        //    var reportReduceList = new List<Business_VehicleCheckChangeReport>();
        //    DbBusinessDataService.Command(db =>
        //    {
        //        if (ShowType == "Added")
        //        {
        //            if (!db.Queryable<Business_VehicleCheckChangeReport>().Any(x => x.YearMonth == YearMonth && x.ChangeType == ShowType))
        //            {
        //                YearMonth = YearMonth.Replace("-", "");
        //                var addedList = db.Queryable<Business_AssetReview>().Where(x => x.GROUP_ID == "出租车" && !x.ISVERIFY)
        //                    .ToList();
        //                foreach (var item in addedList)
        //                {
        //                    var report = new Business_VehicleCheckChangeReport();
        //                    report.YearMonth = YearMonth;
        //                    
        //                    report.VGUID = Guid.NewGuid();
        //                    report.VehicleModel = item.VEHICLE_SHORTNAME;
        //                    report.ManageCompany = item.MANAGEMENT_COMPANY;
        //                    report.BelongToCompany = item.BELONGTO_COMPANY;
        //                    report.ChangeType = ShowType;
        //                    report.Quantity = 1;
        //                    report.CreateDate = DateTime.Now;
        //                    report.CreateUser = cache[PubGet.GetUserKey].LoginName;
        //                    reportAddedList.Add(report);
        //                }
        //                var reportAddedCache = CacheManager<List<Business_VehicleCheckChangeReport>>.GetInstance();
        //                reportAddedCache.Remove(PubGet.GetVehicleCheckReportKey);
        //                reportAddedCache.Add(PubGet.GetVehicleCheckReportKey, reportAddedList, 8 * 60 * 60);
        //            }
        //            else
        //            {
        //                reportAddedList = db.Queryable<Business_VehicleCheckChangeReport>().Where(x => x.YearMonth == YearMonth && x.ChangeType == ShowType)
        //                    .ToList();
        //            }
        //        }else if (ShowType == "Reduce")
        //        {
        //            if (!db.Queryable<Business_VehicleCheckChangeReport>().Any(x => x.YearMonth == YearMonth && x.ChangeType == ShowType))
        //            {
        //                YearMonth = YearMonth.Replace("-", "");
        //                var ReduceList = db.SqlQueryable<Business_ScrapVehicleShowModel>(@"select mi.TAG_NUMBER
        //                                                                                         , mi.VEHICLE_SHORTNAME
        //                                                                                         , mi.MANAGEMENT_COMPANY
        //                                                                                         , mi.BELONGTO_COMPANY
        //                                                                                         , mi.ENGINE_NUMBER
        //                                                                                         , mv.*
	       //                                                                                      , mi.EXP_ACCOUNT_SEGMENT
	       //                                                                                      , mi.COMMISSIONING_DATE as PERIOD
	       //                                                                                      , mi.QUANTITY
	       //                                                                                      , mi.ASSET_COST
	       //                                                                                      , mi.ASSET_ID
	       //                                                                                      , mi.GROUP_ID
	       //                                                                                      , mi.BOOK_TYPE_CODE
						  //                                                                       , mi.LISENSING_DATE
        //                                                                                    from Business_ScrapVehicle mv
        //                                                                                    left join Business_AssetMaintenanceInfo mi
        //                                                                                    on mv.ORIGINALID = mi.ORIGINALID").Where(i => !i.ISVERIFY).ToList();
        //                foreach (var item in ReduceList)
        //                {
        //                    var report = new Business_VehicleCheckChangeReport();
        //                    report.YearMonth = YearMonth;
        //                    
        //                    report.VGUID = Guid.NewGuid();
        //                    report.VehicleModel = item.VEHICLE_SHORTNAME;
        //                    report.ManageCompany = item.MANAGEMENT_COMPANY;
        //                    report.BelongToCompany = item.BELONGTO_COMPANY;
        //                    report.ChangeType = ShowType;
        //                    report.Quantity = 1;
        //                    report.CreateDate = DateTime.Now;
        //                    report.CreateUser = cache[PubGet.GetUserKey].LoginName;
        //                    reportAddedList.Add(report);
        //                }
        //                var reportReduceCache = CacheManager<List<Business_VehicleCheckChangeReport>>.GetInstance();
        //                reportReduceCache.Remove(PubGet.GetVehicleCheckReportKey);
        //                reportReduceCache.Add(PubGet.GetVehicleCheckReportKey, reportAddedList, 8 * 60 * 60);
        //            }
        //            else
        //            {
        //                reportAddedList = db.Queryable<Business_VehicleCheckChangeReport>().Where(x => x.YearMonth == YearMonth && x.ChangeType == ShowType)
        //                    .ToList();
        //            }
        //        }
        //        //增加数量为0的公司
        //        var apiReaultModify = AssetMaintenanceAPI.GetModifyVehicleAsset(YearMonth);
        //        var resultApiModifyModel = apiReaultModify
        //            .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
        //        //全量获取车辆信息
        //        var resultColumn = resultApiModifyModel.data[0].COLUMNS;
        //        var resultData = resultApiModifyModel.data[0].DATA;
        //        foreach (var item in resultData)
        //        {
        //            var nv = new Api_ModifyVehicleAsset();
        //            var t = nv.GetType();
        //            for (var k = 0; k < resultColumn.Count; k++)
        //            {
        //                var pi = t.GetProperty(resultColumn[k]);
        //                if (pi != null) pi.SetValue(nv, item[k], null);
        //            }
        //            assetModifyFlowList.Add(nv);
        //        }
        //        var companyData = assetModifyFlowList.GroupBy(x => new { x.BELONGTO_COMPANY }).ToList();
        //        var ssList = db.Queryable<Business_SevenSection>().Where(x =>
        //            x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
        //        foreach (var item in companyData)
        //        {
        //            if (ssList.Any(x => x.OrgID == item.Key.BELONGTO_COMPANY))
        //            {
        //                var belongToCompany = ssList.First(x => x.OrgID == item.Key.BELONGTO_COMPANY).Abbreviation;
        //                if (reportAddedList.Count > 0 && !reportAddedList.Any(x => x.BelongToCompany == belongToCompany))
        //                {
        //                    var report = new Business_VehicleCheckChangeReport();
        //                    report.YearMonth = YearMonth;
        //                    report.VGUID = Guid.NewGuid();
        //                    report.VehicleModel = reportAddedList.First().VehicleModel;
        //                    report.BelongToCompany = belongToCompany;
        //                    report.ManageCompany = reportAddedList.First().ManageCompany;
        //                    report.ChangeType = ShowType;
        //                    report.Quantity = 0;
        //                    report.CreateDate = DateTime.Now;
        //                    report.CreateUser = cache[PubGet.GetUserKey].LoginName;
        //                    reportAddedList.Add(report);
        //                }
        //            }
        //        }
        //    });
        //    return Json(reportAddedList.OrderBy(x => x.VehicleModel).ToList(), JsonRequestBehavior.AllowGet);
        //}
        public JsonResult SubmitAssetReport(string YearMonth)
        {
            YearMonth = YearMonth.Replace("-", "");
            var resultModel = new ResultModel<string,string>() { IsSuccess = false, Status = "0" };
            var reportCache = CacheManager<List<VehicleCheckShowReport>>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                if (!db.Queryable<Business_VehicleCheckReport>().Any(x => x.YearMonth == YearMonth))
                {
                    var companyBelongTo = "";
                    var companyManage = "";
                    var cache = CacheManager<Sys_User>.GetInstance();
                    var listManageReport = reportCache[PubGet.GetVehicleCheckMangeCompanyReportKey].OrderBy(x => x.Sort).ToList();
                    //校验数据 期初加增加减去减少等于期末
                    //所属公司
                    var belongToCompany = listManageReport.Where(x => x.CompanyType == "所属公司");
                    //所属公司
                    var manageCompany = listManageReport.Where(x => x.CompanyType == "管理公司");
                    var belongToCompanys = belongToCompany.GroupBy(x => x.Company).Select(x => x.Key).ToList();
                    var manageCompanys = manageCompany.GroupBy(x => x.Company).Select(x => x.Key).ToList();
                    //校验
                    foreach (var item in belongToCompanys)
                    {
                        var startPeriod = belongToCompany.Where(x => x.Company == item && x.PeriodType == "期初")
                            .Sum(x => x.ResultVehicleModelList.Sum(k => k.Quantity));
                        var addedPeriod = belongToCompany.Where(x => x.Company == item && x.PeriodType == "增加")
                            .Sum(x => x.ResultVehicleModelList.Sum(k => k.Quantity));
                        var reducePeriod = belongToCompany.Where(x => x.Company == item && x.PeriodType == "减少")
                            .Sum(x => x.ResultVehicleModelList.Sum(k => k.Quantity));
                        var endPeriod = belongToCompany.Where(x => x.Company == item && x.PeriodType == "期末")
                            .Sum(x => x.ResultVehicleModelList.Sum(k => k.Quantity));
                        if (startPeriod + addedPeriod - reducePeriod != endPeriod)
                        {
                            companyBelongTo = companyBelongTo + item  + ",";
                        }
                    }
                    foreach (var item in manageCompanys)
                    {
                        var startPeriod = manageCompany.Where(x => x.Company == item && x.PeriodType == "期初")
                            .Sum(x => x.ResultVehicleModelList.Sum(k => k.Quantity));
                        var addedPeriod = manageCompany.Where(x => x.Company == item && x.PeriodType == "增加")
                            .Sum(x => x.ResultVehicleModelList.Sum(k => k.Quantity));
                        var reducePeriod = manageCompany.Where(x => x.Company == item && x.PeriodType == "减少")
                            .Sum(x => x.ResultVehicleModelList.Sum(k => k.Quantity));
                        var endPeriod = manageCompany.Where(x => x.Company == item && x.PeriodType == "期末")
                            .Sum(x => x.ResultVehicleModelList.Sum(k => k.Quantity));
                        if (startPeriod + addedPeriod - reducePeriod != endPeriod)
                        {
                            companyManage = companyManage + item + ",";
                        }
                    }
                    if (companyManage == "" && companyBelongTo == "")
                    {
                        if (listManageReport.Count > 0)
                        {
                            var listCheckReport = new List<Business_VehicleCheckReport>();
                            foreach (var itemParent in listManageReport)
                            {
                                foreach (var itemChildren in itemParent.ResultVehicleModelList)
                                {
                                    var report = new Business_VehicleCheckReport();
                                    report.VGUID = Guid.NewGuid();
                                    report.CreateDate = DateTime.Now;
                                    report.YearMonth = YearMonth;
                                    report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                    report.CompanyType = itemParent.CompanyType;
                                    report.PeriodType = itemParent.PeriodType;
                                    report.CompanyName = itemParent.Company;
                                    report.VehicleModel = itemChildren.VehicleModel;
                                    report.Quantity = itemChildren.Quantity;
                                    listCheckReport.Add(report);
                                }
                            }
                            db.Insertable<Business_VehicleCheckReport>(listCheckReport).ExecuteCommand();
                            resultModel.IsSuccess = true;
                            resultModel.Status = "1";
                        }
                    }
                    else
                    {
                        resultModel.IsSuccess = false;
                        resultModel.Status = "3";
                        resultModel.ResultInfo = "所属公司：" + companyBelongTo + "校验不通过;";
                        resultModel.ResultInfo2 = "管理公司：" + companyManage + "校验不通过;";
                    }
                }
                else
                {
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetManageCompanyAssetReport(string YearMonth, GridParams para)
        {
            var resuleModel = new ResultModel<List<string>,List<VehicleCheckShowReport>>() { IsSuccess = false, Status = "0" };
            var currentDate = YearMonth.TryToDate();
            var nextDate = currentDate.AddMonths(1);
            var lastDate1 = currentDate.AddMonths(-1);
            var LastYearMonth = lastDate1.Year + lastDate1.Month.ToString().PadLeft(2, '0');
            YearMonth = YearMonth.Replace("-", "");
            var cache = CacheManager<Sys_User>.GetInstance();
            List<Api_ModifyVehicleAsset> assetModifyCurrentFlowList = new List<Api_ModifyVehicleAsset>();
            List<Api_ModifyVehicleAsset> assetModifyLastFlowList = new List<Api_ModifyVehicleAsset>();
            var reportList = new List<Business_VehicleCheckReport>();
            var listReport = new List<VehicleCheckShowReport>();
            var listStrCompany = new List<string>();
            var listStrBelongToCompany = new List<string>();
            var listStrVehicleModel = new List<string>();
            DbBusinessDataService.Command(db =>
            {
                var isSubmit = db.Queryable<Business_VehicleCheckReport>().Any(x => x.YearMonth == YearMonth);
                if (!isSubmit)
                {
                    var CurrentYearMonth = currentDate.Year.ToString() + currentDate.Month.ToString().PadLeft(2, '0');
                    var apiReaultModifyCurrentData = AssetMaintenanceAPI.GetModifyVehicleAsset(CurrentYearMonth);
                    var resultApiModifyCurrentDataModel = apiReaultModifyCurrentData
                        .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
                    var resultCurrentDataColumn = resultApiModifyCurrentDataModel.data[0].COLUMNS;
                    var resultCurrentData = resultApiModifyCurrentDataModel.data[0].DATA;
                    foreach (var item in resultCurrentData)
                    {
                        var nv = new Api_ModifyVehicleAsset();
                        var t = nv.GetType();
                        for (var k = 0; k < resultCurrentDataColumn.Count; k++)
                        {
                            var pi = t.GetProperty(resultCurrentDataColumn[k]);
                            if (pi != null) pi.SetValue(nv, item[k], null);
                        }
                        assetModifyCurrentFlowList.Add(nv);
                    }
                    var lastDate = currentDate.AddMonths(-1);
                    var lastYearMonth = lastDate.Year.ToString() + lastDate.Month.ToString().PadLeft(2, '0');
                    var apiReaultModifyLastData = AssetMaintenanceAPI.GetModifyVehicleAsset(lastYearMonth);
                    var resultApiModifyLastDataModel = apiReaultModifyLastData
                        .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
                    var resultLastDataColumn = resultApiModifyLastDataModel.data[0].COLUMNS;
                    var resultLastData = resultApiModifyLastDataModel.data[0].DATA;
                    foreach (var item in resultLastData)
                    {
                        var nv = new Api_ModifyVehicleAsset();
                        var t = nv.GetType();
                        for (var k = 0; k < resultLastDataColumn.Count; k++)
                        {
                            var pi = t.GetProperty(resultLastDataColumn[k]);
                            if (pi != null) pi.SetValue(nv, item[k], null);
                        }
                        assetModifyLastFlowList.Add(nv);
                    }
                    assetModifyCurrentFlowList = assetModifyCurrentFlowList.Where(x =>
                        x.BELONGTO_COMPANY != "37" && x.MANAGEMENT_COMPANY != "197" && x.MANAGEMENT_COMPANY != "480" && x.VEHICLE_STATE == "在役").ToList();
                    assetModifyLastFlowList = assetModifyLastFlowList.Where(x =>
                        x.BELONGTO_COMPANY != "37" && x.MANAGEMENT_COMPANY != "197" && x.MANAGEMENT_COMPANY != "480" && x.VEHICLE_STATE == "在役").ToList();
                    var allFlowList = assetModifyCurrentFlowList.Union(assetModifyLastFlowList);
                    var dt = assetModifyCurrentFlowList.TryToDataTable();
                    //车型
                    var vehicleModelList = allFlowList.GroupBy(x => x.VEHICLE_SHORTNAME).Where(x => x.Key != null).Select(x => x.Key).OrderBy(x => x).ToList();
                    foreach (var item in vehicleModelList)
                    {
                        listStrVehicleModel.Add(item);
                    }
                    //获取所有的公司
                    var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                        x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                    foreach (var item in assetModifyCurrentFlowList)
                    {
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
                    }
                    foreach (var item in assetModifyLastFlowList)
                    {
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
                    }
                    var companyManageList = assetModifyCurrentFlowList.Union(assetModifyLastFlowList).GroupBy(x => new { x.MANAGEMENT_COMPANY }).Where(x => x.Key.MANAGEMENT_COMPANY != null)
                        .Select(x => new { x.Key }).OrderBy(x => x.Key.MANAGEMENT_COMPANY).ToList();
                    foreach (var item in companyManageList)
                    {
                        listStrCompany.Add(item.Key.MANAGEMENT_COMPANY);
                    }
                    var companyBelongList = assetModifyCurrentFlowList.Union(assetModifyLastFlowList).GroupBy(x => new { x.BELONGTO_COMPANY }).Where(x => x.Key.BELONGTO_COMPANY != null)
                        .Select(x => new { x.Key }).OrderBy(x => x.Key.BELONGTO_COMPANY).ToList();
                    foreach (var item in companyBelongList)
                    {
                        listStrBelongToCompany.Add(item.Key.BELONGTO_COMPANY);
                    }
                }
                else
                {
                    var vlist = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth).GroupBy(x => x.VehicleModel).Select(x => x.VehicleModel).ToList();
                    foreach (var item in vlist)
                    {
                        listStrVehicleModel.Add(item);
                    }
                }
                #region 期末
                if (!isSubmit)
                {
                    
                    foreach (var item in assetModifyCurrentFlowList)
                    {
                        var report = new Business_VehicleCheckReport();
                        report.YearMonth = YearMonth;
                        report.VGUID = Guid.NewGuid();
                        report.VehicleModel = item.VEHICLE_SHORTNAME;
                        report.CompanyName = item.MANAGEMENT_COMPANY;
                        report.Quantity = 1;
                        report.CreateDate = DateTime.Now;
                        report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        report.PeriodType = "期末";
                        report.CompanyType = "管理公司";
                        reportList.Add(report);
                    }
                }
                else
                {
                    reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth && x.PeriodType == "期末" && x.CompanyType == "管理公司").ToList();
                    var companylList = reportList.GroupBy(x => new { x.CompanyName }).Where(x => x.Key.CompanyName != null)
                        .Select(x => new { x.Key }).OrderBy(x => x.Key.CompanyName).ToList();
                    foreach (var item in companylList)
                    {
                        listStrCompany.Add(item.Key.CompanyName);
                    }
                }
                //期末数据构造
                {
                    var dataShow = reportList.GroupBy(x => new {x.CompanyType, x.PeriodType, x.CompanyName, x.VehicleModel })
                        .Select(x => new
                        {
                            CompanyType = x.Key.CompanyType,
                            CompanyName = x.Key.CompanyName,
                            VehicleModel = x.Key.VehicleModel,
                            Quantity = x.Sum(s => s.Quantity),
                            PeriodType = x.Key.PeriodType
                        }).OrderBy(x => x.VehicleModel).ToList();
                    for (var i = 0; i < listStrCompany.Count; i++)
                    {
                        var report = new VehicleCheckShowReport();
                        report.PeriodType = "期末";
                        report.CompanyType = "管理公司";
                        report.Sort = 4;
                        report.Company = listStrCompany[i];
                        foreach (var item in listStrVehicleModel)
                        {
                            var mdata = new VehicleModelData();
                            mdata.VehicleModel = item;
                            mdata.Quantity = dataShow
                                .Where(x => x.CompanyName == report.Company && x.VehicleModel == item)
                                .Sum(s => s.Quantity);
                            report.ResultVehicleModelList.Add(mdata);
                        }
                        listReport.Add(report);
                    }
                }
                #endregion
                #region 期初
                if (!isSubmit)
                {
                    reportList.Clear();
                    //查询上个月期末 
                    if (db.Queryable<Business_VehicleCheckReport>().Any(x =>
                        x.YearMonth == LastYearMonth && x.PeriodType == "期末" && x.CompanyType == "管理公司"))
                    {
                        reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == LastYearMonth && x.PeriodType == "期末" && x.CompanyType == "管理公司")
                            .ToList();
                    }
                    //else
                    //{
                    //    foreach (var item in assetModifyLastFlowList)
                    //    {
                    //        var report = new Business_VehicleCheckReport();
                    //        report.YearMonth = YearMonth;
                    //        report.VGUID = Guid.NewGuid();
                    //        report.VehicleModel = item.VEHICLE_SHORTNAME;
                    //        report.CompanyName = item.MANAGEMENT_COMPANY;
                    //        report.Quantity = 1;
                    //        report.CreateDate = DateTime.Now;
                    //        report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                    //        report.PeriodType = "期初";
                    //        report.CompanyType = "管理公司";
                    //        reportList.Add(report);
                    //    }
                    //}
                }
                else
                {
                    reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth && x.PeriodType == "期初" && x.CompanyType == "管理公司")
                        .ToList();
                }
                //期初数据构造
                {
                    var dataShow = reportList.GroupBy(x => new { x.CompanyType, x.PeriodType, x.CompanyName, x.VehicleModel })
                        .Select(x => new
                        {
                            ManageCompany = x.Key.CompanyName,
                            CompanyType = x.Key.CompanyType,
                            VehicleModel = x.Key.VehicleModel,
                            Quantity = x.Sum(s => s.Quantity),
                            PeriodType = x.Key.PeriodType
                        }).OrderBy(x => x.VehicleModel).ToList();
                    for (var i = 0; i < listStrCompany.Count; i++)
                    {
                        var report = new VehicleCheckShowReport();
                        report.PeriodType = "期初";
                        report.CompanyType = "管理公司";
                        report.Sort = 1;
                        report.Company = listStrCompany[i];
                        foreach (var item in listStrVehicleModel)
                        {
                            var mdata = new VehicleModelData();
                            mdata.VehicleModel = item;
                            mdata.Quantity = dataShow
                                .Where(x => x.ManageCompany == report.Company && x.VehicleModel == item)
                                .Sum(s => s.Quantity);
                            report.ResultVehicleModelList.Add(mdata);
                        }
                        listReport.Add(report);
                    }
                }
                #endregion
                #region 增加
                if (!isSubmit)
                {
                    reportList.Clear();
                    YearMonth = YearMonth.Replace("-", "");
                    var addedList = db.Queryable<Business_AssetReview>().Where(x => x.GROUP_ID == "出租车" && x.ISVERIFY).Where(x => x.LISENSING_DATE >= currentDate && x.LISENSING_DATE < nextDate)
                        .ToList();
                    foreach (var item in addedList)
                    {
                        var report = new Business_VehicleCheckReport();
                        report.YearMonth = YearMonth;
                        report.VGUID = Guid.NewGuid();
                        report.VehicleModel = item.VEHICLE_SHORTNAME;
                        report.CompanyName = item.MANAGEMENT_COMPANY;
                        report.Quantity = 1;
                        report.CreateDate = DateTime.Now;
                        report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        report.PeriodType = "增加";
                        reportList.Add(report);
                    }
                }
                else
                {
                    reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth && x.PeriodType == "增加" && x.CompanyType == "管理公司")
                        .ToList();
                }
                //增加数据构造
                {
                    var dataShow = reportList.GroupBy(x => new { x.CompanyType, x.PeriodType, x.CompanyName, x.VehicleModel })
                        .Select(x => new
                        {
                            CompanyName = x.Key.CompanyName,
                            CompanyType = x.Key.CompanyType,
                            VehicleModel = x.Key.VehicleModel,
                            Quantity = x.Sum(s => s.Quantity),
                            PeriodType = x.Key.PeriodType
                        }).OrderBy(x => x.VehicleModel).ToList();

                    for (var i = 0; i < listStrCompany.Count; i++)
                    {
                        var report = new VehicleCheckShowReport();
                        report.PeriodType = "增加";
                        report.CompanyType = "管理公司";
                        report.Sort = 2;
                        report.Company = listStrCompany[i];
                        foreach (var item in listStrVehicleModel)
                        {
                            var mdata = new VehicleModelData();
                            mdata.VehicleModel = item;
                            mdata.Quantity = dataShow
                                .Where(x => x.CompanyName == report.Company && x.VehicleModel == item)
                                .Sum(s => s.Quantity);
                            report.ResultVehicleModelList.Add(mdata);
                        }
                        listReport.Add(report);
                    }
                }
                #endregion
                #region 减少
                if (!isSubmit)
                {
                    reportList.Clear();
                    YearMonth = YearMonth.Replace("-", "");
                    var ReduceList = db.SqlQueryable<Business_ScrapVehicleShowModel>(@"select mi.TAG_NUMBER
                                                                                                 , mi.VEHICLE_SHORTNAME
                                                                                                 , mi.MANAGEMENT_COMPANY
                                                                                                 , mi.BELONGTO_COMPANY
                                                                                                 , mi.ENGINE_NUMBER
                                                                                                 , mv.*
	                                                                                             , mi.EXP_ACCOUNT_SEGMENT
	                                                                                             , mi.COMMISSIONING_DATE as PERIOD
	                                                                                             , mi.QUANTITY
	                                                                                             , mi.ASSET_COST
	                                                                                             , mi.ASSET_ID
	                                                                                             , mi.GROUP_ID
	                                                                                             , mi.BOOK_TYPE_CODE
						                                                                         , mi.LISENSING_DATE
                                                                                            from Business_ScrapVehicle mv
                                                                                            left join Business_AssetMaintenanceInfo mi
                                                                                            on mv.ORIGINALID = mi.ORIGINALID").Where(i => !i.ISVERIFY).ToList();
                    foreach (var item in ReduceList)
                    {
                        var report = new Business_VehicleCheckReport();
                        report.YearMonth = YearMonth;
                        report.VGUID = Guid.NewGuid();
                        report.VehicleModel = item.VEHICLE_SHORTNAME;
                        report.CompanyName = item.MANAGEMENT_COMPANY;
                        report.Quantity = 1;
                        report.CreateDate = DateTime.Now;
                        report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        report.PeriodType = "减少";
                        report.CompanyType = "管理公司";
                        reportList.Add(report);
                    }
                }
                else
                {
                    reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth && x.PeriodType == "减少" && x.CompanyType == "管理公司")
                        .ToList();
                }
                //增加数据构造
                {
                    var dataShow = reportList.GroupBy(x => new { x.CompanyType, x.PeriodType, x.CompanyName, x.VehicleModel })
                        .Select(x => new
                        {
                            CompanyName = x.Key.CompanyName,
                            CompanyType = x.Key.CompanyType,
                            VehicleModel = x.Key.VehicleModel,
                            Quantity = x.Sum(s => s.Quantity),
                            PeriodType = x.Key.PeriodType
                        }).OrderBy(x => x.VehicleModel).ToList();

                    for (var i = 0; i < listStrCompany.Count; i++)
                    {
                        var report = new VehicleCheckShowReport();
                        report.PeriodType = "减少";
                        report.CompanyType = "管理公司";
                        report.Sort = 3;
                        report.Company = listStrCompany[i];
                        foreach (var item in listStrVehicleModel)
                        {
                            var mdata = new VehicleModelData();
                            mdata.VehicleModel = item;
                            mdata.Quantity = dataShow
                                .Where(x => x.CompanyName == report.Company && x.VehicleModel == item)
                                .Sum(s => s.Quantity);
                            report.ResultVehicleModelList.Add(mdata);
                        }
                        listReport.Add(report);
                    }
                }
                #endregion
                #region 期末
                if (!isSubmit)
                {
                    reportList.Clear();
                    foreach (var item in assetModifyCurrentFlowList)
                    {
                        var report = new Business_VehicleCheckReport();
                        report.YearMonth = YearMonth;
                        report.VGUID = Guid.NewGuid();
                        report.VehicleModel = item.VEHICLE_SHORTNAME;
                        report.CompanyName = item.BELONGTO_COMPANY;
                        report.Quantity = 1;
                        report.CreateDate = DateTime.Now;
                        report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        report.PeriodType = "期末";
                        report.CompanyType = "所属公司";
                        reportList.Add(report);
                    }
                }
                else
                {
                    reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth && x.PeriodType == "期末" && x.CompanyType == "所属公司").ToList();
                    var companylList = reportList.GroupBy(x => new { x.CompanyName }).Where(x => x.Key.CompanyName != null)
                        .Select(x => new { x.Key }).OrderBy(x => x.Key.CompanyName).ToList();
                    foreach (var item in companylList)
                    {
                        listStrBelongToCompany.Add(item.Key.CompanyName);
                    }
                }
                //期末数据构造
                {
                    var dataShow = reportList.GroupBy(x => new { x.CompanyType, x.PeriodType, x.CompanyName, x.VehicleModel })
                        .Select(x => new
                        {
                            CompanyName = x.Key.CompanyName,
                            CompanyType = x.Key.CompanyType,
                            VehicleModel = x.Key.VehicleModel,
                            Quantity = x.Sum(s => s.Quantity),
                            PeriodType = x.Key.PeriodType
                        }).OrderBy(x => x.VehicleModel).ToList();
                    for (var i = 0; i < listStrBelongToCompany.Count; i++)
                    {
                        var report = new VehicleCheckShowReport();
                        report.PeriodType = "期末";
                        report.CompanyType = "所属公司";
                        report.Sort = 4;
                        report.Company = listStrBelongToCompany[i];
                        foreach (var item in listStrVehicleModel)
                        {
                            var mdata = new VehicleModelData();
                            mdata.VehicleModel = item;
                            mdata.Quantity = dataShow
                                .Where(x => x.CompanyName == report.Company && x.VehicleModel == item)
                                .Sum(s => s.Quantity);
                            report.ResultVehicleModelList.Add(mdata);
                        }
                        listReport.Add(report);
                    }
                }
                #endregion
                #region 期初
                if (!isSubmit)
                {
                    reportList.Clear();
                    //查询上个月期末 
                    if (db.Queryable<Business_VehicleCheckReport>().Any(x =>
                        x.YearMonth == LastYearMonth && x.PeriodType == "期末" && x.CompanyType == "所属公司"))
                    {
                        reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == LastYearMonth && x.PeriodType == "期末" && x.CompanyType == "所属公司")
                            .ToList();
                    }
                    //else
                    //{
                    //    foreach (var item in assetModifyLastFlowList)
                    //    {
                    //        var report = new Business_VehicleCheckReport();
                    //        report.YearMonth = YearMonth;
                    //        report.VGUID = Guid.NewGuid();
                    //        report.VehicleModel = item.VEHICLE_SHORTNAME;
                    //        report.CompanyName = item.BELONGTO_COMPANY;
                    //        report.CompanyType = "所属公司";
                    //        report.Quantity = 1;
                    //        report.CreateDate = DateTime.Now;
                    //        report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                    //        report.PeriodType = "期初";
                    //        reportList.Add(report);
                    //    }
                    //}
                }
                else
                {
                    reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth && x.PeriodType == "期初" && x.CompanyType == "所属公司")
                        .ToList();
                }
                //期初数据构造
                {
                    var dataShow = reportList.GroupBy(x => new { x.CompanyType, x.PeriodType, x.CompanyName, x.VehicleModel })
                        .Select(x => new
                        {
                            ManageCompany = x.Key.CompanyName,
                            CompanyType = x.Key.CompanyType,
                            VehicleModel = x.Key.VehicleModel,
                            Quantity = x.Sum(s => s.Quantity),
                            PeriodType = x.Key.PeriodType
                        }).OrderBy(x => x.VehicleModel).ToList();

                    for (var i = 0; i < listStrBelongToCompany.Count; i++)
                    {
                        var report = new VehicleCheckShowReport();
                        report.PeriodType = "期初";
                        report.CompanyType = "所属公司";
                        report.Sort = 1;
                        report.Company = listStrBelongToCompany[i];
                        foreach (var item in listStrVehicleModel)
                        {
                            var mdata = new VehicleModelData();
                            mdata.VehicleModel = item;
                            mdata.Quantity = dataShow
                                .Where(x => x.ManageCompany == report.Company && x.VehicleModel == item)
                                .Sum(s => s.Quantity);
                            report.ResultVehicleModelList.Add(mdata);
                        }
                        listReport.Add(report);
                    }
                }
                #endregion
                #region 增加
                if (!isSubmit)
                {
                    reportList.Clear();
                    YearMonth = YearMonth.Replace("-", "");
                    var addedList = db.Queryable<Business_AssetReview>().Where(x => x.GROUP_ID == "出租车" && x.ISVERIFY).Where(x => x.LISENSING_DATE >= currentDate && x.LISENSING_DATE < nextDate)
                        .ToList();
                    foreach (var item in addedList)
                    {
                        var report = new Business_VehicleCheckReport();
                        report.YearMonth = YearMonth;

                        report.VGUID = Guid.NewGuid();
                        report.VehicleModel = item.VEHICLE_SHORTNAME;
                        report.CompanyName = item.BELONGTO_COMPANY;
                        report.Quantity = 1;
                        report.CreateDate = DateTime.Now;
                        report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        report.PeriodType = "增加";
                        report.CompanyType = "所属公司";
                        reportList.Add(report);
                    }
                }
                else
                {
                    reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth && x.PeriodType == "增加" && x.CompanyType == "所属公司")
                        .ToList();
                }
                //增加数据构造
                {
                    var dataShow = reportList.GroupBy(x => new { x.CompanyType, x.PeriodType, x.CompanyName, x.VehicleModel })
                        .Select(x => new
                        {
                            CompanyName = x.Key.CompanyName,
                            CompanyType = x.Key.CompanyType,
                            VehicleModel = x.Key.VehicleModel,
                            Quantity = x.Sum(s => s.Quantity),
                            PeriodType = x.Key.PeriodType
                        }).OrderBy(x => x.VehicleModel).ToList();

                    for (var i = 0; i < listStrBelongToCompany.Count; i++)
                    {
                        var report = new VehicleCheckShowReport();
                        report.PeriodType = "增加";
                        report.CompanyType = "所属公司";
                        report.Sort = 2;
                        report.Company = listStrBelongToCompany[i];
                        foreach (var item in listStrVehicleModel)
                        {
                            var mdata = new VehicleModelData();
                            mdata.VehicleModel = item;
                            mdata.Quantity = dataShow
                                .Where(x => x.CompanyName == report.Company && x.VehicleModel == item)
                                .Sum(s => s.Quantity);
                            report.ResultVehicleModelList.Add(mdata);
                        }
                        listReport.Add(report);
                    }
                }
                #endregion
                #region 减少
                if (!isSubmit)
                {
                    reportList.Clear();
                    YearMonth = YearMonth.Replace("-", "");
                    var ReduceList = db.SqlQueryable<Business_ScrapVehicleShowModel>(@"select mi.TAG_NUMBER
                                                                                                 , mi.VEHICLE_SHORTNAME
                                                                                                 , mi.MANAGEMENT_COMPANY
                                                                                                 , mi.BELONGTO_COMPANY
                                                                                                 , mi.ENGINE_NUMBER
                                                                                                 , mv.*
	                                                                                             , mi.EXP_ACCOUNT_SEGMENT
	                                                                                             , mi.COMMISSIONING_DATE as PERIOD
	                                                                                             , mi.QUANTITY
	                                                                                             , mi.ASSET_COST
	                                                                                             , mi.ASSET_ID
	                                                                                             , mi.GROUP_ID
	                                                                                             , mi.BOOK_TYPE_CODE
						                                                                         , mi.LISENSING_DATE
                                                                                            from Business_ScrapVehicle mv
                                                                                            left join Business_AssetMaintenanceInfo mi
                                                                                            on mv.ORIGINALID = mi.ORIGINALID").Where(i => !i.ISVERIFY).ToList();
                    foreach (var item in ReduceList)
                    {
                        var report = new Business_VehicleCheckReport();
                        report.YearMonth = YearMonth;

                        report.VGUID = Guid.NewGuid();
                        report.VehicleModel = item.VEHICLE_SHORTNAME;
                        report.CompanyName = item.BELONGTO_COMPANY;
                        report.Quantity = 1;
                        report.CreateDate = DateTime.Now;
                        report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        report.PeriodType = "减少";
                        report.CompanyType = "所属公司";
                        reportList.Add(report);
                    }
                }
                else
                {
                    reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth && x.PeriodType == "减少")
                        .ToList();
                }
                //增加数据构造
                {
                    var dataShow = reportList.GroupBy(x => new { x.CompanyType, x.PeriodType, x.CompanyName, x.VehicleModel })
                        .Select(x => new
                        {
                            CompanyName = x.Key.CompanyName,
                            CompanyType = x.Key.CompanyType,
                            VehicleModel = x.Key.VehicleModel,
                            Quantity = x.Sum(s => s.Quantity),
                            PeriodType = x.Key.PeriodType
                        }).OrderBy(x => x.VehicleModel).ToList();

                    for (var i = 0; i < listStrBelongToCompany.Count; i++)
                    {
                        var report = new VehicleCheckShowReport();
                        report.PeriodType = "减少";
                        report.CompanyType = "所属公司";
                        report.Sort = 3;
                        report.Company = listStrBelongToCompany[i];
                        foreach (var item in listStrVehicleModel)
                        {
                            var mdata = new VehicleModelData();
                            mdata.VehicleModel = item;
                            mdata.Quantity = dataShow
                                .Where(x => x.CompanyName == report.Company && x.VehicleModel == item)
                                .Sum(s => s.Quantity);
                            report.ResultVehicleModelList.Add(mdata);
                        }
                        listReport.Add(report);
                    }
                }
                #endregion
            });
            resuleModel.IsSuccess = true;
            resuleModel.Status = "1";
            resuleModel.ResultInfo = listStrVehicleModel;
            resuleModel.ResultInfo2 = listReport.OrderBy(x => x.Sort).ToList();
            var reportManageCompanyCache = CacheManager<List<VehicleCheckShowReport>>.GetInstance();
            reportManageCompanyCache.Remove(PubGet.GetVehicleCheckMangeCompanyReportKey);
            reportManageCompanyCache.Add(PubGet.GetVehicleCheckMangeCompanyReportKey, listReport, 8 * 60 * 60);
            return Json(resuleModel, JsonRequestBehavior.AllowGet);
        }
    }
}