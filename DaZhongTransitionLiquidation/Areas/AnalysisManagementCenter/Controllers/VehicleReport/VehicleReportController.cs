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
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.VehicleReport
{
    public class VehicleReportController : BaseController
    {
        // GET: AnalysisManagementCenter/VehicleReport
        public VehicleReportController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetVehicleModel(string YearMonth)
        {
            var listType = new List<string>();
            List<Api_ModifyVehicleAsset> assetModifyFlowList = new List<Api_ModifyVehicleAsset>();
            var _db = DbBigDataConfig.GetInstance();
            var vehicleModelList = _db.SqlQueryable<dynamic>(
                    @"SELECT distinct model.Name FROM  DZSrc.VehicleAbbreviation_D model inner join Cab.Cab_Base_Info_D info on model.VehicleAbbreviationId = info.VehicleAbbreviationId where info.RetireDate is not null")
                .ToList();
            foreach (var item in vehicleModelList)
            {
                listType.Add(item.Name);
            }
            return Json(listType.ToList(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetManageCompanyVehicleReport(string YearMonth, GridParams para)
        {
            var currentDate = YearMonth.TryToDate();
            var YearMonthDate = YearMonth + "-01";
            var cache = CacheManager<Sys_User>.GetInstance();
            var reportList = new List<Business_VehicleCheckReport>();
            var listReport = new List<VehicleCheckShowReport>();
            var listStrCompany = new List<string>();
            var listStrBelongToCompany = new List<string>();
            var listType = new List<string>();
            var listStrVehicleModel = new List<string>();
            var json = "";
            var _db = DbBigDataConfig.GetInstance();
            var vehicleModelList = _db.SqlQueryable<dynamic>(
                    @"SELECT distinct model.Name FROM  DZSrc.VehicleAbbreviation_D model inner join Cab.Cab_Base_Info_D info on model.VehicleAbbreviationId = info.VehicleAbbreviationId where info.RetireDate is not null")
                .ToList();
            foreach (var item in vehicleModelList)
            {
                listType.Add(item.Name);
            }
            var pivotStr = JoinStr(listType);
            var data = _db.Ado.GetDataTable(
                @"select *
                    from
                    (
                        select PeriodType                 = '期初'
                             , CompanyType                = '管理公司'
                             , info.Name                  as CompanyName
                             , info.OwnOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from Cab.Cab_Base_Info_D                  info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
                        where info.LicenseDate < '" + YearMonthDate + @"'
                              and info.RetireDate is not null
                              and info.RetireDate < '" + YearMonthDate + @"'
                        group by info.OwnOrganizationId
                               , info.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                        union all
                        select PeriodType                 = '期初'
                             , CompanyType                = '所属公司'
                             , info.Name                  as CompanyName
                             , info.UsageOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from Cab.Cab_Base_Info_D                  info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
                        where info.LicenseDate < '" + YearMonthDate + @"'
                              and info.RetireDate is not null
                              and info.RetireDate < '" + YearMonthDate + @"'
                        group by info.UsageOrganizationId
                               , info.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                        union all
                        select PeriodType                 = '期末'
                             , CompanyType                = '管理公司'
                             , info.Name                  as CompanyName
                             , info.UsageOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from Cab.Cab_Base_Info_D                  info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
                        where info.LicenseDate < '" + YearMonthDate + @"'
                              and info.RetireDate is not null
                              and info.RetireDate < '" + YearMonthDate + @"'
                        group by info.UsageOrganizationId
                               , info.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                        union all
                        select PeriodType                 = '期末'
                             , CompanyType                = '所属公司'
                             , info.Name                  as CompanyName
                             , info.UsageOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from Cab.Cab_Base_Info_D                  info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
                        where info.LicenseDate < '" + YearMonthDate + @"'
                              and info.RetireDate is not null
                              and info.RetireDate < '" + YearMonthDate + @"'
                        group by info.UsageOrganizationId
                               , info.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                    ) a
                    pivot
                    (
                        sum(Quantity)
                        for VehicleModel in (" + pivotStr + @")
                    ) b
                    ");
            json = data.DataTableToJson();
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public string JoinStr(List<string> list)
        {
            var strArr = "";
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    strArr = strArr + "[" + item + "],";
                }
                strArr = strArr.Substring(0, strArr.Length - 1);
                return strArr;
            }
            else
            {
                return strArr;
            }
        }
    }
}