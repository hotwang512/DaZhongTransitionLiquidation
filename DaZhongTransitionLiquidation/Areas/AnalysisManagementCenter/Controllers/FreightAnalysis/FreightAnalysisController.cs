using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Controllers.FreightAnalysis
{
    public class FreightAnalysisController : BaseController
    {
        // GET: AnalysisManagementCenter/FreightAnalysis
        public FreightAnalysisController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetFreightAnalysisYear(string VehicleModel, GridParams para)
        {
            var jsonResult = new List<string>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult = db.Queryable<Business_FreightAnalysis>().Where(x => x.VehicleModel == VehicleModel)
                    .GroupBy(x => x.DateOfYear).Select(x => x.DateOfYear).ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public string GetFreightAnalysisList(string VehicleModel, string paras, string DayOrNight)
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
		                         , "+ DayOrNight + @"
		                         , Zorder
                            from Business_FreightAnalysis  where VehicleModel = '" + VehicleModel + @"'
                        ) a
                        pivot
                        (
                            max(" + DayOrNight + @")
                            for DateOfYear in (" + paras + @")
                        ) b order by Zorder");
                    json = data.DataTableToJson();
                });
            }
            return json;
        }
        public JsonResult GetFreightAnalysisDetail(string VehicleModel, string VehicleModelName, string DateOfYear)
        {
            var jsonResult = new JsonResultModel<Business_FreightAnalysis>();
            DbBusinessDataService.Command(db =>
            {
                if (db.Queryable<Business_FreightAnalysis>().Any(x => x.VehicleModel == VehicleModel && x.DateOfYear == DateOfYear))
                {
                    jsonResult.Rows = db.Queryable<Business_FreightAnalysis>().Where(x =>
                        x.VehicleModel == VehicleModel && x.DateOfYear == DateOfYear).OrderBy(x => x.Zorder).ToList();
                }
                else
                {
                    if (VehicleModel != "")
                    {
                        var _db = DbConfig.GetInstance();
                        var masterData = _db.Queryable<CS_Master_2>()
                            .Where(x => x.VGUID == "BD1219F5-0BAF-4C02-AC34-D7CA485A7754".TryToGuid()).OrderBy(x => x.MasterCode).ToList();
                        var list = new List<Business_FreightAnalysis>();
                        foreach (var item in masterData)
                        {
                            var freightAnalysis = new Business_FreightAnalysis();
                            freightAnalysis.VGUID = Guid.NewGuid();
                            freightAnalysis.VehicleModel = VehicleModel;
                            freightAnalysis.VehicleModelName = VehicleModelName;
                            freightAnalysis.DateOfYear = DateOfYear;
                            freightAnalysis.ProjectName = item.DESC0;
                            freightAnalysis.Zorder = item.MasterCode;
                            freightAnalysis.CreateDate = DateTime.Now;
                            freightAnalysis.CreateUser = UserInfo.UserName;
                            list.Add(freightAnalysis);
                        }
                        jsonResult.Rows = list;
                    }
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult SaveFreightAnalysis(List<Business_FreightAnalysis> FreightAnalysisList)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var VehicleModel = FreightAnalysisList.First().VehicleModel;
                var DateOfYear = FreightAnalysisList.First().DateOfYear;

                if (db.Queryable<Business_FreightAnalysis>().Any(x => x.VehicleModel == VehicleModel && x.DateOfYear == DateOfYear))
                {
                    db.Updateable<Business_FreightAnalysis>(FreightAnalysisList).ExecuteCommand();
                }
                else
                {
                    db.Insertable<Business_FreightAnalysis>(FreightAnalysisList).ExecuteCommand();
                }
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}