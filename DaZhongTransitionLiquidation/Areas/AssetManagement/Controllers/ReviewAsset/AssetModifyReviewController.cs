using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.ReviewAsset
{
    public class AssetModifyReviewController : BaseController
    {
        // GET: AssetManagement/AssetModifyReview
        public AssetModifyReviewController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/ReviewAsset
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
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
                    (@"SELECT mv.*,
                       mi.PLATE_NUMBER AS PLATE_NUMBER_M,
                       mi.MODEL_MAJOR AS MODEL_MAJOR_M,
                       mi.MODEL_MINOR AS MODEL_MINOR_M,
                       mi.MANAGEMENT_COMPANY AS MANAGEMENT_COMPANY_M,
                       mi.BELONGTO_COMPANY AS BELONGTO_COMPANY_M
                    FROM Business_ModifyVehicle mv
                    LEFT JOIN Business_AssetMaintenanceInfo mi
                        ON mv.ORIGINALID = mi.ORIGINALID")
                    .Where(i => !i.ISVERIFY)
                    .Where(i => i.MODIFY_TYPE == searchParams.MODIFY_TYPE)
                    .WhereIF(!searchParams.ENGINE_NUMBER.IsNullOrEmpty(), i => i.ENGINE_NUMBER.Contains(searchParams.ENGINE_NUMBER) )
                    .WhereIF(!searchParams.CHASSIS_NUMBER.IsNullOrEmpty(), i => i.CHASSIS_NUMBER.Contains(searchParams.CHASSIS_NUMBER))
                    .WhereIF(!searchParams.VEHICLE_SHORTNAME.IsNullOrEmpty(), i => i.VEHICLE_SHORTNAME.Contains(searchParams.VEHICLE_SHORTNAME))
                    .WhereIF(!searchParams.PLATE_NUMBER.IsNullOrEmpty(), i => i.PLATE_NUMBER.Contains(searchParams.PLATE_NUMBER))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubmitModifyVehicleReview(List<Guid> guids,string MODIFY_TYPE)
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
                       mi.START_VEHICLE_DATE AS PERIOD,
                       mi.DESCRIPTION AS DESCRIPTION,
					   mi.BOOK_TYPE_CODE,
                       mi.ASSET_ID AS ASSET_ID
                    FROM Business_ModifyVehicle mv
                    LEFT JOIN Business_AssetMaintenanceInfo mi
                        ON mv.ORIGINALID = mi.ORIGINALID").Where(x => guids.Contains(x.VGUID) && x.MODIFY_TYPE == MODIFY_TYPE).ToList();
                    var assetSwapList = new List<AssetMaintenanceInfo_Swap>();
                    switch (MODIFY_TYPE)
                    {
                        case "PLATE_NUMBER":
                            foreach (var item in modifyVehicleList)
                            {
                                //先更新资产维护表，再写入Oracle 中间表
                                //Oracle标签号  出租车车辆 沪XXXXXXX-19N     N:新增 M:改标签 C:改账套 
                                item.TAG_NUMBER = item.PLATE_NUMBER + "-" + DateTime.Now.Year.ToString().Remove(0, 2) + "M";
                                db.Updateable<Business_AssetMaintenanceInfo>()
                                    .UpdateColumns(x => new Business_AssetMaintenanceInfo { PLATE_NUMBER = item.PLATE_NUMBER, TAG_NUMBER = item.TAG_NUMBER }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                db.Updateable<Business_ModifyVehicle>()
                                    .UpdateColumns(x => new Business_ModifyVehicle { ISVERIFY = true }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                var assetSwapModel = new AssetMaintenanceInfo_Swap();
                                assetSwapModel.TRANSACTION_ID = item.VGUID;
                                assetSwapModel.TAG_NUMBER = item.TAG_NUMBER;
                                //传入订单选择的部门
                                assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                                assetSwapModel.CREATE_DATE = DateTime.Now;
                                assetSwapModel.ASSET_ID = item.ASSET_ID;
                                assetSwapModel.STATUS = "N";
                                var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Abbreviation == item.BELONGTO_COMPANY).First();
                                assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                                assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                                assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR_M;
                                assetSwapModel.MODEL_MINOR = item.MODEL_MINOR_M;
                                assetSwapModel.PERIOD = item.PERIOD;
                                assetSwapModel.BOOK_TYPE_CODE = item.BOOK_TYPE_CODE;
                                assetSwapList.Add(assetSwapModel);
                            }
                            db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                            break;
                        case "FA_LOC_1":
                            foreach (var item in modifyVehicleList)
                            {
                                //先更新资产维护表，再写入Oracle 中间表
                                db.Updateable<Business_AssetMaintenanceInfo>()
                                    .UpdateColumns(x => new Business_AssetMaintenanceInfo { MANAGEMENT_COMPANY = item.MANAGEMENT_COMPANY }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                db.Updateable<Business_ModifyVehicle>()
                                    .UpdateColumns(x => new Business_ModifyVehicle { ISVERIFY = true }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                var assetSwapModel = new AssetMaintenanceInfo_Swap();
                                assetSwapModel.TRANSACTION_ID = item.VGUID;
                                assetSwapModel.FA_LOC_1 = item.MANAGEMENT_COMPANY;
                                //传入订单选择的部门
                                assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                                assetSwapModel.CREATE_DATE = DateTime.Now;
                                assetSwapModel.ASSET_ID = item.ASSET_ID;
                                assetSwapModel.STATUS = "N";
                                var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Abbreviation == item.BELONGTO_COMPANY).First();
                                assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                                assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                                assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR_M;
                                assetSwapModel.MODEL_MINOR = item.MODEL_MINOR_M;
                                assetSwapModel.PERIOD = item.PERIOD;
                                assetSwapModel.BOOK_TYPE_CODE = item.BOOK_TYPE_CODE;
                                assetSwapList.Add(assetSwapModel);
                            }
                            db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                            break;
                        case "FA_LOC_3":
                            break;
                        case "BUSINESS_MODEL":
                            foreach (var item in modifyVehicleList)
                            {
                                //先更新资产维护表，再写入Oracle 中间表
                                db.Updateable<Business_AssetMaintenanceInfo>()
                                    .UpdateColumns(x => new Business_AssetMaintenanceInfo { MODEL_MAJOR = item.MODEL_MAJOR, MODEL_MINOR = item.MODEL_MINOR }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                db.Updateable<Business_ModifyVehicle>()
                                    .UpdateColumns(x => new Business_ModifyVehicle { ISVERIFY = true }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                var assetSwapModel = new AssetMaintenanceInfo_Swap();
                                assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR;
                                assetSwapModel.MODEL_MINOR = item.MODEL_MINOR;
                                assetSwapModel.TRANSACTION_ID = item.VGUID;
                                //传入订单选择的部门
                                assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                                assetSwapModel.CREATE_DATE = DateTime.Now;
                                assetSwapModel.ASSET_ID = item.ASSET_ID;
                                assetSwapModel.STATUS = "N";
                                var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Abbreviation == item.BELONGTO_COMPANY).First();
                                assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                                assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                                assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR;
                                assetSwapModel.MODEL_MINOR = item.MODEL_MINOR;
                                assetSwapModel.PERIOD = item.PERIOD;
                                assetSwapModel.BOOK_TYPE_CODE = item.BOOK_TYPE_CODE;
                                assetSwapList.Add(assetSwapModel);
                            }
                            db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                            break;
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