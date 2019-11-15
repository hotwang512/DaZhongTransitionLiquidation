using DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.RentTarget
{
    public class RentTargetController : BaseController
    {
        // GET: AnalysisManagementCenter/RentTarget
        public RentTargetController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AnalysisManagementCenter/RentTarget
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetRentTargetYear(string VehicleModel, GridParams para)
        {
            var jsonResult = new List<string>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult = db.Queryable<Business_RentTarget>().Where(x => x.VehicleModel == VehicleModel)
                    .GroupBy(x => x.DateOfYear).Select(x => x.DateOfYear).ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public string GetRentTargetList(string VehicleModel, string paras, string SingleOrDouble)
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
                            from Business_RentTarget  where VehicleModel = '" + VehicleModel + @"'
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
        public JsonResult GetRentTargetDetail(string VehicleModel, string VehicleModelName, string DateOfYear)
        {
            var jsonResult = new JsonResultModel<Business_RentTarget>();
            DbBusinessDataService.Command(db =>
            {
                if (db.Queryable<Business_RentTarget>().Any(x => x.VehicleModel == VehicleModel && x.DateOfYear == DateOfYear))
                {
                    jsonResult.Rows = db.Queryable<Business_RentTarget>().Where(x =>
                        x.VehicleModel == VehicleModel && x.DateOfYear == DateOfYear).OrderBy(x => x.Zorder).ToList();
                }
                else
                {
                    if (VehicleModel != "")
                    {
                        var _db = DbConfig.GetInstance();
                        var masterData = _db.Queryable<CS_Master_2>()
                            .Where(x => x.VGUID == "deb98d3c-6826-4c78-8eeb-4c7bd62fb2a6".TryToGuid()).OrderBy(x => x.MasterCode).ToList();
                        var list = new List<Business_RentTarget>();
                        foreach (var item in masterData)
                        {
                            var RentTarget = new Business_RentTarget();
                            RentTarget.VGUID = Guid.NewGuid();
                            RentTarget.VehicleModel = VehicleModel;
                            RentTarget.VehicleModelName = VehicleModelName;
                            RentTarget.DateOfYear = DateOfYear;
                            RentTarget.ProjectName = item.DESC0;
                            RentTarget.Zorder = item.MasterCode;
                            RentTarget.CreateDate = DateTime.Now;
                            RentTarget.CreateUser = UserInfo.UserName;
                            list.Add(RentTarget);
                        }
                        jsonResult.Rows = list;
                    }
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveRentTarget(List<Business_RentTarget> RentTargetList)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var VehicleModel = RentTargetList.First().VehicleModel;
                var DateOfYear = RentTargetList.First().DateOfYear;

                if (db.Queryable<Business_RentTarget>().Any(x => x.VehicleModel == VehicleModel && x.DateOfYear == DateOfYear))
                {
                    db.Updateable<Business_RentTarget>(RentTargetList).ExecuteCommand();
                }
                else
                {
                    db.Insertable<Business_RentTarget>(RentTargetList).ExecuteCommand();
                }
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}