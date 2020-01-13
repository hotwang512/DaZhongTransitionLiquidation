using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.VehicleAgeReport
{
    public class VehicleAgeReportController : BaseController
    {
        // GET: AnalysisManagementCenter/VehicleAgeReport
        public VehicleAgeReportController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetVehicleAgeReport(string DateOfYear, GridParams para)
        {
            var jsonResult = new JsonResultModel<Models.VehicleAgeReport>();
            DbBusinessDataService.Command(db =>
            {
                var currentYear = DateOfYear.TryToInt();
                var nextYear = (currentYear + 1).ToString();
                var minYearMonth = DateOfYear + "-01-01";
                var maxYearMonth = (nextYear + "-01-01").TryToDate().ToString("yyyy-MM-dd");
                if (DateOfYear != "")
                {
                    var sqlStr = @"select [FA_LOC_1]
                                 , [FA_LOC_2]
                                 , [DESCRIPTION]
                                 , PERIOD_CODE
                                 , case
                                       when MONTHS > 0
                                            and MONTHS <= 12 then
                                           '1-12个月'
                                       when MONTHS > 12
                                            and MONTHS <= 24 then
                                           '13-24个月'
                                       when MONTHS > 24
                                            and MONTHS <= 36 then
                                           '25-36个月'
                                       when MONTHS > 36
                                            and MONTHS <= 48 then
                                           '37-48个月'
                                       when MONTHS > 48
                                            and MONTHS <= 60 then
                                           '49-60个月'
                                       when MONTHS > 60
                                            and MONTHS <= 72 then
                                           '61-72个月'
                                       else
                                           '其它'
                                   end          as MONTHS
                                 , [QUANTITY]
                            from
                            (
                                select [FA_LOC_1]
                                     , [FA_LOC_2]
                                     , ASSET_CATEGORY_MAJOR
                                     , [DESCRIPTION]
                                     , ASSET_CREATION_DATE
                                     , PERIOD_CODE
                                     , [QUANTITY]
                                     , datediff(m, ASSET_CREATION_DATE, convert(datetime, PERIOD_CODE + '-01'))         as MONTHS
                                     , row_number() over (partition by PERIOD_CODE, ASSET_ID order by CREATE_DATE desc) as id
                                from AssetsLedger_Swap
                                where ASSET_CATEGORY_MAJOR = '专用工具'
                                      and PERIOD_CODE like '" + DateOfYear + @"%'
                            ) cte
                            where cte.id = 1";
                    var list = db.SqlQueryable<Models.VehicleAgeReport>(sqlStr).ToList();
                    jsonResult.Rows = list;
                    jsonResult.TotalRows = list.Count();
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}