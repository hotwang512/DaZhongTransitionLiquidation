using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VehicleBusiness
{
    public class VehicleBusinessController : BaseController
    {
        public VehicleBusinessController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/VehicleBusiness
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GeVehicleData(Business_VehicleList searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_VehicleList>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                var yearMonth = "";
                if(searchParams.YearMonth != null)
                {
                    yearMonth = searchParams.YearMonth.Replace("-", "");
                }
                jsonResult.Rows = db.Queryable<Business_VehicleList>()
                .WhereIF(searchParams.YearMonth != null, i => i.YearMonth == yearMonth)
                .WhereIF(searchParams.PLATE_NUMBER != null, i => i.PLATE_NUMBER.Contains(searchParams.PLATE_NUMBER))
                .WhereIF(searchParams.MODEL_MINOR != null, i => i.MODEL_MINOR.Contains(searchParams.MODEL_MINOR))
                .WhereIF(searchParams.MODEL_DAYS != null, i => i.MODEL_DAYS == searchParams.MODEL_DAYS)
                .OrderBy(i => i.PLATE_NUMBER).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetVehicleBusinessInfo()//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = true, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                List<Business_VehicleList> vehicleList = new List<Business_VehicleList>();
                var url = ConfigSugar.GetAppString("GetVehicleUrl");
                var month = DateTime.Now.AddMonths(-1).Month.TryToString();
                month = month.Length > 1 ? month : "0" + month;
                var yearMonth = DateTime.Now.Year.TryToString() + month;
                var data = "{" +
                                        "\"YearMonth\":\"{YearMonth}\"".Replace("{YearMonth}", yearMonth) +
                                        "}";
                try
                {
                    WebClient wc = new WebClient();
                    wc.Headers.Clear();
                    wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                    wc.Encoding = System.Text.Encoding.UTF8;
                    var resultData = wc.UploadString(new Uri(url), data);
                    var modelData = resultData.JsonToModel<VehicleResult>();
                    if (modelData.success)
                    {
                        var vehicleData = modelData.data[0].DATA;
                        if(vehicleData != null)
                        {
                            foreach (var item in vehicleData)
                            {
                                Business_VehicleList vehicle = new Business_VehicleList();
                                vehicle.VGUID = Guid.NewGuid();
                                vehicle.YearMonth = yearMonth;
                                vehicle.ORIGINALID = item[0];
                                vehicle.PLATE_NUMBER = item[1];
                                vehicle.MODEL_DAYS = item[2];
                                vehicle.MODEL_MINOR = item[3];
                                vehicle.Founder = UserInfo.LoginName;
                                vehicle.CreatTime = DateTime.Now;
                                vehicleList.Add(vehicle);
                            }
                            if(vehicleList != null)
                            {
                                db.Deleteable<Business_VehicleList>().Where(x => x.YearMonth == yearMonth).ExecuteCommand();
                                db.Insertable(vehicleList).ExecuteCommand();
                            }
                        }
                    }
                    else
                    {
                        resultModel.IsSuccess = false;
                        resultModel.ResultInfo = modelData.message;
                    }
                }
                catch (Exception ex)
                {
                    resultModel.IsSuccess = false;
                    resultModel.ResultInfo = ex.Message;
                }
            });
            return Json(resultModel);
        }
    }
}