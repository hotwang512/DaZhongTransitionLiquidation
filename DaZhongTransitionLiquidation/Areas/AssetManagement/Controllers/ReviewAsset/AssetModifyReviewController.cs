using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Controllers;
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
                    .Where(i => i.MODIFY_TYPE == searchParams.MODIFY_TYPE)
                    .WhereIF(!searchParams.ENGINE_NUMBER.IsNullOrEmpty(), i => i.ENGINE_NUMBER.Contains(searchParams.ENGINE_NUMBER) )
                    .WhereIF(!searchParams.CHASSIS_NUMBER.IsNullOrEmpty(), i => i.CHASSIS_NUMBER.Contains(searchParams.CHASSIS_NUMBER))
                    .WhereIF(!searchParams.VEHICLE_SHORTNAME.IsNullOrEmpty(), i => i.VEHICLE_SHORTNAME.Contains(searchParams.VEHICLE_SHORTNAME))
                    .WhereIF(!searchParams.PLATE_NUMBER.IsNullOrEmpty(), i => i.PLATE_NUMBER.Contains(searchParams.PLATE_NUMBER))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToList();
                    //.OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                //var list = jsonResult.Rows = db.SqlQueryable<Business_ModifyVehicleModel>
                //(@"SELECT mv.*,
                //       mi.PLATE_NUMBER AS PLATE_NUMBER_M,
                //       mi.MODEL_MAJOR AS MODEL_MAJOR_M,
                //       mi.MODEL_MINOR AS MODEL_MINOR_M,
                //       mi.MANAGEMENT_COMPANY AS MANAGEMENT_COMPANY_M,
                //       mi.BELONGTO_COMPANY AS BELONGTO_COMPANY_M
                //    FROM Business_ModifyVehicle mv
                //    LEFT JOIN Business_AssetMaintenanceInfo mi
                //        ON mv.ORIGINALID = mi.ORIGINALID").OrderBy(x => x.MODEL_MAJOR).ToList();
                //var table1 = list.TryToDataTable();;
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
        public JsonResult SubmitModifyVehicleReview(List<Guid> vguids, string MODIFY_TYPE)
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
                        ON mv.ORIGINALID = mi.ORIGINALID").Where(x => vguids.Contains(x.VGUID) && x.MODIFY_TYPE == MODIFY_TYPE).ToList();
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
                                assetSwapModel.TRANSACTION_ID =Guid.NewGuid();
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
                                assetSwapModel.CHECK_STATE = false;
                                assetSwapModel.PROCESS_TYPE = "PLATE_NUMBER";
                                assetSwapList.Add(assetSwapModel);
                            }
                            break;
                        case "FA_LOC_1":
                            foreach (var item in modifyVehicleList)
                            {
                                //先更新资产维护表，再写入Oracle 中间表
                                db.Updateable<Business_AssetMaintenanceInfo>()
                                    .UpdateColumns(x => new Business_AssetMaintenanceInfo { BELONGTO_COMPANY = item.FA_LOC_1 }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                db.Updateable<Business_ModifyVehicle>()
                                    .UpdateColumns(x => new Business_ModifyVehicle { ISVERIFY = true }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                var assetSwapModel = new AssetMaintenanceInfo_Swap();
                                assetSwapModel.TRANSACTION_ID =Guid.NewGuid();
                                assetSwapModel.FA_LOC_1 = item.FA_LOC_1;
                                assetSwapModel.FA_LOC_2 = item.MANAGEMENT_COMPANY_M;
                                assetSwapModel.FA_LOC_3 = item.FA_LOC_3;
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
                                assetSwapModel.CHECK_STATE = false;
                                assetSwapModel.PROCESS_TYPE = "FA_LOC_1";
                                assetSwapList.Add(assetSwapModel);
                            }
                            break;
                        case "FA_LOC_2":
                            foreach (var item in modifyVehicleList)
                            {
                                //先更新资产维护表，再写入Oracle 中间表
                                db.Updateable<Business_AssetMaintenanceInfo>()
                                    .UpdateColumns(x => new Business_AssetMaintenanceInfo { MANAGEMENT_COMPANY = item.FA_LOC_2 }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                db.Updateable<Business_ModifyVehicle>()
                                    .UpdateColumns(x => new Business_ModifyVehicle { ISVERIFY = true }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                var assetSwapModel = new AssetMaintenanceInfo_Swap();
                                assetSwapModel.TRANSACTION_ID =Guid.NewGuid();
                                assetSwapModel.FA_LOC_1 = item.BELONGTO_COMPANY_M;
                                assetSwapModel.FA_LOC_2 = item.FA_LOC_2;
                                assetSwapModel.FA_LOC_3 = item.FA_LOC_3;
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
                                assetSwapModel.CHECK_STATE = false;
                                assetSwapModel.PROCESS_TYPE = "FA_LOC_2";
                                assetSwapList.Add(assetSwapModel);
                            }
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
                                var info = db.Queryable<Business_AssetMaintenanceInfo>()
                                    .Where(x => x.ORIGINALID == item.ORIGINALID).First();
                                if (info.ASSET_CATEGORY_MINOR != item.ASSET_CATEGORY_MINOR)
                                {
                                    assetSwapModel.ASSET_CATEGORY_MAJOR = item.ASSET_CATEGORY_MAJOR;
                                    assetSwapModel.ASSET_CATEGORY_MINOR = item.ASSET_CATEGORY_MINOR;
                                }
                                assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR;
                                assetSwapModel.MODEL_MINOR = item.MODEL_MINOR;
                                assetSwapModel.TRANSACTION_ID =Guid.NewGuid();
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
                                assetSwapModel.CHECK_STATE = false;
                                assetSwapModel.PROCESS_TYPE = "BUSINESS_MODEL";
                                assetSwapList.Add(assetSwapModel);
                            }
                            break;
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
        public JsonResult GetModifyVehicleReview(string MODIFY_TYPE,string YearMonth)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    List<Api_ModifyVehicleAsset> assetModifyFlowList = new List<Api_ModifyVehicleAsset>();
                    var lastMonthDate = DateTime.Now.AddMonths(-1);
                    //var YearMonth = lastMonthDate.Year.ToString() + lastMonthDate.Month.ToString().PadLeft(2, '0');
                    YearMonth = YearMonth.Replace("-", "");
                    var apiReaultModify = AssetMaintenanceAPI.GetModifyVehicleAsset(YearMonth);
                    var resultApiModifyModel = apiReaultModify
                        .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
                    //全量获取车辆信息
                    {
                        var resultColumn = resultApiModifyModel.data[0].COLUMNS;
                        var resultData = resultApiModifyModel.data[0].DATA;
                        foreach (var item in resultData)
                        {
                            var nv = new Api_ModifyVehicleAsset();
                            var t = nv.GetType();
                            for (var k = 0; k < resultColumn.Count; k++)
                            {
                                var pi = t.GetProperty(resultColumn[k]);
                                if (pi != null) pi.SetValue(nv, item[k], null);
                            }
                            assetModifyFlowList.Add(nv);
                        }

                        //var dt = assetModifyFlowList.Where(x => x.OPERATING_STATE == "在运" && x.MODEL_MINOR == "").ToList().TryToDataTable();
                        AutoSyncAssetsMaintenance.WirterSyncModifyAssetFlow(assetModifyFlowList, MODIFY_TYPE);
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