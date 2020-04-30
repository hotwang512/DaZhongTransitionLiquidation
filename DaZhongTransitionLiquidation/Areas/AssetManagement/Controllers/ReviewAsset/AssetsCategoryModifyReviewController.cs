using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.ReviewAsset
{
    public class AssetsCategoryModifyReviewController : BaseController
    {
        // GET: AssetManagement/AssetsCategory
        public AssetsCategoryModifyReviewController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("eaf2caac-98f9-459c-939b-faf6468c5c64");
            return View();
        }
        public JsonResult GetReviewAssetListDatas(Business_ModifyVehicle searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_ModifyVehicleModel>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.SqlQueryable<Business_ModifyVehicleModel>
                    (@"SELECT mv.*,mi.DESCRIPTION,
					   mi.ASSET_CATEGORY_MAJOR as ASSET_CATEGORY_MAJOR_M,
					   mi.ASSET_CATEGORY_MINOR as ASSET_CATEGORY_MINOR_M,
                       mi.PLATE_NUMBER AS PLATE_NUMBER_M,
                       mi.MODEL_MAJOR AS MODEL_MAJOR_M,
                       mi.MODEL_MINOR AS MODEL_MINOR_M,
                       mi.MANAGEMENT_COMPANY AS MANAGEMENT_COMPANY_M,
                       mi.BELONGTO_COMPANY AS BELONGTO_COMPANY_M
                    FROM Business_ModifyVehicle mv
                    LEFT JOIN Business_AssetMaintenanceInfo mi
                        ON mv.ORIGINALID = mi.ORIGINALID")
                    .Where(i => !i.ISVERIFY)
                    .Where(i => i.MODIFY_TYPE == "AssetsCategory")
                    .WhereIF(!searchParams.ENGINE_NUMBER.IsNullOrEmpty(), i => i.ENGINE_NUMBER.Contains(searchParams.ENGINE_NUMBER))
                    .WhereIF(!searchParams.CHASSIS_NUMBER.IsNullOrEmpty(), i => i.CHASSIS_NUMBER.Contains(searchParams.CHASSIS_NUMBER))
                    .WhereIF(!searchParams.VEHICLE_SHORTNAME.IsNullOrEmpty(), i => i.VEHICLE_SHORTNAME.Contains(searchParams.VEHICLE_SHORTNAME))
                    .WhereIF(!searchParams.PLATE_NUMBER.IsNullOrEmpty(), i => i.PLATE_NUMBER.Contains(searchParams.PLATE_NUMBER))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToList();
                jsonResult.TotalRows = pageCount;
            });
            return Json(
                jsonResult,
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
        public JsonResult SubmitModifyVehicleReview(List<Guid> vguids,string YearMonth)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var modifyVehicleList = db.SqlQueryable<Business_ModifyVehicleModel>(@"SELECT mv.*,
                       mi.PLATE_NUMBER AS PLATE_NUMBER_M,
                       mi.MODEL_MAJOR AS MODEL_MAJOR_M,
                       mi.MODEL_MINOR AS MODEL_MINOR_M,
                       mi.MANAGEMENT_COMPANY AS MANAGEMENT_COMPANY_M,
                       mi.BELONGTO_COMPANY AS BELONGTO_COMPANY_M,
                       mi.ORGANIZATION_NUM AS FA_LOC_3,
                       mi.START_VEHICLE_DATE AS PERIOD,
                       mi.DESCRIPTION AS DESCRIPTION,
					   mi.BOOK_TYPE_CODE,
                       mi.ASSET_ID AS ASSET_ID,
                       mv.BELONGTO_COMPANY AS FA_LOC_1,
                       mv.MANAGEMENT_COMPANY AS FA_LOC_2
                    FROM Business_ModifyVehicle mv
                    LEFT JOIN Business_AssetMaintenanceInfo mi
                        ON mv.ORIGINALID = mi.ORIGINALID").Where(x => vguids.Contains(x.VGUID) && x.MODIFY_TYPE == "AssetsCategory").ToList();
                    var assetSwapList = new List<AssetMaintenanceInfo_Swap>();
                    foreach (var item in modifyVehicleList)
                    {
                        //先更新资产维护表，再写入Oracle 中间表
                        db.Updateable<Business_AssetMaintenanceInfo>()
                            .UpdateColumns(x => new Business_AssetMaintenanceInfo { ASSET_CATEGORY_MAJOR = item.ASSET_CATEGORY_MAJOR, ASSET_CATEGORY_MINOR = item.ASSET_CATEGORY_MINOR }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                        db.Updateable<Business_ModifyVehicle>()
                            .UpdateColumns(x => new Business_ModifyVehicle { ISVERIFY = true }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                        var assetSwapModel = new AssetMaintenanceInfo_Swap();
                        assetSwapModel.ASSET_CATEGORY_MAJOR = item.ASSET_CATEGORY_MAJOR;
                        assetSwapModel.ASSET_CATEGORY_MINOR = item.ASSET_CATEGORY_MINOR;
                        assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR;
                        assetSwapModel.MODEL_MINOR = item.MODEL_MINOR;
                        assetSwapModel.TRANSACTION_ID = Guid.NewGuid();
                        //传入订单选择的部门
                        assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                        assetSwapModel.CREATE_DATE = DateTime.Now;
                        assetSwapModel.ASSET_ID = item.ASSET_ID;
                        assetSwapModel.STATUS = "N";
                        var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                            x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Abbreviation == item.BELONGTO_COMPANY).First();
                        assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                        assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                        assetSwapModel.PERIOD = YearMonth;
                        assetSwapModel.BOOK_TYPE_CODE = item.BOOK_TYPE_CODE;
                        assetSwapModel.CHECK_STATE = false;
                        assetSwapModel.PROCESS_TYPE = "AssetsCategory";
                        assetSwapList.Add(assetSwapModel);
                    }
                    if (assetSwapList.Count > 0)
                    {
                        db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetModifyVehicleReview()
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var list = new List<Business_ModifyVehicle>();
                    var assetMaintenanceInfoList = db.Queryable<Business_AssetMaintenanceInfo>().Where(x => x.GROUP_ID == "出租车").ToList();
                    var manageModelList = db.Queryable<Business_ManageModel>().ToList();
                    foreach (var item in assetMaintenanceInfoList)
                    {
                        var minorModel = manageModelList.First(x => x.LevelNum == 1 && x.BusinessName == item.MODEL_MINOR);
                        var modelCategory = db.Queryable<Business_ManageModel_AssetsCategory>()
                            .Where(x => x.GoodsModel == item.VEHICLE_SHORTNAME && x.ManageModelVGUID == minorModel.VGUID).First();
                        if (!modelCategory.IsNullOrEmpty() && (modelCategory.CategoryMajor != item.ASSET_CATEGORY_MAJOR ||
                            modelCategory.CategoryMinor != item.ASSET_CATEGORY_MINOR))
                        {
                            var model = new Business_ModifyVehicle();
                            model.VGUID = Guid.NewGuid();
                            model.ORIGINALID = item.ORIGINALID;
                            model.PLATE_NUMBER = item.PLATE_NUMBER;
                            model.TAG_NUMBER = item.TAG_NUMBER;
                            model.VEHICLE_SHORTNAME = item.VEHICLE_SHORTNAME;
                            model.MANAGEMENT_COMPANY = item.MANAGEMENT_COMPANY;
                            model.BELONGTO_COMPANY = item.BELONGTO_COMPANY;
                            model.VEHICLE_STATE = item.VEHICLE_STATE;
                            model.OPERATING_STATE = item.OPERATING_STATE;
                            model.ENGINE_NUMBER = item.ENGINE_NUMBER;
                            model.MODEL_MAJOR = item.MODEL_MAJOR;
                            model.MODEL_MINOR = item.MODEL_MINOR;
                            model.ASSET_CATEGORY_MAJOR = modelCategory.CategoryMajor;
                            model.ASSET_CATEGORY_MINOR = modelCategory.CategoryMinor;
                            model.MODIFY_TYPE = "AssetsCategory";
                            model.OLDDATA = item.ASSET_CATEGORY_MAJOR + "|" + item.ASSET_CATEGORY_MINOR;
                            model.ISVERIFY = false;
                            model.CREATE_DATE = DateTime.Now;
                            model.CREATE_USER = "System";
                            list.Add(model);
                        }
                    }
                    if (list.Count > 0)
                    {
                        db.Insertable<Business_ModifyVehicle>(list).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}