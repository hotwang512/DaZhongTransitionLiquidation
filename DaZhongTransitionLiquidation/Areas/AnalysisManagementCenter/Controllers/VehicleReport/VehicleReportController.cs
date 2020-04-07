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
            var currentDay = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var currentMonnthDate = YearMonth.TryToDate();
            var nextMonthDate = getEndMonth(currentMonnthDate).AddDays(1);
            var nextDateStr = nextMonthDate.ToString("yyyy-MM-dd");
            var YearMonthDate = YearMonth + "-01";
            var json = "";
            var _db = DbBigDataConfig.GetInstance();
            var sqlStr = @" with cte as (
                        select PeriodType                 = '期初'
                             , Sort                       = 1
                             , CompanyType                = '管理公司'
                             , org.Name                  as CompanyName
                             , info.UsageOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from
                        (
                            select distinct
                                   OriginalId
                                 , License
                                 , LicenseDate
                                 , RetireDate
                                 , OrganizationID
                                 , Name
                                 , VehicleAbbreviationId
                                 , UsageId
                                 , VehicleCategoryId
                                 , OwnOrganizationId
                                 , UsageOrganizationId
                            from Cab.Cab_Base_Info_D
                        )                                         info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
								left join DZSrc.Organization_D org on org.OrganizationId = info.UsageOrganizationId
                        where info.LicenseDate < '" + YearMonthDate + @"'
                              and (info.RetireDate is null or info.RetireDate between '" + YearMonthDate + @"' and '" + currentDay + @"')
							  and info.UsageOrganizationId in(53, 54, 55, 198, 35 , 11749 , 432 , 521)
                        group by info.UsageOrganizationId
                               , org.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                        union all
                        select PeriodType                 = '期初'
                             , Sort                = 1
                             , CompanyType                = '所属公司'
                             , org.Name                  as CompanyName
                             , info.OwnOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from
                        (
                            select distinct
                                   OriginalId
                                 , License
                                 , LicenseDate
                                 , RetireDate
                                 , OrganizationID
                                 , Name
                                 , VehicleAbbreviationId
                                 , UsageId
                                 , VehicleCategoryId
                                 , OwnOrganizationId
                                 , UsageOrganizationId
                            from Cab.Cab_Base_Info_D
                        )                                         info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
								left join DZSrc.Organization_D org on org.OrganizationId = info.OwnOrganizationId
                        where info.LicenseDate < '" + YearMonthDate + @"'
                              and (info.RetireDate is null or info.RetireDate between '" + YearMonthDate + @"' and '" + currentDay + @"')
							  and info.OwnOrganizationId in(53, 54, 55, 198, 35 , 11749 , 432 , 521)
                        group by info.OwnOrganizationId
                               , org.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                        union all
                            select PeriodType             = '增加'
                             , Sort                       = 2
                             , CompanyType                = '管理公司'
                             , org.Name                  as CompanyName
                             , info.UsageOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from
                        (
                            select distinct
                                   OriginalId
                                 , License
                                 , LicenseDate
                                 , RetireDate
                                 , OrganizationID
                                 , Name
                                 , VehicleAbbreviationId
                                 , UsageId
                                 , VehicleCategoryId
                                 , OwnOrganizationId
                                 , UsageOrganizationId
                            from Cab.Cab_Base_Info_D
                        )                                         info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
								left join DZSrc.Organization_D org on org.OrganizationId = info.UsageOrganizationId
                        where info.LicenseDate between '" + YearMonthDate + @"' and '" + nextDateStr + @"'
							  and info.UsageOrganizationId in(53, 54, 55, 198, 35 , 11749 , 432 , 521)
                        group by info.UsageOrganizationId
                               , org.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                        union all
                            select PeriodType             = '增加'
                             , Sort                       = 2
                             , CompanyType                = '所属公司'
                             , org.Name                  as CompanyName
                             , info.OwnOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from
                        (
                            select distinct
                                   OriginalId
                                 , License
                                 , LicenseDate
                                 , RetireDate
                                 , OrganizationID
                                 , Name
                                 , VehicleAbbreviationId
                                 , UsageId
                                 , VehicleCategoryId
                                 , OwnOrganizationId
                                 , UsageOrganizationId
                            from Cab.Cab_Base_Info_D
                        )                                         info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
								left join DZSrc.Organization_D org on org.OrganizationId = info.OwnOrganizationId
                        where info.LicenseDate between '" + YearMonthDate + @"' and '" + nextDateStr + @"'
							  and info.OwnOrganizationId in(53, 54, 55, 198, 35 , 11749 , 432 , 521)
                        group by info.OwnOrganizationId
                               , org.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                        union all
                            select PeriodType                 = '减少'
                                 , Sort                       = 3
                                 , CompanyType                = '管理公司'
                                 , org.Name                  as CompanyName
                                 , info.UsageOrganizationId     as CompanyID
                                 , abbr.VehicleAbbreviationId as VehicleID
                                 , abbr.Name                  as VehicleModel
                                 , count(1)                   as Quantity
                                from
                                (
                                    select distinct
                                           OriginalId
                                         , License
                                         , LicenseDate
                                         , RetireDate
                                         , OrganizationID
                                         , Name
                                         , VehicleAbbreviationId
                                         , UsageId
                                         , VehicleCategoryId
                                         , OwnOrganizationId
                                         , UsageOrganizationId
                                    from Cab.Cab_Base_Info_D
                                )                                         info
                                left join DZSrc.VehicleAbbreviation_D abbr
                                    on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
								left join DZSrc.Organization_D org on org.OrganizationId = info.UsageOrganizationId
                            where info.RetireDate between '" + YearMonthDate + @"' and '" + nextDateStr + @"'
							  and info.UsageOrganizationId in(53, 54, 55, 198, 35 , 11749 , 432 , 521)
                            group by info.UsageOrganizationId
                                   , org.Name
                                   , abbr.VehicleAbbreviationId
                                   , abbr.Name
                        union all
                            select PeriodType                 = '减少'
                                 , Sort                       = 3
                                 , CompanyType                = '所属公司'
                                 , org.Name                  as CompanyName
                                 , info.OwnOrganizationId     as CompanyID
                                 , abbr.VehicleAbbreviationId as VehicleID
                                 , abbr.Name                  as VehicleModel
                                 , count(1)                   as Quantity
                                from
                                (
                                    select distinct
                                           OriginalId
                                         , License
                                         , LicenseDate
                                         , RetireDate
                                         , OrganizationID
                                         , Name
                                         , VehicleAbbreviationId
                                         , UsageId
                                         , VehicleCategoryId
                                         , OwnOrganizationId
                                         , UsageOrganizationId
                                    from Cab.Cab_Base_Info_D
                                )                                         info
                                left join DZSrc.VehicleAbbreviation_D abbr
                                    on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
								left join DZSrc.Organization_D org on org.OrganizationId = info.OwnOrganizationId
                            where info.RetireDate between '" + YearMonthDate + @"' and '" + nextDateStr + @"'
							  and info.OwnOrganizationId in(53, 54, 55, 198, 35 , 11749 , 432 , 521)
                            group by info.OwnOrganizationId
                                   , org.Name
                                   , abbr.VehicleAbbreviationId
                                   , abbr.Name
                        union all
                        select PeriodType                 = '期末'
                             , Sort                       = 4
                             , CompanyType                = '管理公司'
                             , org.Name                  as CompanyName
                             , info.UsageOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from
                        (
                            select distinct
                                   OriginalId
                                 , License
                                 , LicenseDate
                                 , RetireDate
                                 , OrganizationID
                                 , Name
                                 , VehicleAbbreviationId
                                 , UsageId
                                 , VehicleCategoryId
                                 , OwnOrganizationId
                                 , UsageOrganizationId
                            from Cab.Cab_Base_Info_D
                        )                                         info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
								left join DZSrc.Organization_D org on org.OrganizationId = info.UsageOrganizationId
                        where info.LicenseDate < '" + nextMonthDate + @"'
                              and (info.RetireDate is null or info.RetireDate between '" + nextMonthDate + @"' and '" + currentDay + @"')
							  and info.UsageOrganizationId in(53, 54, 55, 198, 35 , 11749 , 432 , 521)
                        group by info.UsageOrganizationId
                               , org.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name
                        union all
                        select PeriodType                 = '期末'
                             , Sort                       = 4
                             , CompanyType                = '所属公司'
                             , org.Name                  as CompanyName
                             , info.OwnOrganizationId     as CompanyID
                             , abbr.VehicleAbbreviationId as VehicleID
                             , abbr.Name                  as VehicleModel
                             , count(1)                   as Quantity
                        from
                        (
                            select distinct
                                   OriginalId
                                 , License
                                 , LicenseDate
                                 , RetireDate
                                 , OrganizationID
                                 , Name
                                 , VehicleAbbreviationId
                                 , UsageId
                                 , VehicleCategoryId
                                 , OwnOrganizationId
                                 , UsageOrganizationId
                            from Cab.Cab_Base_Info_D
                        )                                         info
                            left join DZSrc.VehicleAbbreviation_D abbr
                                on abbr.VehicleAbbreviationId = info.VehicleAbbreviationId
								left join DZSrc.Organization_D org on org.OrganizationId = info.OwnOrganizationId
                        where info.LicenseDate < '" + nextMonthDate + @"'
                              and (info.RetireDate is null or info.RetireDate between '" + nextMonthDate + @"' and'" + currentDay + @"')
							  and info.OwnOrganizationId in(53, 54, 55, 198, 35 , 11749 , 432 , 521)
                        group by info.OwnOrganizationId
                               , org.Name
                               , abbr.VehicleAbbreviationId
                               , abbr.Name)
	                select * from cte order by cte.Sort ,cte.CompanyType,cte.VehicleID";
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