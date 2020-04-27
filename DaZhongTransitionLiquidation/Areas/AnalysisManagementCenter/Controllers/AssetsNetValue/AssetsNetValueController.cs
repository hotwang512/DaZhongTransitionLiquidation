using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
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
        public JsonResult GetManageCompany()
        {
            var list = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                var intArray = new[] { "53", "54", "55", "56" };
                var intList = intArray.ToList();
                list = db.Queryable<Business_SevenSection>().Where(x =>
                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").In("OrgID", intList).ToList();
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAssetOwnerCompany()
        {
            var list = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                var intArray = new[] { "2", "3", "35", "36", "4" };
                var intList = intArray.ToList();
                list = db.Queryable<Business_SevenSection>().Where(x =>
                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").In("OrgID", intList).ToList();
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAssetsNetValueDetail(string DateOfYear, string Month,string ManageCompany,string AssetOwnerCompany, GridParams para)
        {
            var jsonResult = new JsonResultModel<Models.AssetsNetValue>();
            DbBusinessDataService.Command(db =>
            {
                var currentYearMonth = DateOfYear.TryToInt() + "-" + Month;
                var sqlWhere = "";
                if (DateOfYear != "")
                {
                    if (!ManageCompany.IsNullOrEmpty())
                    {
                        sqlWhere += "and FA_LOC_2 ='" + ManageCompany + "' ";
                    }
                    if (!AssetOwnerCompany.IsNullOrEmpty())
                    {
                        sqlWhere += "and FA_LOC_1 ='" + AssetOwnerCompany + "'";
                    }
                    var sql =
                        @"select cte.PERIOD_CODE                              as YearMonth
                         , cte.ASSET_CATEGORY_MAJOR                           as MAJOR
                         , cte.ASSET_CATEGORY_MINOR                           as MINOR
                         , VMODEL
                         , count(1)                                           as ASSETCOUNT
                         , sum(cast(cte.ASSET_COST as decimal(20, 2)))        as COST
                         , sum(cast(cte.ACCT_DEPRECIATION as decimal(20, 2))) as ACCT
                         , sum(cast(cte.PTD_DEPRECIATION as decimal(20, 2))) as PTD
                         , sum(cast(cte.YTD_DEPRECIATION as decimal(20, 2))) as YTD
                    from
                    (
                        select PERIOD_CODE
                             , ASSET_COST
                             , ACCT_DEPRECIATION
                             , ASSET_CATEGORY_MAJOR
                             , ASSET_CATEGORY_MINOR
							 , PTD_DEPRECIATION
							 , YTD_DEPRECIATION
                             , case
                                   when DESCRIPTION like '途安%'
                                        or DESCRIPTION like '桑塔纳%'
                                        or DESCRIPTION like '荣威%' then
                                       DESCRIPTION
                                   else
                                       ''
                               end                                                                              as VMODEL
                             , row_number() over (partition by PERIOD_CODE, ASSET_ID order by CREATE_DATE desc) as id
                        from AssetsLedger_Swap
                        where PERIOD_CODE = '" + currentYearMonth + @"'"+ sqlWhere + @"
                    ) cte
                    where cte.id = 1
                    group by PERIOD_CODE
                           , ASSET_CATEGORY_MAJOR
                           , ASSET_CATEGORY_MINOR
                           , VMODEL
                    union all
                    select cte.PERIOD_CODE                                    as YearMonth
                         , cte.ASSET_CATEGORY_MAJOR                           as MAJOR
                         , '小计'                                               as MINOR
                         , VMODEL
                         , count(1)                                           as ASSETCOUNT
                         , sum(cast(cte.ASSET_COST as decimal(20, 2)))        as COST
                         , sum(cast(cte.ACCT_DEPRECIATION as decimal(20, 2))) as ACCT
                         , sum(cast(cte.PTD_DEPRECIATION as decimal(20, 2))) as PTD
                         , sum(cast(cte.YTD_DEPRECIATION as decimal(20, 2))) as YTD
                    from
                    (
                        select PERIOD_CODE
                             , ASSET_COST
                             , ACCT_DEPRECIATION
                             , ASSET_CATEGORY_MAJOR
                             , ASSET_CATEGORY_MINOR
							 , PTD_DEPRECIATION
							 , YTD_DEPRECIATION
                             , ''                                                                               as VMODEL
                             , row_number() over (partition by PERIOD_CODE, ASSET_ID order by CREATE_DATE desc) as id
                        from AssetsLedger_Swap
                        where PERIOD_CODE = '" + currentYearMonth + @"'" + sqlWhere + @"
                    ) cte
                    where cte.id = 1
                    group by PERIOD_CODE
                           , ASSET_CATEGORY_MAJOR
                           , VMODEL";
                    var list = db.SqlQueryable<Models.AssetsNetValue>(sql).ToList();
                    jsonResult.Rows = list;
                    jsonResult.TotalRows = list.Count();
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}