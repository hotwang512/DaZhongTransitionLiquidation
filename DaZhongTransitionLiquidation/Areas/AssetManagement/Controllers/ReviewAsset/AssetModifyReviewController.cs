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
                       mi.PERIOD AS PERIOD,
                       mi.DESCRIPTION AS DESCRIPTION,
                       mi.ASSET_ID AS ASSET_ID
                    FROM Business_ModifyVehicle mv
                    LEFT JOIN Business_AssetMaintenanceInfo mi
                        ON mv.ORIGINALID = mi.ORIGINALID").Where(x => guids.Contains(x.VGUID) && x.MODIFY_TYPE == MODIFY_TYPE).ToList();
                    //获取所有的经营模式
                    var manageModelList = db.Queryable<Business_ManageModel>().ToList();
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
                                var assetSwapModel = new AssetMaintenanceInfo_Swap();
                                assetSwapModel.TRANSACTION_ID = item.VGUID;
                                assetSwapModel.TAG_NUMBER = item.TAG_NUMBER;
                                //assetSwapModel.DESCRIPTION = item.DESCRIPTION;
                                //assetSwapModel.FA_LOC_1 = item.MANAGEMENT_COMPANY;
                                //传入订单选择的部门
                                assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                                assetSwapModel.CREATE_DATE = DateTime.Now;
                                assetSwapModel.ASSET_ID = item.ASSET_ID;
                                assetSwapModel.STATUS = "N";
                                var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Descrption == item.BELONGTO_COMPANY).First();
                                assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                                //assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                                assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR_M;
                                assetSwapModel.MODEL_MINOR = item.MODEL_MINOR_M;
                                assetSwapModel.PERIOD = item.PERIOD;
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
                                var assetSwapModel = new AssetMaintenanceInfo_Swap();
                                assetSwapModel.TRANSACTION_ID = item.VGUID;
                                assetSwapModel.FA_LOC_1 = item.MANAGEMENT_COMPANY;
                                //传入订单选择的部门
                                assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                                assetSwapModel.CREATE_DATE = DateTime.Now;
                                assetSwapModel.ASSET_ID = item.ASSET_ID;
                                assetSwapModel.STATUS = "N";
                                var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Descrption == item.BELONGTO_COMPANY).First();
                                assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                                assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                                assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR_M;
                                assetSwapModel.MODEL_MINOR = item.MODEL_MINOR_M;
                                assetSwapModel.PERIOD = item.PERIOD;
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
                                //经营模式子类 传过来的经营模式上级
                                var minor = manageModelList.FirstOrDefault(x => x.BusinessName == item.MODEL_MINOR);
                                item.MODEL_MINOR = manageModelList
                                    .First(x => minor != null && x.VGUID == minor.ParentVGUID).BusinessName;
                                //经营模式主类 传过来的经营模式上上级
                                var major = manageModelList.FirstOrDefault(x => x.BusinessName == item.MODEL_MINOR);
                                item.MODEL_MAJOR = manageModelList
                                    .First(x => major != null && x.VGUID == major.ParentVGUID).BusinessName;
                                db.Updateable<Business_AssetMaintenanceInfo>()
                                    .UpdateColumns(x => new Business_AssetMaintenanceInfo { MODEL_MAJOR = item.MODEL_MAJOR, MODEL_MINOR = item.MODEL_MINOR }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                                var assetSwapModel = new AssetMaintenanceInfo_Swap();
                                assetSwapModel.TRANSACTION_ID = item.VGUID;
                                //传入订单选择的部门
                                assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                                assetSwapModel.CREATE_DATE = DateTime.Now;
                                assetSwapModel.ASSET_ID = item.ASSET_ID;
                                assetSwapModel.STATUS = "N";
                                var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Descrption == item.BELONGTO_COMPANY).First();
                                assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                                assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                                assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR_M;
                                assetSwapModel.MODEL_MINOR = item.MODEL_MINOR_M;
                                assetSwapModel.PERIOD = item.PERIOD;
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
        public JsonResult SubmitReviewAsset(string YearMonth)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var reviewList = db.Queryable<Business_AssetReview>()
                    .Where(i => i.START_VEHICLE_DATE == YearMonth && !i.ISVERIFY)
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToList();
                //调用车管系统接口
                var apiReault = GetNewVehicleAsset(YearMonth);
                var resultApiModel = apiReault.JsonToModel<JsonResultListApi<Api_NewVehicleAsset>>();
                var newVehicleList = resultApiModel.data;
                if (reviewList.Count() == resultApiModel.data.Count())
                {
                    //获取所有的经营模式
                    var manageModelList = db.Queryable<Business_ManageModel>().ToList();
                    //对比数据
                    foreach (var reviewItem in reviewList)
                    {
                        if (newVehicleList.Any(x => x.ENGINE_NUMBER == reviewItem.ENGINE_NUMBER
                                                    && x.CHASSIS_NUMBER == reviewItem.CHASSIS_NUMBER
                                                    && x.MANAGEMENT_COMPANY == reviewItem.MANAGEMENT_COMPANY
                                                    && x.BELONGTO_COMPANY == reviewItem.BELONGTO_COMPANY))
                        {
                            var newVehicle = newVehicleList.First(x =>
                                x.ENGINE_NUMBER == reviewItem.ENGINE_NUMBER &&
                                x.CHASSIS_NUMBER == reviewItem.CHASSIS_NUMBER);
                            reviewItem.ORIGINALID = newVehicle.ORIGINALID;
                            reviewItem.PLATE_NUMBER = newVehicle.PLATE_NUMBER;
                            reviewItem.VEHICLE_SHORTNAME = newVehicle.VEHICLE_SHORTNAME;
                            reviewItem.VEHICLE_STATE = newVehicle.VEHICLE_STATE;
                            reviewItem.OPERATING_STATE = newVehicle.OPERATING_STATE;
                            reviewItem.MODEL_MINOR = newVehicle.MODEL_MINOR;
                            reviewItem.ENGINE_NUMBER = newVehicle.ENGINE_NUMBER;
                            reviewItem.CHASSIS_NUMBER = newVehicle.CHASSIS_NUMBER;
                            reviewItem.PRODUCTION_DATE = newVehicle.PRODUCTION_DATE;
                            reviewItem.ORIGINALID = newVehicle.ORIGINALID;
                            reviewItem.PURCHASE_DATE = newVehicle.PURCHASE_DATE;
                            reviewItem.LISENSING_DATE = newVehicle.LISENSING_DATE;
                            reviewItem.COMMISSIONING_DATE = newVehicle.COMMISSIONING_DATE;
                            reviewItem.FUEL_TYPE = newVehicle.FUEL_TYPE;
                            reviewItem.DELIVERY_INFORMATION = newVehicle.DELIVERY_INFORMATION;
                            reviewItem.ISVERIFY = true;
                            //Oracle标签号  出租车车辆 沪XXXXXXX-19N     N:新增 M:改标签 C:改账套 
                            reviewItem.TAG_NUMBER = newVehicle.PLATE_NUMBER + "-" + DateTime.Now.Year.ToString().Remove(0, 2) + "N";
                            //车龄 月末时间减去上牌时间（计算两个时间的月数，可能有小数点，保留整位）
                            var months = ((DateTime.Now.Year - newVehicle.LISENSING_DATE.TryToDate().Year) * 12) + (DateTime.Now.Month - newVehicle.LISENSING_DATE.TryToDate().Month);
                            reviewItem.VEHICLE_AGE = months;
                            reviewItem.YTD_DEPRECIATION = 0;
                            reviewItem.ACCT_DEPRECIATION = 0;
                            //经营模式子类 传过来的经营模式上级
                            var minor = manageModelList.FirstOrDefault(x => x.BusinessName == newVehicle.MODEL_MINOR);
                            reviewItem.MODEL_MINOR = manageModelList
                                .First(x => minor != null && x.VGUID == minor.ParentVGUID).BusinessName;
                            //经营模式主类 传过来的经营模式上上级
                            var major = manageModelList.FirstOrDefault(x => x.BusinessName == reviewItem.MODEL_MINOR);
                            reviewItem.MODEL_MAJOR = manageModelList
                                .First(x => major != null && x.VGUID == major.ParentVGUID).BusinessName;
                            reviewItem.ISVERIFY = true;
                        }
                        else
                        {
                            resultModel.IsSuccess = false;
                            resultModel.Status = "2";
                            resultModel.ResultInfo = "数据存在不一致";
                            break;
                        }
                        resultModel.IsSuccess = true;
                        resultModel.Status = "1";
                    }
                    if (resultModel.IsSuccess)
                    {
                        db.Updateable<Business_AssetReview>(reviewList).IgnoreColumns(it => new { it.ISVERIFY, it.START_VEHICLE_DATE }).ExecuteCommand();
                        //资产新增后写入Oracle中间表
                        var assetSwapList = new List<AssetMaintenanceInfo_Swap>();
                        foreach (var item in reviewList)
                        {
                            var assetSwapModel = new AssetMaintenanceInfo_Swap();
                            assetSwapModel.TRANSACTION_ID = item.VGUID;
                            assetSwapModel.BOOK_TYPE_CODE = item.BOOK_TYPE_CODE;
                            assetSwapModel.TAG_NUMBER = item.TAG_NUMBER;
                            assetSwapModel.DESCRIPTION = item.DESCRIPTION;
                            assetSwapModel.QUANTITY = item.QUANTITY;
                            assetSwapModel.ASSET_CATEGORY_MAJOR = item.ASSET_CATEGORY_MAJOR;
                            assetSwapModel.ASSET_CATEGORY_MINOR = item.ASSET_CATEGORY_MINOR;
                            assetSwapModel.ASSET_CREATION_DATE = item.COMMISSIONING_DATE;
                            assetSwapModel.ASSET_COST = item.ASSET_COST;
                            assetSwapModel.AMORTIZATION_FLAG = item.AMORTIZATION_FLAG;
                            assetSwapModel.YTD_DEPRECIATION = item.YTD_DEPRECIATION;
                            assetSwapModel.ACCT_DEPRECIATION = item.ACCT_DEPRECIATION;
                            //assetSwapModel.PERIOD = DateTime.Now;
                            assetSwapModel.FA_LOC_1 = item.MANAGEMENT_COMPANY;
                            assetSwapModel.FA_LOC_2 = item.BELONGTO_COMPANY;
                            assetSwapModel.FA_LOC_3 = "000000";
                            assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                            assetSwapModel.CREATE_DATE = DateTime.Now;
                            assetSwapModel.ASSET_ID = item.ASSET_ID;
                            assetSwapModel.STATUS = "N";
                            var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Descrption == item.BELONGTO_COMPANY).First();
                            assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                            assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                            assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR;
                            assetSwapModel.MODEL_MINOR = item.MODEL_MINOR;
                            assetSwapList.Add(assetSwapModel);
                        }
                        db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                    }
                }
                else
                {
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                    resultModel.ResultInfo = "数量不一致";
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public string GetNewVehicleAsset(string YearMonth)
        {
            var url = ConfigSugar.GetAppString("NewVehicleAssetUrl");
            var data = "{" + "\"YearMonth\":\"{YearMonth}\",".Replace("{YearMonth}", YearMonth) + "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), "GET", data);
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
                return resultData;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
                return "";
            }
        }
    }
}