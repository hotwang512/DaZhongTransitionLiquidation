using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.VehicleExtrasFeeSetting
{
    public class VehicleExtrasFeeSettingController : BaseController
    {
        // GET: SystemManagement/VehicleExtrasFeeSetting
        public VehicleExtrasFeeSettingController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetVehicleExtrasFeeSettingListDatas_bak(Business_VehicleExtrasFeeSetting searchModel, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_VehicleExtrasFeeSetting>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_VehicleExtrasFeeSetting>()
                    .WhereIF(!searchModel.VehicleModel.IsNullOrEmpty(), i => i.VehicleModelCode == searchModel.VehicleModel)
                    .OrderBy(i => i.VehicleModelCode)
                    .OrderBy(i => i.BusinessSubItem, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public string GetVehicleExtrasFeeSettingListDatas(Business_VehicleExtrasFeeSetting searchModel, GridParams para)
        {
            var json = "";
            DbBusinessDataService.Command(db =>
            {
                var data = db.Ado.GetDataTable("SELECT * FROM (SELECT BusinessProject,VehicleModel,Fee FROM Business_VehicleExtrasFeeSetting) a pivot(max(Fee) for VehicleModel\r\n in (桑塔纳4000,荣威Ei5,[途安1.4T],[途安1.6],[途安1.6L]) )b");
                json = data.DataTableToJson();
            });
            return json;
        }

        public JsonResult GetVehicleExtrasFeeSettingColumns()
        {
            var resultModel = new ResultModel<List<string>>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var data = db.Queryable<Business_VehicleExtrasFeeSetting>().GroupBy(x => x.VehicleModel)
                    .Select(x => x.VehicleModel).ToList();
                resultModel.ResultInfo = data;
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteVehicleExtrasFeeSetting(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //删除主表信息
                saveChanges = db.Deleteable<Business_VehicleExtrasFeeSetting>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}