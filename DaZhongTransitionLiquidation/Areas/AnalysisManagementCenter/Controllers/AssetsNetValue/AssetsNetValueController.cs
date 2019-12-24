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

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.AssetsNetValue
{
    public class AssetsNetValueController : BaseController
    {
        // GET: AnalysisManagementCenter/AssetsNetValue
        public AssetsNetValueController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetAssetsNetValueDetail(string DateOfYear, string minMonth, string maxMonth, GridParams para)
        {
            var jsonResult = new JsonResultModel<Models.AssetsNetValue>();
            DbBusinessDataService.Command(db =>
            {
                var currentYear = DateOfYear.TryToInt();
                var nextYear = (currentYear + 1).ToString();
                var minYearMonth = DateOfYear + "-01-01";
                var maxYearMonth = (nextYear + "-01-01").TryToDate().ToString("yyyy-MM-dd");
                if (DateOfYear != "")
                {
                    var list = db.SqlQueryable<Models.AssetsNetValue>(
                        @"select datename(yyyy, LISENSING_DATE) + datename(mm, LISENSING_DATE) as YearMonth
                             , ASSET_CATEGORY_MAJOR                                              as MAJOR
                             , ASSET_CATEGORY_MINOR                                              as MINOR
                             , case  when VEHICLE_SHORTNAME is null then '' when VEHICLE_SHORTNAME = 'OBD' then '' else VEHICLE_SHORTNAME end  as VMODEL  
                             , count(1)                                                          as ASSETCOUNT
                             , sum(ASSET_COST)                                                   as COST
                             , sum(ACCT_DEPRECIATION)                                            as ACCT
                             , 0                                                                 as DEVALUE
                        from dbo.Business_AssetMaintenanceInfo
                        where LISENSING_DATE > '" + minYearMonth + @"' and LISENSING_DATE < '" + maxYearMonth + @"'
                        group by datename(yyyy, LISENSING_DATE) + datename(mm, LISENSING_DATE)
                               , ASSET_CATEGORY_MAJOR
                               , ASSET_CATEGORY_MINOR
                               , VEHICLE_SHORTNAME").ToList();
                    jsonResult.Rows = list;
                    jsonResult.TotalRows = list.Count();
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}