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
        public JsonResult GetFixedAssetsNetValueDetail(string DateOfYear, string minMonth, string maxMonth, GridParams para)
        {
            var jsonResult = new JsonResultModel<Models.FixedAssetsNetValue>();
            DbBusinessDataService.Command(db =>
            {
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
                        @"SELECT ASSET_CATEGORY_MAJOR as MAJOR,sum(ASSET_COST) as COST,sum(ACCT_DEPRECIATION) as ACCT, '期初' as PeriodType  FROM dbo.Business_AssetMaintenanceInfo
                    where LISENSING_DATE < '" + DateOfYear + @"-01-01'
                    group by ASSET_CATEGORY_MAJOR").ToList();
                    //本期增加
                    var addedPeriodList = db.SqlQueryable<FixedAssetsNetValueModel>(
                        @"SELECT ASSET_CATEGORY_MAJOR as MAJOR,sum(ASSET_COST) as COST,sum(ACCT_DEPRECIATION) as ACCT, '增加' as PeriodType  FROM dbo.Business_AssetMaintenanceInfo
                    where LISENSING_DATE >= '" + minYearMonth + @"' and LISENSING_DATE < '" + maxYearMonth + @"'
                    group by ASSET_CATEGORY_MAJOR").ToList();
                    //本期减少
                    var reducePeriodList = db.SqlQueryable<FixedAssetsNetValueModel>(
                        @"SELECT ASSET_CATEGORY_MAJOR as MAJOR,sum(ASSET_COST) as COST,sum(ACCT_DEPRECIATION) as ACCT, '减少' as PeriodType  FROM dbo.Business_AssetMaintenanceInfo
                    where BACK_CAR_DATE >= '" + minYearMonth + @"' and BACK_CAR_DATE < '" + maxYearMonth + @"'
                    group by ASSET_CATEGORY_MAJOR").ToList();
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