using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.VehicleAmountReport
{
    public class VehicleAmountController : BaseController
    {
        // GET: AnalysisManagementCenter/VehicleAmount
        public VehicleAmountController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetVehicleAmountCompanyList()
        {
            var jsonResult = new JsonResultModel<CS_Master_2>();

            var _db = DbConfig.GetInstance();
            var masterData = _db.Queryable<CS_Master_2>()
                .Where(x => x.VGUID == "B0A3E68C-5A7F-4C86-8A68-4085971D3276".TryToGuid()).OrderBy(x => x.Zorder).ToList();
            jsonResult.Rows = masterData;
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetVehicleAmountValueList(string DateOfYear)
        {
            var jsonResult = new JsonResultModel<Business_VehicleAmountReport>();
            DbBusinessDataService.Command(db =>
            {
                var valueData = db.Queryable<Business_VehicleAmountReport>()
                    .Where(x => x.DateOfYear == DateOfYear).ToList();
                jsonResult.Rows = valueData;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public string GetVehicleAmountList(string VacationType, string paras)
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
        public JsonResult SaveVehicleAmountList(List<Business_VehicleAmountReport> tbValues)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var DateOfYear = tbValues.First().DateOfYear;
                if (db.Queryable<Business_VehicleAmountReport>().Any(x => x.DateOfYear == DateOfYear))
                {
                    db.Updateable<Business_VehicleAmountReport>(tbValues).ExecuteCommand();
                }
                else
                {
                    tbValues.ForEach(x => x.VGUID = Guid.NewGuid());
                    db.Insertable<Business_VehicleAmountReport>(tbValues).ExecuteCommand();
                }
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}