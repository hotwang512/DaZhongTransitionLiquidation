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

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.FilialeStatistics
{
    public class FilialeStatisticsController : BaseController
    {
        // GET: AnalysisManagementCenter/FilialeStatistics
        public FilialeStatisticsController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetFilialeStatisticsDetail(string DateOfYear, GridParams para)
        {
           
            var jsonResult = new JsonResultModel<Business_FilialeStatistics>();
            DbBusinessDataService.Command(db =>
            {
                var currentYear = DateOfYear.TryToInt();
                var lastYear = (currentYear - 1).ToString();
                if (db.Queryable<Business_FilialeStatistics>().Any(x => x.DateOfYear == DateOfYear))
                {
                    int pageCount = 0;
                    para.pagenum = para.pagenum + 1;
                    jsonResult.Rows = db.Queryable<Business_FilialeStatistics>().Where(x => x.DateOfYear == DateOfYear).OrderBy(x => x.Zorder).ToList();
                    jsonResult.TotalRows = pageCount;
                }
                else
                {
                    if (DateOfYear != "")
                    {
                        var list = new List<Business_FilialeStatistics>();
                        for (int i = 1; i <= 12; i++)
                        {
                            var FilialeStatistics = new Business_FilialeStatistics();
                            FilialeStatistics.VGUID = Guid.NewGuid();
                            FilialeStatistics.DateOfYear = DateOfYear;
                            FilialeStatistics.YearMonth = i + "月";
                            if (i == 1 && db.Queryable<Business_FilialeStatistics>().Any(x => x.DateOfYear == lastYear))
                            {
                                FilialeStatistics.BeginningPeriod = db.Queryable<Business_FilialeStatistics>().Where(x => x.DateOfYear == lastYear && x.YearMonth == "12月").First().EndPeriod;
                            }
                            FilialeStatistics.CreateDate = DateTime.Now;
                            FilialeStatistics.CreateUser = UserInfo.UserName;
                            FilialeStatistics.Zorder = i;
                            list.Add(FilialeStatistics);
                        }
                        jsonResult.Rows = list.OrderBy(x => x.Zorder).ToList();
                        jsonResult.TotalRows = list.Count();
                    }
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveFilialeStatistics(List<Business_FilialeStatistics> FilialeStatisticsList)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var DateOfYear = FilialeStatisticsList.First().DateOfYear;
                if (db.Queryable<Business_FilialeStatistics>().Any(x => x.DateOfYear == DateOfYear))
                {
                    db.Updateable<Business_FilialeStatistics>(FilialeStatisticsList).ExecuteCommand();
                }
                else
                {
                    db.Insertable<Business_FilialeStatistics>(FilialeStatisticsList).ExecuteCommand();
                }
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}