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

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.DriversIncome
{
    public class DriversIncomeController : BaseController
    {
        // GET: AnalysisManagementCenter/DriversIncome
        public DriversIncomeController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetDriversIncomeYear(string VehicleModel, GridParams para)
        {
            var jsonResult = new List<string>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult = db.Queryable<Business_DriversIncome>().Where(x => x.VehicleModel == VehicleModel)
                    .GroupBy(x => x.DateOfYear).Select(x => x.DateOfYear).ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public string GetDriversIncomeList(string VehicleModel, string paras, string SingleOrDouble)
        {
            var json = "";
            if (!paras.IsNullOrEmpty())
            {
                DbBusinessDataService.Command(db =>
                {
                    var data = db.Ado.GetDataTable(
                        @"select *
                        from
                        (
                            select ProjectName
                                 , DateOfYear
		                         , " + SingleOrDouble + @"
		                         , Zorder
                            from Business_DriversIncome  where VehicleModel = '" + VehicleModel + @"'
                        ) a
                        pivot
                        (
                            max(" + SingleOrDouble + @")
                            for DateOfYear in (" + paras + @")
                        ) b order by Zorder");
                    json = data.DataTableToJson();
                });
            }
            return json;
        }
        public JsonResult GetDriversIncomeDetail(string VehicleModel, string VehicleModelName, string DateOfYear)
        {
            var jsonResult = new JsonResultModel<Business_DriversIncome>();
            DbBusinessDataService.Command(db =>
            {
                if (db.Queryable<Business_DriversIncome>().Any(x => x.VehicleModel == VehicleModel && x.DateOfYear == DateOfYear))
                {
                    jsonResult.Rows = db.Queryable<Business_DriversIncome>().Where(x =>
                        x.VehicleModel == VehicleModel && x.DateOfYear == DateOfYear).OrderBy(x => x.Zorder).ToList();
                }
                else
                {
                    if (VehicleModel != "")
                    {
                        var _db = DbConfig.GetInstance();
                        var masterData = _db.Queryable<CS_Master_2>()
                            .Where(x => x.VGUID == "DD89C384-9195-4683-BA3E-4354593DF6E6".TryToGuid()).OrderBy(x => x.MasterCode).ToList();
                        var list = new List<Business_DriversIncome>();
                        foreach (var item in masterData)
                        {
                            var DriversIncome = new Business_DriversIncome();
                            DriversIncome.VGUID = Guid.NewGuid();
                            DriversIncome.VehicleModel = VehicleModel;
                            DriversIncome.VehicleModelName = VehicleModelName;
                            DriversIncome.DateOfYear = DateOfYear;
                            DriversIncome.ProjectName = item.DESC0;
                            DriversIncome.Zorder = item.MasterCode.TryToInt();
                            DriversIncome.CreateDate = DateTime.Now;
                            DriversIncome.CreateUser = UserInfo.UserName;
                            list.Add(DriversIncome);
                        }
                        jsonResult.Rows = list.OrderBy(x => x.Zorder).ToList();
                    }
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveDriversIncome(List<Business_DriversIncome> DriversIncomeList)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var VehicleModel = DriversIncomeList.First().VehicleModel;
                var DateOfYear = DriversIncomeList.First().DateOfYear;

                if (db.Queryable<Business_DriversIncome>().Any(x => x.VehicleModel == VehicleModel && x.DateOfYear == DateOfYear))
                {
                    db.Updateable<Business_DriversIncome>(DriversIncomeList).ExecuteCommand();
                }
                else
                {
                    db.Insertable<Business_DriversIncome>(DriversIncomeList).ExecuteCommand();
                }
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}