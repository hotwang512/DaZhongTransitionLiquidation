using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.UnUsedVehicleStatistics
{
    public class UnUsedVehicleStatisticsController : BaseController
    {
        // GET: AnalysisManagementCenter/UnUsedVehicleStatistics
        public UnUsedVehicleStatisticsController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetUnUsedVehicleStatistics(string Year, GridParams para)
        {
            var _db = DbBigDataConfig.GetInstance();
            var sqlStr = @"select Period
                         , case when Organization.Name like '%新亚%' then '分管单位' when Organization.Name like '%星光%' then '分管单位' else '营运公司' end as OrganizationType
                         , '管理公司' as CompanyType
	                     , Cab_Work_Info.OrganizationId as CompanyCode
                         , Organization.Name            as CompanyName
                         , count(1)                     as Quantity
                         , sum(   case Cab_Work_Info.OperationStatus
                                      when 0 then
                                          1
                                      else
                                          0
                                  end
                              )                         as StopVehicleQuantity
                    from Cab.Cab_Work_Info_ByPeriod_FH Cab_Work_Info
                        left join Cab.Cab_Base_Info_D  Cab
                            on Cab_Work_Info.CabId = Cab.CabId
                        left join DZSrc.Organization_D Organization
                            on Cab.OrganizationID = Organization.OrganizationId
                    where Period like '"+ Year + @"%'
                          and Cab_Work_Info.OrganizationId in ( 53, 54, 55, 198, 451 )
                    group by Cab_Work_Info.Period
                           , Cab_Work_Info.OrganizationId
                           , Organization.Name";
            var data = _db.SqlQueryable<Models.UnUsedVehicleStatistics>(sqlStr).ToList();
            var sqlStr1 = @"select Period
                             , '营运公司' as OrganizationType
                             , '资产公司' as CompanyType
	                         , Cab_Work_Info.OrganizationId as CompanyCode
                             , Organization.Name            as CompanyName
                             , count(1)                     as Quantity
                             , sum(   case Cab_Work_Info.OperationStatus
                                          when 0 then
                                              1
                                          else
                                              0
                                      end
                                  )                         as StopVehicleQuantity
                        from Cab.Cab_Work_Info_ByPeriod_FH Cab_Work_Info
                            left join Cab.Cab_Base_Info_D  Cab
                                on Cab_Work_Info.CabId = Cab.CabId
                            left join DZSrc.Organization_D Organization
                                on Cab.OrganizationID = Organization.OrganizationId
                        where Period like '" + Year + @"%'
                              and Cab_Work_Info.OrganizationId in ( 53, 54, 55 )
                        group by Cab_Work_Info.Period
                               , Cab_Work_Info.OrganizationId
                               , Organization.Name";
            var data1 = _db.SqlQueryable<Models.UnUsedVehicleStatistics>(sqlStr1).ToList();
            var unUsedVehicleStatisticsList = data.Union(data1);
            var db = DbBusinessDataConfig.GetInstance();
            var sqlAmountStr = @"select master2.CompanyCode
                                 , report.DateOfYear + right('0' + cast(report.YearMonth as nvarchar(50)), 2) as Period
                                 , report.CompanyName
                                 , report.LicenseAmount
                            from Business_VehicleAmountReport report
                                left join
                                (
                                    select DESC0  as CompanyName
                                         , Param4 as CompanyCode
                                         , LGUID
                                    from DEV_DaZhong_ReckoningSystem.dbo.CS_Master_2
                                    where VGUID = 'B0A3E68C-5A7F-4C86-8A68-4085971D3276'
                                )                             as master2
                                    on master2.LGUID = report.CompanyGuid
                            where report.DateOfYear = '" + Year + "'";
            var licenseAmountList = db.SqlQueryable<Models.LicenseAmountModel>(sqlAmountStr).ToList();
            foreach (var item in unUsedVehicleStatisticsList)
            {
                if (licenseAmountList.Any(x => x.CompanyCode == item.CompanyCode && x.Period == item.Period))
                {
                    if (!licenseAmountList.First(x => x.CompanyCode == item.CompanyCode && x.Period == item.Period).LicenseAmount.IsNullOrEmpty())
                    {
                        item.StopLicence = licenseAmountList.First(x => x.CompanyCode == item.CompanyCode && x.Period == item.Period).LicenseAmount - item.Quantity;
                    }
                }
            }
            return Json(unUsedVehicleStatisticsList, JsonRequestBehavior.AllowGet);
        }
    }
}