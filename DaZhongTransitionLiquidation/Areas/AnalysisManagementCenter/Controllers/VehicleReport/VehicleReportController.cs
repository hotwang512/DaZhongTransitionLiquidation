﻿using System;
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
            var nextDate = getEndMonth(currentDate);
            var nextDateStr = nextDate.ToString("yyyy-MM-dd");
            var YearMonthDate = YearMonth + "-01";
            var json = "";
            var _db = DbBigDataConfig.GetInstance();
            var sqlStr = @"select PeriodType                 = '期初'
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
                            select PeriodType             = '增加'
                             , CompanyType                = '管理公司'
                             , info.Name                  as CompanyName
                             , info.OwnOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                            
                             , count(1)                   as Quantity
                        from Cab.Cab_Base_Info_D                  info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
                        where info.LicenseDate between '" + YearMonthDate + @"' and '" + nextDateStr + @"'
                        group by info.OwnOrganizationId
                               , info.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                        union all
                            select PeriodType                 = '减少'
                                 , CompanyType                = '管理公司'
                                 , info.Name                  as CompanyName
                                 , info.OwnOrganizationId     as CompanyID
                                 , abbr.VehicleAbbreviationId as VehicleID
                                 , abbr.Name                  as VehicleModel
                                
                                 , count(1)                   as Quantity
                            from Cab.Cab_Base_Info_D                  info
                                left join DZSrc.VehicleAbbreviation_D abbr
                                    on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
                            where info.RetireDate between '" + YearMonthDate + @"' and '" + nextDateStr + @"'
                            group by info.OwnOrganizationId
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
                               , abbr.Name";
            var data = _db.Ado.GetDataTable(sqlStr);
            json = data.DataTableToJson();
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public static DateTime getEndMonth(DateTime date)
        {
            int span = Convert.ToInt32(date.Day);
            var d1 = date.AddMonths(1).AddDays(-span);
            return d1;
        }
    }
}