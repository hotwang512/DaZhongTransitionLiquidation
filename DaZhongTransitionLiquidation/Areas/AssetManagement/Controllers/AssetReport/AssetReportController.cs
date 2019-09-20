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

        public JsonResult GetAssetReportListDatas(string YearMonth,GridParams para)
        {
            YearMonth = YearMonth.Replace("-", "");
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
                        //Code转名称
                        if (ssList.Any(x => x.OrgID == item.MANAGEMENT_COMPANY))
                        {
                            item.MANAGEMENT_COMPANY =
                                ssList.First(x => x.OrgID == item.MANAGEMENT_COMPANY).Abbreviation;
                        }
                        var report = new Business_VehicleCheckReport();
                        report.YearMonth = YearMonth;
                        report.Orginalid = item.ORIGINALID;
                        report.VGUID = Guid.NewGuid();
                        report.VehicleModel = item.VEHICLE_SHORTNAME;
                        report.ManageCompany = item.MANAGEMENT_COMPANY;
                        report.Quantity = 1;
                        report.CreateDate = DateTime.Now;
                        report.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        reportList.Add(report);
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