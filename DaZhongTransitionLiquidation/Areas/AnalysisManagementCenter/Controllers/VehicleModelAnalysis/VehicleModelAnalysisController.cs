using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.VehicleModelAnalysis
{
    public class VehicleModelAnalysisController : BaseController
    {
        // GET: AnalysisManagementCenter/VehicleReport
        public VehicleModelAnalysisController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetManageCompanyVehicleModelAnalysis(string Year, GridParams para)
        {

            var lastYearDate = (Year.TryToInt() - 1).ToString() + "-12-01" ;
            var currentYearDate = (Year.TryToInt() + 1) + "-01-01" ;
            var _db = DbBigDataConfig.GetInstance();
            var sqlStr = @"select CompanyType = '管理公司'                                       
                             , org.Name                                                          as CompanyName
                             , datename(yyyy, info.LicenseDate) + datename(mm, info.LicenseDate) as YearMonth
                             , info.UsageOrganizationId                                          as CompanyID
                             , abbr.VehicleAbbreviationId                                        as VehicleID
                             , abbr.Name                                                         as VehicleModel
                             , count(1)                                                          as Quantity
                        from Cab.Cab_Base_Info_D                  info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
                            left join DZSrc.Organization_D        org
                                on org.OrganizationId = info.UsageOrganizationId
                        where info.LicenseDate > '" + lastYearDate + @"'
                              and info.LicenseDate < '"+ currentYearDate + @"'
                              and info.UsageOrganizationId in ( 53, 54, 55, 198, 35, 11749, 432, 521 )
                        group by datename(yyyy, info.LicenseDate) + datename(mm, info.LicenseDate)
                               , info.UsageOrganizationId
                               , org.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
		                       union all
		                 select CompanyType = '所属公司'
                             , org.Name                                                          as CompanyName
                             , datename(yyyy, info.LicenseDate) + datename(mm, info.LicenseDate) as YearMonth
                             , info.OwnOrganizationId                                          as CompanyID
                             , abbr.VehicleAbbreviationId                                        as VehicleID
                             , abbr.Name                                                         as VehicleModel
                             , count(1)                                                          as Quantity
                        from Cab.Cab_Base_Info_D                  info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
                            left join DZSrc.Organization_D        org
                                on org.OrganizationId = info.OwnOrganizationId
                        where info.LicenseDate > '" + lastYearDate + @"'
                              and info.LicenseDate < '" + currentYearDate + @"'
                              and info.OwnOrganizationId in ( 53, 54, 55, 198, 35, 11749, 432, 521 )
                        group by datename(yyyy, info.LicenseDate) + datename(mm, info.LicenseDate)
                               , info.OwnOrganizationId
                               , org.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name";
            var data = _db.SqlQueryable<Models.VehicleModelAnalysis>(sqlStr).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}