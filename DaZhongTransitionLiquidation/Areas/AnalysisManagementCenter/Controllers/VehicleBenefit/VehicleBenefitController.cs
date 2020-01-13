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

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.VehicleBenefit
{
    public class VehicleBenefitController : BaseController
    {
        // GET: AnalysisManagementCenter/VehicleBenefit
        public VehicleBenefitController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetVehicleBenefitYear(string VacationType, GridParams para)
        {
            var jsonResult = new List<string>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult = db.Queryable<Business_VehicleBenefit>()
                    .GroupBy(x => x.DateOfYear).Select(x => x.DateOfYear).ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public string GetVehicleBenefitList(string paras)
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
		                         , ProjectValue
		                         , Zorder
                            from Business_VehicleBenefit 
                        ) a
                        pivot
                        (
                            max(ProjectValue)
                            for DateOfYear in (" + paras + @")
                        ) b order by Zorder");
                    json = data.DataTableToJson();
                });
            }
            return json;
        }
        public JsonResult GetVehicleBenefitDetail(string DateOfYear)
        {
            var jsonResult = new JsonResultModel<Business_VehicleBenefit>();
            DbBusinessDataService.Command(db =>
            {
                if (db.Queryable<Business_VehicleBenefit>().Any(x => x.DateOfYear == DateOfYear))
                {
                    jsonResult.Rows = db.Queryable<Business_VehicleBenefit>().Where(x =>
                        x.DateOfYear == DateOfYear).OrderBy(x => x.Zorder).ToList();
                }
                else
                {
                    var _db = DbConfig.GetInstance();
                    var masterData = _db.Queryable<CS_Master_2>().Where(x => x.VGUID == "672FD1D4-9708-45C9-9B0A-C7DB4B1080FE".TryToGuid()).OrderBy(x => x.MasterCode).ToList();
                    var list = new List<Business_VehicleBenefit>();
                    foreach (var item in masterData)
                    {
                        var VehicleBenefit = new Business_VehicleBenefit();
                        VehicleBenefit.VGUID = Guid.NewGuid();
                        VehicleBenefit.ProjectVGUID = item.LGUID;
                        VehicleBenefit.DateOfYear = DateOfYear;
                        VehicleBenefit.ProjectName = item.DESC0;
                        VehicleBenefit.Zorder = item.MasterCode.TryToInt();
                        VehicleBenefit.CreateDate = DateTime.Now;
                        VehicleBenefit.CreateUser = UserInfo.UserName;
                        list.Add(VehicleBenefit);
                    }
                    jsonResult.Rows = list.OrderBy(x => x.Zorder).ToList();
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveVehicleBenefit(List<Business_VehicleBenefit> VehicleBenefitList)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var DateOfYear = VehicleBenefitList.First().DateOfYear;
                if (db.Queryable<Business_VehicleBenefit>().Any(x => x.DateOfYear == DateOfYear))
                {
                    db.Updateable<Business_VehicleBenefit>(VehicleBenefitList).ExecuteCommand();
                }
                else
                {
                    db.Insertable<Business_VehicleBenefit>(VehicleBenefitList).ExecuteCommand();
                }
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}