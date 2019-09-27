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
        public JsonResult GetAssetReportPeriodListDatas(string YearMonth, string PeriodType, GridParams para)
        {
            var date = YearMonth.TryToDate();
            if (PeriodType == "StartPeriod")
            {
                date = date.AddMonths(-1);
                YearMonth = date.Year.ToString() + date.Month.ToString().PadLeft(2,'0');
            }
            else
            {
                YearMonth = YearMonth.Replace("-", "");
            }
            var cache = CacheManager<Sys_User>.GetInstance();
            List<Api_ModifyVehicleAsset> assetModifyFlowList = new List<Api_ModifyVehicleAsset>();
            var reportList = new List<Business_VehicleCheckReport>();
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
                    //获取所有的公司
                    var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                        x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                    foreach (var item in assetModifyFlowList)
                    {
                        if (item.BELONGTO_COMPANY.TryToInt() < 100 && item.MANAGEMENT_COMPANY.TryToInt() < 100)
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
                            var report = new Business_VehicleCheckReport();
                            report.YearMonth = YearMonth;
                            report.Orginalid = item.ORIGINALID;
                            report.VGUID = Guid.NewGuid();
                            report.VehicleModel = item.VEHICLE_SHORTNAME;
                            report.ManageCompany = item.MANAGEMENT_COMPANY;
                            report.BelongToCompany = item.BELONGTO_COMPANY;
                            report.Quantity = 1;
                            report.CreateDate = DateTime.Now;
                            report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                            reportList.Add(report);
                        }
                    }
                    var reportCache = CacheManager<List<Business_VehicleCheckReport>>.GetInstance();
                    reportCache.Remove(PubGet.GetVehicleCheckReportKey);
                    reportCache.Add(PubGet.GetVehicleCheckReportKey, reportList, 8 * 60 * 60);
                }
                else
                {
                    reportList = db.Queryable<Business_VehicleCheckReport>().Where(x => x.YearMonth == YearMonth)
                        .ToList();
                }
            });
            return Json(reportList.OrderBy(x => x.VehicleModel).ToList(), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="YearMonth">年月</param>
        /// <param name="ShowType">增加减少</param>
        /// <param name="para"></param>
        /// <returns></returns>
        public JsonResult GetAssetReportChangedListDatas(string YearMonth, string ShowType, GridParams para)
        {
            YearMonth = YearMonth.Replace("-", "");
            var cache = CacheManager<Sys_User>.GetInstance();
            List<Api_ModifyVehicleAsset> assetModifyFlowList = new List<Api_ModifyVehicleAsset>();
            var reportAddedList = new List<Business_VehicleCheckChangeReport>();
            var reportReduceList = new List<Business_VehicleCheckChangeReport>();
            DbBusinessDataService.Command(db =>
            {
                if (ShowType == "Added")
                {
                    if (!db.Queryable<Business_VehicleCheckChangeReport>().Any(x => x.YearMonth == YearMonth && x.ChangeType == ShowType))
                    {
                        YearMonth = YearMonth.Replace("-", "");
                        var addedList = db.Queryable<Business_AssetReview>().Where(x => x.GROUP_ID == "出租车" && !x.ISVERIFY)
                            .ToList();
                        foreach (var item in addedList)
                        {
                            var report = new Business_VehicleCheckChangeReport();
                            report.YearMonth = YearMonth;
                            report.Orginalid = item.ORIGINALID;
                            report.VGUID = Guid.NewGuid();
                            report.VehicleModel = item.VEHICLE_SHORTNAME;
                            report.ManageCompany = item.MANAGEMENT_COMPANY;
                            report.BelongToCompany = item.BELONGTO_COMPANY;
                            report.ChangeType = ShowType;
                            report.Quantity = 1;
                            report.CreateDate = DateTime.Now;
                            report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                            reportAddedList.Add(report);
                        }
                        var reportAddedCache = CacheManager<List<Business_VehicleCheckChangeReport>>.GetInstance();
                        reportAddedCache.Remove(PubGet.GetVehicleCheckReportKey);
                        reportAddedCache.Add(PubGet.GetVehicleCheckReportKey, reportAddedList, 8 * 60 * 60);
                    }
                    else
                    {
                        reportAddedList = db.Queryable<Business_VehicleCheckChangeReport>().Where(x => x.YearMonth == YearMonth && x.ChangeType == ShowType)
                            .ToList();
                    }
                }else if (ShowType == "Reduce")
                {
                    if (!db.Queryable<Business_VehicleCheckChangeReport>().Any(x => x.YearMonth == YearMonth && x.ChangeType == ShowType))
                    {
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
                            var report = new Business_VehicleCheckChangeReport();
                            report.YearMonth = YearMonth;
                            report.Orginalid = item.ORIGINALID;
                            report.VGUID = Guid.NewGuid();
                            report.VehicleModel = item.VEHICLE_SHORTNAME;
                            report.ManageCompany = item.MANAGEMENT_COMPANY;
                            report.BelongToCompany = item.BELONGTO_COMPANY;
                            report.ChangeType = ShowType;
                            report.Quantity = 1;
                            report.CreateDate = DateTime.Now;
                            report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                            reportAddedList.Add(report);
                        }
                        var reportReduceCache = CacheManager<List<Business_VehicleCheckChangeReport>>.GetInstance();
                        reportReduceCache.Remove(PubGet.GetVehicleCheckReportKey);
                        reportReduceCache.Add(PubGet.GetVehicleCheckReportKey, reportAddedList, 8 * 60 * 60);
                    }
                    else
                    {
                        reportAddedList = db.Queryable<Business_VehicleCheckChangeReport>().Where(x => x.YearMonth == YearMonth && x.ChangeType == ShowType)
                            .ToList();
                    }
                }

            });
            return Json(reportAddedList.OrderBy(x => x.VehicleModel).ToList(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult SubmitAssetReport(string YearMonth)
        {
            YearMonth = YearMonth.Replace("-", "");
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var reportCache = CacheManager<List<Business_VehicleCheckReport>>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                if (!db.Queryable<Business_VehicleCheckReport>().Any(x => x.YearMonth == YearMonth))
                {
                    var list = reportCache[PubGet.GetVehicleCheckReportKey];
                    db.Insertable<Business_VehicleCheckReport>(list).ExecuteCommand();
                    resultModel.IsSuccess = true;
                    resultModel.Status = "1";
                }
                else
                {
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}