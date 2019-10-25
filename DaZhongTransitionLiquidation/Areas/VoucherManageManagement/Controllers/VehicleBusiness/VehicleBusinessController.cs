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
using SqlSugar;
using System.Text;

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
        public JsonResult GeVehicleData(Business_VehicleUnitList searchParams, GridParams para)
        {
            var response = new List<Business_VehicleUnitList>();
            DbBusinessDataService.Command(db =>
            {
                //int pageCount = 0;
                //para.pagenum = para.pagenum + 1;
                var yearMonth = "";
                if (searchParams.YearMonth != null)
                {
                    yearMonth = searchParams.YearMonth.Replace("-", "");
                }
                //response = db.SqlQueryable<Business_VehicleUnitList>(@"select a.VGUID,a.ORIGINALID,a.YearMonth,a.PLATE_NUMBER,a.MODEL_MINOR,CAST(a.MODEL_DAYS AS decimal(18,0)) as MODEL_DAYS
                //            , b.MANAGEMENT_COMPANY, b.BELONGTO_COMPANY,b.MODEL_MAJOR, b.DESCRIPTION as CarType from Business_VehicleList as a left join Business_AssetMaintenanceInfo
                //            as b on a.PLATE_NUMBER = b.PLATE_NUMBER where b.OPERATING_STATE='在运' and b.GROUP_ID='出租车' ")
                response = db.SqlQueryable<Business_VehicleUnitList>(@"select a.VGUID,a.ORIGINALID,a.YearMonth,a.PLATE_NUMBER,m.BusinessName1 as MODEL_MAJOR, 
                            m.BusinessName2 as MODEL_MINOR,CAST(a.MODEL_DAYS AS decimal(18,2)) as MODEL_DAYS
                            ,b.MANAGEMENT_COMPANY,b.BELONGTO_COMPANY,b.DESCRIPTION as CarType from Business_VehicleList as a 
                            left join Business_AssetMaintenanceInfo as b on a.PLATE_NUMBER = b.PLATE_NUMBER 
                            left join (select a.BusinessName as BusinessName1,b.BusinessName as BusinessName2,c.BusinessName as BusinessName3 from Business_ManageModel as a
							            left join Business_ManageModel as b on a.VGUID = b.ParentVGUID
							            left join Business_ManageModel as c on b.VGUID = c.ParentVGUID
							            where c.BusinessName is not null
							            group by a.BusinessName,b.BusinessName,c.BusinessName) as m on a.MODEL_MINOR = m.BusinessName3
                            where  b.GROUP_ID='出租车' ")
                .WhereIF(searchParams.YearMonth != null, i => i.YearMonth == yearMonth)
                .WhereIF(searchParams.PLATE_NUMBER != null, i => i.PLATE_NUMBER.Contains(searchParams.PLATE_NUMBER))
                .WhereIF(searchParams.MODEL_MINOR != null, i => i.MODEL_MINOR.Contains(searchParams.MODEL_MINOR))
                //.WhereIF(searchParams.MODEL_DAYS != null, i => i.MODEL_DAYS == searchParams.MODEL_DAYS)
                .OrderBy("MODEL_MAJOR asc,MODEL_MINOR asc,CarType asc").ToList();
                //jsonResult.TotalRows = pageCount;
            });
            return Json(
                 response,
                 "application/json",
                 Encoding.UTF8,
                 JsonRequestBehavior.AllowGet
             );
        }
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
        public JsonResult GetVehicleBusinessInfo()//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = true, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var userName = UserInfo.LoginName;
                SyncVehicleBusiness(db, resultModel, userName);
            });
            return Json(resultModel);
        }
        public static void SyncVehicleBusiness(SqlSugarClient db, ResultModel<string> resultModel,string userName)
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
                    if (vehicleData != null)
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
                            vehicle.Founder = userName;
                            vehicle.CreatTime = DateTime.Now;
                            vehicleList.Add(vehicle);
                        }
                        if (vehicleList != null)
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
        }
    }
}