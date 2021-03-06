﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.OutageReport
{
    public class OutageReportController : BaseController
    {
        // GET: AnalysisManagementCenter/VehicleReport
        public OutageReportController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetManageCompanyOutageReport(string YearMonth, GridParams para)
        {
            var _db = DbBigDataConfig.GetInstance();
            var sqlStr = @"select Period
	                 ,abbr.Name as VehicleModel
                     , Organization.Name         as CompanyName
                     , Quantity                  = 1
                from Cab.Cab_Work_Info_ByPeriod_FH        Cab_Work_Info
                    left join Cab.Cab_Base_Info_D         Cab
                        on Cab_Work_Info.CabId = Cab.CabId
                    left join DZSrc.Organization_D        Organization
                        on Cab.OrganizationID = Organization.OrganizationId
                    left join DZSrc.VehicleAbbreviation_D abbr
                        on abbr.VehicleAbbreviationId = Cab_Work_Info.VehicleAbbreviationId
                where Period like '" + YearMonth + @"%'
                      and Cab_Work_Info.OrganizationId in ( 53, 54, 55, 198, 451 )
                      and Cab_Work_Info.OperationStatus = 0";
            var data = _db.SqlQueryable<Models.OutageReport>(sqlStr).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetUseCompanyOutageReport(string YearMonth, GridParams para)
        {
            var _db = DbBigDataConfig.GetInstance();
            var sqlStr = @"select Period
	                    ,abbr.Name as VehicleModel
                        , org.Name         as CompanyName
                        , Quantity                  = 1
                from Cab.Cab_Work_Info_ByPeriod_FH        Cab_Work_Info
                    left join Cab.Cab_Base_Info_D         Cab
                        on Cab_Work_Info.CabId = Cab.CabId
                    left join DZSrc.Owner_D org on org.ID = Cab.AssetOwnerId
                    left join DZSrc.VehicleAbbreviation_D abbr
                        on abbr.VehicleAbbreviationId = Cab_Work_Info.VehicleAbbreviationId
                where Period like '" + YearMonth + @"%'
                      and Cab_Work_Info.OrganizationId in ( 53, 54, 55, 198, 451 )
                      and Cab_Work_Info.OperationStatus = 0";
            var data = _db.SqlQueryable<Models.OutageReport>(sqlStr).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}