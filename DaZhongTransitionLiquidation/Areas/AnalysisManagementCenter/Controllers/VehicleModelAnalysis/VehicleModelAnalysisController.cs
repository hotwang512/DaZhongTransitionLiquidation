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

            var lastYearDate = (Year.TryToInt() - 1).ToString() + "-12-31" ;
            var currentYearDate = (Year.TryToInt() + 1) + "-01-01" ;
            var _db = DbBigDataConfig.GetInstance();
            var sqlStr = @"select CompanyType = '管理公司'                                       
                             , org.Name                                                          as CompanyName
                             , datename(yyyy, info.LicenseDate) + datename(mm, info.LicenseDate) as YearMonth
                             , info.UsageOrganizationId                                          as CompanyID
                             , abbr.VehicleAbbreviationId                                        as VehicleID
                             , abbr.Name                                                         as VehicleModel
                             , count(1)                                                          as Quantity
                             , (select count(1) from  (SELECT distinct UsageOrganizationId,License,LicenseDate,VehicleAbbreviationId FROM Cab.Cab_Base_Info_D) info where LicenseDate > '" + lastYearDate + @"' and LicenseDate < '" + currentYearDate + @"'and UsageOrganizationId in ( 53, 54, 55, 198, 35, 11749, 432, 521 )) as Total
                             , cast(count(1) * 100 / (select count(1) from (SELECT distinct UsageOrganizationId,License,LicenseDate,VehicleAbbreviationId FROM Cab.Cab_Base_Info_D) info where LicenseDate > '" + lastYearDate + @"'and LicenseDate <  '" + currentYearDate + @"'and UsageOrganizationId in ( 53, 54, 55, 198, 35, 11749, 432, 521 )) as decimal(18,6)) as [Percent]
                        from (SELECT distinct UsageOrganizationId,License,LicenseDate,VehicleAbbreviationId FROM Cab.Cab_Base_Info_D) info
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
                             , info.AssetOwnerId                                          as CompanyID
                             , abbr.VehicleAbbreviationId                                        as VehicleID
                             , abbr.Name                                                         as VehicleModel
                             , count(1)                                                          as Quantity
                             , (select count(1) from  (SELECT distinct UsageOrganizationId,License,LicenseDate,VehicleAbbreviationId FROM Cab.Cab_Base_Info_D) info where LicenseDate > '" + lastYearDate + @"' and LicenseDate < '" + currentYearDate + @"'and UsageOrganizationId in ( 53, 54, 55, 198, 35, 11749, 432, 521 )) as Total
                             , cast(count(1) * 100 / (select count(1) from  (SELECT distinct UsageOrganizationId,License,LicenseDate,VehicleAbbreviationId FROM Cab.Cab_Base_Info_D) info where LicenseDate > '" + lastYearDate + @"'and LicenseDate <  '" + currentYearDate + @"'and UsageOrganizationId in ( 53, 54, 55, 198, 35, 11749, 432, 521 )) as decimal(18,6)) as [Percent]
                        from (SELECT distinct UsageOrganizationId,AssetOwnerId,License,LicenseDate,VehicleAbbreviationId FROM Cab.Cab_Base_Info_D) info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
                            left join DZSrc.Owner_D org on org.ID = info.AssetOwnerId
                        where info.LicenseDate > '" + lastYearDate + @"'
                              and info.LicenseDate < '" + currentYearDate + @"'
                              and info.UsageOrganizationId in ( 53, 54, 55, 198, 35, 11749, 432, 521 )
                        group by datename(yyyy, info.LicenseDate) + datename(mm, info.LicenseDate)
                               , info.AssetOwnerId
                               , org.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name";
            var data = _db.SqlQueryable<Models.VehicleModelAnalysis>(sqlStr).ToList();
            data = SetData(data, "管理公司");
            data = SetData(data, "所属公司");
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public List<Models.VehicleModelAnalysis> SetData(List<Models.VehicleModelAnalysis> data,string CompanyType)
        {
            var yearMonths = data.Where(x => x.CompanyType == CompanyType).GroupBy(x => x.YearMonth).Select(x => x.Key);
            foreach (var item in yearMonths)
            {
                var totals = data.Where(x => x.YearMonth == item && x.CompanyType == CompanyType).Sum(x => x.Quantity);
                foreach (var dataitem in data.Where(x => x.YearMonth == item && x.CompanyType == CompanyType))
                {
                    dataitem.Total = totals;
                }
            }
            return data;
        }
    }
}