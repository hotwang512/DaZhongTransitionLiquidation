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

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.VacationSalary
{
    public class VacationSalaryController : BaseController
    {
        // GET: AnalysisManagementCenter/VacationSalary
        public VacationSalaryController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetVacationSalaryYear(string VacationType, GridParams para)
        {
            var jsonResult = new List<string>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult = db.Queryable<Business_VacationSalary>().Where(x => x.VacationType == VacationType)
                    .GroupBy(x => x.DateOfYear).Select(x => x.DateOfYear).ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public string GetVacationSalaryList(string VacationType, string paras)
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
		                         , Salary
		                         , Zorder
                            from Business_VacationSalary  where VacationType = '" + VacationType + @"'
                        ) a
                        pivot
                        (
                            max(Salary)
                            for DateOfYear in (" + paras + @")
                        ) b order by Zorder");
                    json = data.DataTableToJson();
                });
            }
            return json;
        }
        public JsonResult GetVacationSalaryDetail(Guid VacationTypeID, string VacationType, string DateOfYear)
        {
            var jsonResult = new JsonResultModel<Business_VacationSalary>();
            DbBusinessDataService.Command(db =>
            {
                if (db.Queryable<Business_VacationSalary>().Any(x => x.VacationType == VacationType && x.DateOfYear == DateOfYear))
                {
                    jsonResult.Rows = db.Queryable<Business_VacationSalary>().Where(x =>
                        x.VacationType == VacationType && x.DateOfYear == DateOfYear).OrderBy(x => x.Zorder).ToList();
                }
                else
                {
                    if (VacationType != "")
                    {
                        var _db = DbConfig.GetInstance();
                        var masterData = _db.Queryable<CS_Master_2>()
                            .Where(x => x.VGUID == VacationTypeID).OrderBy(x => x.MasterCode).ToList();
                        var list = new List<Business_VacationSalary>();
                        foreach (var item in masterData)
                        {
                            var VacationSalary = new Business_VacationSalary();
                            VacationSalary.VGUID = Guid.NewGuid();
                            VacationSalary.DateOfYear = DateOfYear;
                            VacationSalary.ProjectName = item.DESC0;
                            VacationSalary.VacationType = VacationType;
                            VacationSalary.Zorder = item.MasterCode.TryToInt();
                            VacationSalary.CreateDate = DateTime.Now;
                            VacationSalary.CreateUser = UserInfo.UserName;
                            list.Add(VacationSalary);
                        }
                        jsonResult.Rows = list.OrderBy(x => x.Zorder).ToList();
                    }
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveVacationSalary(List<Business_VacationSalary> VacationSalaryList)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var VehicleModel = VacationSalaryList.First().VacationType;
                var DateOfYear = VacationSalaryList.First().DateOfYear;

                if (db.Queryable<Business_VacationSalary>().Any(x => x.VacationType == VehicleModel && x.DateOfYear == DateOfYear))
                {
                    db.Updateable<Business_VacationSalary>(VacationSalaryList).ExecuteCommand();
                }
                else
                {
                    db.Insertable<Business_VacationSalary>(VacationSalaryList).ExecuteCommand();
                }
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}