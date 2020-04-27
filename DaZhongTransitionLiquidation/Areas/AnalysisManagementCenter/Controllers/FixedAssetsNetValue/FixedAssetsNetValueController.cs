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

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.FixedAssetsNetValue
{
    public class FixedAssetsNetValueController : BaseController
    {
        // GET: AnalysisManagementCenter/FixedAssetsNetValue
        public FixedAssetsNetValueController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
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
                var intArray = new[] { "53", "54", "55" };
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
        public JsonResult GetFixedAssetsNetValueDetail(string DateOfYear, string minMonth, string maxMonth, string ManageCompany, string AssetOwnerCompany, GridParams para)
        {
            var jsonResult = new JsonResultModel<Models.FixedAssetsNetValue>();
            var sqlWhere = "";
            DbBusinessDataService.Command(db =>
            {
                if (!ManageCompany.IsNullOrEmpty())
                {
                    sqlWhere += "and FA_LOC_2 ='" + ManageCompany + "' ";
                }
                if (!AssetOwnerCompany.IsNullOrEmpty())
                {
                    sqlWhere += "and FA_LOC_1 ='" + AssetOwnerCompany + "'";
                }
                var currentYear = DateOfYear.TryToInt();
                var lastYear = (currentYear - 1).ToString();
                var nextYear = (currentYear + 1).ToString();
                var minYearMonth = DateOfYear + " - " + minMonth.PadLeft(2, '0') + "-01";
                var maxYearMonth = (DateOfYear + " - " + maxMonth.PadLeft(2, '0') + "-01").TryToDate().AddMonths(1).ToString("yyyy-MM-dd");
                if (DateOfYear != "")
                {
                    var list = new List<Models.FixedAssetsNetValue>();
                    var _db = DbConfig.GetInstance();
                    var masterData = _db.Queryable<CS_Master_2>()
                        .Where(x => x.VGUID == "b7c32141-a457-44fc-94de-4a733c361902".TryToGuid()).OrderBy(x => x.MasterCode).ToList();
                    foreach (var item in masterData)
                    {
                        var netValue = new Models.FixedAssetsNetValue();
                        netValue.MainCategory = item.DESC0;
                        netValue.Zorder = item.MasterCode.TryToInt();
                        netValue.CalculationType = item.Param2;
                        netValue.CategoryType = item.Param1;
                        list.Add(netValue);
                    }
                    //年初数
                    var startPeriodList = db.SqlQueryable<FixedAssetsNetValueModel>(
                        @"SELECT ASSET_CATEGORY_MAJOR as MAJOR,sum(ASSET_COST) as COST,sum(ACCT_DEPRECIATION) as ACCT, '期初' as PeriodType  FROM dbo.AssetsLedger_Swap
                        where PERIOD_CODE =  '" + lastYear + @"-12' " + sqlWhere + @"
                    group by ASSET_CATEGORY_MAJOR").ToList();
                    //本期增加
                    var addedSql = @"SELECT ASSET_CATEGORY_MAJOR as MAJOR,sum(ASSET_COST) as COST,sum(ACCT_DEPRECIATION) as ACCT, '增加' as PeriodType  FROM (select PERIOD_CODE
                                 , ASSET_CATEGORY_MAJOR
                                 , ASSET_COST
		                         , ACCT_DEPRECIATION
                                 , ASSET_CATEGORY_MINOR
								 , ASSET_CREATION_DATE
								 , FA_LOC_1
								 , FA_LOC_2
                                 , row_number() over (partition by ASSET_ID order by CREATE_DATE desc) as id
                            from dbo.AssetsLedger_Swap) cte
                                            where cte.id = 1 and ASSET_CREATION_DATE >= '" + minYearMonth +
                                   @"' and ASSET_CREATION_DATE < '" + maxYearMonth + @"'" + sqlWhere + @"
                                            group by ASSET_CATEGORY_MAJOR";
                    var addedPeriodList = db.SqlQueryable<FixedAssetsNetValueModel>(addedSql).ToList();
                    //本期减少
                    var reduceSql =
                        @"SELECT ASSET_CATEGORY_MAJOR as MAJOR,sum(cast(ASSET_COST as decimal(20,5))) as COST,sum(cast(RETIRE_ACCT_DEPRECIATION as decimal(20,5))) as ACCT, '减少' as PeriodType  FROM (select PERIOD
                                , ASSET_CATEGORY_MAJOR
                                , ASSET_COST
		                        , RETIRE_ACCT_DEPRECIATION
                                , ASSET_CATEGORY_MINOR
		                        , RETIRE_DATE
								 , BELONGTO_COMPANY as FA_LOC_1
								 , MANAGEMENT_COMPANY as FA_LOC_2
                                , row_number() over (partition by ASSET_ID order by CREATE_DATE desc) as id
                        from (select swap.*,info.MANAGEMENT_COMPANY,info.BELONGTO_COMPANY from dbo.AssetsRetirement_Swap swap left join dbo.Business_AssetMaintenanceInfo info on swap.ASSET_ID = info.ASSET_ID) a ) cte
                                        where cte.id = 1 and RETIRE_DATE >= '" + minYearMonth +
                        @"' and RETIRE_DATE < '" + maxYearMonth + @"'" + sqlWhere + @"
                                        group by ASSET_CATEGORY_MAJOR";
                    var reducePeriodList = db.SqlQueryable<FixedAssetsNetValueModel>(reduceSql).ToList();
                    var allPeriod = startPeriodList.Union(addedPeriodList).Union(reducePeriodList).ToList();
                    foreach (var item in list.Where(x => x.CalculationType != "合计" && x.CategoryType != "减值" && x.CategoryType != "账面"))
                    {
                        if (allPeriod.Any(x => x.MAJOR == item.CalculationType && x.PeriodType == "期初"))
                        {
                            if (item.CategoryType == "原价")
                            {
                                item.StartPeriod = allPeriod.First(x => x.MAJOR == item.CalculationType && x.PeriodType == "期初").COST;
                            }
                            else if (item.CategoryType == "折旧")
                            {
                                item.StartPeriod = allPeriod.First(x => x.MAJOR == item.CalculationType && x.PeriodType == "期初").ACCT;
                            }
                        }
                        if (allPeriod.Any(x => x.MAJOR == item.CalculationType && x.PeriodType == "增加"))
                        {
                            if (item.CategoryType == "原价")
                            {
                                item.AddedPeriod = allPeriod.First(x => x.MAJOR == item.CalculationType && x.PeriodType == "增加").COST;
                            }
                            else if (item.CategoryType == "折旧")
                            {
                                item.AddedPeriod = allPeriod.First(x => x.MAJOR == item.CalculationType && x.PeriodType == "增加").ACCT;
                            }
                        }
                        if (allPeriod.Any(x =>x.MAJOR == item.CalculationType && x.PeriodType == "减少"))
                        {

                            if (item.CategoryType == "原价")
                            {
                                item.ReducePeriod = allPeriod.First(x => x.MAJOR == item.CalculationType && x.PeriodType == "减少").COST;
                            }
                            else if (item.CategoryType == "折旧")
                            {
                                item.ReducePeriod = allPeriod.First(x => x.MAJOR == item.CalculationType && x.PeriodType == "减少").ACCT;
                            }
                        }
                    }
                    //合计
                    foreach (var item in list.Where(x => x.CalculationType == "合计" && x.CategoryType != "减值"))
                    {
                        if (item.CategoryType == "原价")
                        {
                            item.StartPeriod = allPeriod.Where(x => x.MAJOR != "总价" && x.PeriodType == "期初").Sum(x => x.COST);
                        }
                        else if (item.CategoryType == "折旧")
                        {
                            item.StartPeriod = allPeriod.Where(x => x.MAJOR != "总价" && x.PeriodType == "期初").Sum(x => x.ACCT);
                        }
                        if (item.CategoryType == "原价")
                        {
                            item.AddedPeriod = allPeriod.Where(x => x.MAJOR != "总价" && x.PeriodType == "增加").Sum(x => x.COST);
                        }
                        else if (item.CategoryType == "折旧")
                        {
                            item.AddedPeriod = allPeriod.Where(x => x.MAJOR != "总价" && x.PeriodType == "增加").Sum(x => x.ACCT);
                        }
                        if (item.CategoryType == "原价")
                        {
                            item.ReducePeriod = allPeriod.Where(x => x.MAJOR != "总价" && x.PeriodType == "减少").Sum(x => x.COST);
                        }
                        else if (item.CategoryType == "折旧")
                        {
                            item.ReducePeriod = allPeriod.Where(x => x.MAJOR != "总价" && x.PeriodType == "减少").Sum(x => x.ACCT);
                        }
                    }
                    foreach (var item in list.Where(x => x.CalculationType != "合计" && x.CategoryType == "账面"))
                    {
                        item.StartPeriod = list.First(x => x.CalculationType == item.CalculationType && x.CategoryType == "原价").StartPeriod - list.First(x => x.CalculationType == item.CalculationType && x.CategoryType == "折旧").StartPeriod;
                        item.AddedPeriod = list.First(x => x.CalculationType == item.CalculationType && x.CategoryType == "原价").AddedPeriod - list.First(x => x.CalculationType == item.CalculationType && x.CategoryType == "折旧").AddedPeriod;
                        item.ReducePeriod = list.First(x => x.CalculationType == item.CalculationType && x.CategoryType == "原价").ReducePeriod - list.First(x => x.CalculationType == item.CalculationType && x.CategoryType == "折旧").ReducePeriod;
                    }
                    var itemSum = list.Where(x => x.CalculationType == "合计" && x.CategoryType == "账面").First();
                    itemSum.StartPeriod = list.Where(x => x.CalculationType != "总价" && x.CategoryType == "账面").Sum(x => x.StartPeriod);
                    itemSum.AddedPeriod = list.Where(x => x.CalculationType != "总价" && x.CategoryType == "账面").Sum(x => x.AddedPeriod);
                    itemSum.ReducePeriod = list.Where(x => x.CalculationType != "总价" && x.CategoryType == "账面").Sum(x => x.ReducePeriod);
                    jsonResult.Rows = list.OrderBy(x => x.Zorder).ToList();
                    jsonResult.TotalRows = list.Count();
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}