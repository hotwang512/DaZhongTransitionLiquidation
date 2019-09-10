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
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.ReviewAsset
{
    public class ReviewOfficeAssetController : BaseController
    {
        public ReviewOfficeAssetController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/ReviewAsset
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("8F092486-CC43-4E74-BD49-230E9843E528");
            return View();
        }
        public JsonResult GetReviewAssetListDatas(string YearMonth, string Company, string VehicleModel, bool ISVerify)
        {
            var jsonResult = new JsonResultModel<Business_AssetReview>();

            DbBusinessDataService.Command(db =>
            {
                var startDate = YearMonth.TryToDate();
                var endDate = startDate.AddMonths(1);
                int pageCount = 0;
                jsonResult.Rows = db.Queryable<Business_AssetReview>().Where(i => !i.ISVERIFY && i.GROUP_ID != "OBD设备" && i.GROUP_ID != "出租车")
                    .WhereIF(!YearMonth.IsNullOrEmpty(), i => i.LISENSING_DATE >= startDate && i.LISENSING_DATE < endDate)
                    .WhereIF(!Company.IsNullOrEmpty(), i => i.BELONGTO_COMPANY == Company)
                    .WhereIF(!VehicleModel.IsNullOrEmpty(), i => i.VEHICLE_SHORTNAME == VehicleModel)
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToList();
                jsonResult.TotalRows = pageCount;
            });
            return Json(
                jsonResult,
                "application/json",
                Encoding.UTF8,
                JsonRequestBehavior.AllowGet
            );
            //return Json(jsonResult, MaxLengthAttributeAdapter);
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
        public JsonResult GetReviewAsset()
        {
            var resultModel = new ResultModel<string, List<AssetDifference>>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                try
                {
                    var reviewList = db.Queryable<Business_AssetReview>()
                    .Where(i => !i.ISVERIFY)
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToList();
                    if (reviewList.Count > 0)
                    {
                        //调用车管系统接口
                        var CHASSIS_NUMBER = "";
                        var ENGINE_NUMBER = "";
                        foreach (var review in reviewList)
                        {
                            CHASSIS_NUMBER += "'" + review.CHASSIS_NUMBER + "',";
                            ENGINE_NUMBER += "'" + review.ENGINE_NUMBER + "',";
                        }
                        CHASSIS_NUMBER = CHASSIS_NUMBER.Substring(0, CHASSIS_NUMBER.Length - 1);
                        ENGINE_NUMBER = ENGINE_NUMBER.Substring(0, ENGINE_NUMBER.Length - 1);
                        var apiReault = GetNewVehicleAssetByEngineChassis(CHASSIS_NUMBER, ENGINE_NUMBER);
                        var resultApiModel = apiReault.JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();

                        var newVehicleList = new List<Api_NewVehicleAsset>();
                        var resultColumn = resultApiModel.data[0].COLUMNS;
                        var resultData = resultApiModel.data[0].DATA;
                        foreach (var item in resultData)
                        {
                            var nv = new Api_NewVehicleAsset();
                            var t = nv.GetType();
                            for (var k = 0; k < resultColumn.Count; k++)
                            {
                                var pi = t.GetProperty(resultColumn[k]);
                                if (pi != null) pi.SetValue(nv, item[k], null);
                            }
                            newVehicleList.Add(nv);
                        }
                        //var bVehicleList = newVehicleList.Where(x => x.MODEL_MINOR.IsNullOrEmpty() && x.OPERATING_STATE == "在运").ToList().TryToDataTable();
                        //获取所有的公司
                        var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                            x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                        if (reviewList.Count() == resultApiModel.data[0].DATA.Count())
                        {
                            //获取所有的经营模式
                            var manageModelList = db.Queryable<Business_ManageModel>().ToList();
                            //获取所有的资产主类次类
                            var assetsCategoryList = db.Queryable<Business_AssetsCategory>().ToList();
                            foreach (var reviewItem in reviewList)
                            {
                                var newVehicle = newVehicleList.First(x =>
                                    x.ENGINE_NUMBER == reviewItem.ENGINE_NUMBER &&
                                    x.CHASSIS_NUMBER == reviewItem.CHASSIS_NUMBER);
                                reviewItem.ORIGINALID = newVehicle.ORIGINALID;
                                reviewItem.PLATE_NUMBER = newVehicle.PLATE_NUMBER;
                                //车辆简称不使用车管系统接口中传过来的，取固定资产保存的车型
                                //reviewItem.VEHICLE_SHORTNAME = newVehicle.VEHICLE_SHORTNAME; 
                                reviewItem.VEHICLE_STATE = newVehicle.VEHICLE_STATE;
                                reviewItem.OPERATING_STATE = newVehicle.OPERATING_STATE;
                                reviewItem.MODEL_MINOR = newVehicle.MODEL_MINOR;
                                reviewItem.ENGINE_NUMBER = newVehicle.ENGINE_NUMBER;
                                reviewItem.CHASSIS_NUMBER = newVehicle.CHASSIS_NUMBER;
                                reviewItem.PRODUCTION_DATE = newVehicle.PRODUCTION_DATE.TryToDate();
                                reviewItem.ORIGINALID = newVehicle.ORIGINALID;
                                reviewItem.PURCHASE_DATE = newVehicle.PURCHASE_DATE.TryToDate();
                                reviewItem.LISENSING_DATE = newVehicle.LISENSING_DATE.TryToDate();
                                reviewItem.COMMISSIONING_DATE = newVehicle.COMMISSIONING_DATE.TryToDate();
                                reviewItem.FUEL_TYPE = newVehicle.FUEL_TYPE;
                                reviewItem.DELIVERY_INFORMATION = newVehicle.DELIVERY_INFORMATION;
                                reviewItem.START_VEHICLE_DATE = newVehicle.LISENSING_DATE;
                                //Oracle标签号  出租车车辆 沪XXXXXXX-19N     N:新增 M:改标签 C:改账套 
                                reviewItem.TAG_NUMBER = newVehicle.PLATE_NUMBER + "-" + DateTime.Now.Year.ToString().Remove(0, 2) + "N";
                                //车龄 月末时间减去上牌时间（计算两个时间的月数，可能有小数点，保留整位）
                                var months = ((DateTime.Now.Year - newVehicle.LISENSING_DATE.TryToDate().Year) * 12) + (DateTime.Now.Month - newVehicle.LISENSING_DATE.TryToDate().Month);
                                reviewItem.VEHICLE_AGE = months;
                                reviewItem.YTD_DEPRECIATION = 0;
                                reviewItem.ACCT_DEPRECIATION = 0;
                                reviewItem.BOOK_TYPE_CODE = "";
                                if (assetsCategoryList.Any(x => x.ASSET_CATEGORY_MAJOR == reviewItem.ASSET_CATEGORY_MAJOR &&
                                                                  x.ASSET_CATEGORY_MINOR == reviewItem.ASSET_CATEGORY_MINOR))
                                {
                                    var category = assetsCategoryList.First(x => x.ASSET_CATEGORY_MAJOR == reviewItem.ASSET_CATEGORY_MAJOR &&
                                                                                 x.ASSET_CATEGORY_MINOR == reviewItem.ASSET_CATEGORY_MINOR);
                                    reviewItem.BOOK_TYPE_CODE = category.BOOK_TYPE_CODE;
                                }
                                var minorModel = new Business_ManageModel();
                                if (!newVehicle.MODEL_MINOR.IsNullOrEmpty())
                                {
                                    var minor = manageModelList.Where(x => x.LevelNum == 2 && x.BusinessName == newVehicle.MODEL_MINOR).ToList();
                                    //如果经营模式第三级有多个
                                    if (minor.Count > 1)
                                    {
                                        //计算出车龄，并根据车龄判断经营模式子类
                                        //reviewItem.MODEL_MINOR = manageModelList.Where(x => x.VGUID == minor.ParentVGUID && x.VehicleAge > months).OrderBy(x => x.VehicleAge).First().BusinessName;
                                        var level3 = manageModelList
                                            .Where(x => x.LevelNum == 2 && x.BusinessName == reviewItem.MODEL_MINOR && x.VehicleAge > months)
                                            .OrderBy(x => x.VehicleAge).First();
                                        minorModel = manageModelList.First(x => x.VGUID == level3.ParentVGUID);
                                        reviewItem.MODEL_MINOR = minorModel.BusinessName;
                                    }
                                    else if (minor.Count == 1)
                                    {
                                        minorModel = manageModelList
                                            .First(x => x.VGUID == minor.First().ParentVGUID);
                                        reviewItem.MODEL_MINOR = minorModel.BusinessName;
                                    }
                                    //经营模式主类 传过来的经营模式上上级
                                    var major = manageModelList.FirstOrDefault(x => x.VGUID == minorModel.ParentVGUID);
                                    reviewItem.MODEL_MAJOR = major.BusinessName;
                                    //根据经营模式和车型获取到资产主类子类，通过主类子类取到折旧方法维护中的财务信息
                                    var modelCategory = db.Queryable<Business_ManageModel_AssetsCategory>()
                                        .Where(x => x.GoodsModel == reviewItem.VEHICLE_SHORTNAME && x.ManageModelVGUID == minorModel.VGUID).First();
                                    var assetCategory = db.Queryable<Business_AssetsCategory>()
                                        .Where(x => x.VGUID == modelCategory.AssetsCategoryVGUID).First();
                                    reviewItem.ASSET_CATEGORY_MAJOR = assetCategory.ASSET_CATEGORY_MAJOR;
                                    reviewItem.ASSET_CATEGORY_MINOR = assetCategory.ASSET_CATEGORY_MINOR;
                                    reviewItem.LIFE_YEARS = assetCategory.LIFE_YEARS;
                                    reviewItem.LIFE_MONTHS = assetCategory.LIFE_MONTHS;
                                    reviewItem.SALVAGE_PERCENT = assetCategory.SALVAGE_PERCENT;
                                    reviewItem.METHOD = assetCategory.METHOD;
                                    reviewItem.BOOK_TYPE_CODE = assetCategory.BOOK_TYPE_CODE;
                                    reviewItem.ASSET_COST_ACCOUNT = assetCategory.ASSET_COST_ACCOUNT;
                                    reviewItem.ASSET_SETTLEMENT_ACCOUNT = assetCategory.ASSET_SETTLEMENT_ACCOUNT;
                                    reviewItem.DEPRECIATION_EXPENSE_SEGMENT = assetCategory.DEPRECIATION_EXPENSE_SEGMENT;
                                    reviewItem.ACCT_DEPRECIATION_ACCOUNT = assetCategory.ACCT_DEPRECIATION_ACCOUNT;
                                }
                                else
                                {
                                    if (reviewItem.OPERATING_STATE == "停运")
                                    {
                                        reviewItem.MODEL_MAJOR = "停运模式";
                                        reviewItem.MODEL_MINOR = "旧车停运";
                                    }
                                    else if (reviewItem.OPERATING_STATE == "")
                                    {
                                        reviewItem.MODEL_MAJOR = "停运模式";
                                        reviewItem.MODEL_MINOR = "新车停运";
                                    }
                                }
                                reviewItem.MODEL_MINOR = reviewItem.MODEL_MINOR == "" ? null : reviewItem.MODEL_MINOR;
                                if (!newVehicle.BELONGTO_COMPANY.IsNullOrEmpty())
                                {
                                    reviewItem.BELONGTO_COMPANY_CODE = newVehicle.BELONGTO_COMPANY;
                                    reviewItem.BELONGTO_COMPANY =
                                        ssList.First(x => x.OrgID == newVehicle.BELONGTO_COMPANY).Abbreviation;
                                    var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                        x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.OrgID == reviewItem.BELONGTO_COMPANY_CODE).First();
                                    var accountMode = db.Queryable<Business_SevenSection>().Where(x =>
                                        x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" &&
                                        x.Code == ssModel.AccountModeCode).First().Descrption;
                                    reviewItem.EXP_ACCOUNT_SEGMENT = accountMode;
                                }
                                if (!newVehicle.MANAGEMENT_COMPANY.IsNullOrEmpty())
                                {
                                    reviewItem.MANAGEMENT_COMPANY_CODE = newVehicle.MANAGEMENT_COMPANY;
                                    reviewItem.MANAGEMENT_COMPANY =
                                        ssList.First(x => x.OrgID == newVehicle.MANAGEMENT_COMPANY).Abbreviation;
                                }
                                //资产管理公司和资产所属公司调换
                                //if (!newVehicle.BELONGTO_COMPANY.IsNullOrEmpty())
                                //{
                                //    reviewItem.MANAGEMENT_COMPANY_CODE = newVehicle.BELONGTO_COMPANY;
                                //    reviewItem.MANAGEMENT_COMPANY = ssList.First(x => x.OrgID == newVehicle.BELONGTO_COMPANY).Abbreviation;
                                //}
                                //if (!newVehicle.MANAGEMENT_COMPANY.IsNullOrEmpty())
                                //{
                                //    reviewItem.BELONGTO_COMPANY_CODE = newVehicle.MANAGEMENT_COMPANY;
                                //    reviewItem.BELONGTO_COMPANY =
                                //        ssList.First(x => x.OrgID == newVehicle.MANAGEMENT_COMPANY).Descrption;
                                //    var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                //        x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.OrgID == reviewItem.BELONGTO_COMPANY_CODE).First();
                                //    var accountMode = db.Queryable<Business_SevenSection>().Where(x =>
                                //        x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" &&
                                //        x.Code == ssModel.AccountModeCode).First().Descrption;
                                //    reviewItem.EXP_ACCOUNT_SEGMENT = accountMode;
                                //}
                            }
                            resultModel.IsSuccess = true;
                            resultModel.Status = "1";
                            if (resultModel.IsSuccess.TryToBoolean())
                            {
                                db.Updateable<Business_AssetReview>(reviewList).IgnoreColumns(it => new { it.CREATE_DATE, it.CREATE_USER }).ExecuteCommand();
                            }
                        }
                        else
                        {
                            resultModel.IsSuccess = false;
                            resultModel.Status = "3";
                            resultModel.ResultInfo = "请求数量与获取到的数量不一致";
                        }
                    }
                    else
                    {
                        resultModel.IsSuccess = false;
                        resultModel.Status = "3";
                        resultModel.ResultInfo = "没有待审核的车辆";
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCompany()
        {
            var jsonResult = new JsonResultModel<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult.Rows = db.Queryable<Business_SevenSection>().Where(x =>
                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").ToList();
            });
            return Json(jsonResult.Rows, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SubmitReviewAsset(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, List<AssetDifference>>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var reviewList = db.Queryable<Business_AssetReview>()
                    .Where(i => vguids.Contains(i.VGUID) && !i.ISVERIFY)
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToList();
                if (!reviewList.Any(x => x.TAG_NUMBER.IsNullOrEmpty()))
                {
                    //先进中间表再进资产表
                    //资产新增后写入Oracle中间表
                    var assetSwapList = new List<AssetMaintenanceInfo_Swap>();
                    try
                    {
                        var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                            x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
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
                            assetSwapModel.ASSET_CREATION_DATE = item.LISENSING_DATE;
                            assetSwapModel.ASSET_COST = item.ASSET_COST;
                            assetSwapModel.AMORTIZATION_FLAG = item.AMORTIZATION_FLAG;
                            assetSwapModel.YTD_DEPRECIATION = item.YTD_DEPRECIATION;
                            assetSwapModel.ACCT_DEPRECIATION = item.ACCT_DEPRECIATION;
                            assetSwapModel.FA_LOC_1 = item.BELONGTO_COMPANY;
                            assetSwapModel.FA_LOC_2 = item.MANAGEMENT_COMPANY;
                            //传入订单选择的部门
                            assetSwapModel.FA_LOC_3 = item.ORGANIZATION_NUM;
                            assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                            assetSwapModel.CREATE_DATE = DateTime.Now;
                            assetSwapModel.ASSET_ID = item.ASSET_ID;
                            assetSwapModel.STATUS = "N";
                            if (!item.BELONGTO_COMPANY.IsNullOrEmpty())
                            {
                                var ssModel = ssList.First(x => x.Abbreviation == item.BELONGTO_COMPANY);
                                assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                            }
                            assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                            assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR;
                            assetSwapModel.MODEL_MINOR = item.MODEL_MINOR;
                            assetSwapModel.ASSET_CREATION_DATE = item.LISENSING_DATE;
                            assetSwapModel.PERIOD = item.START_VEHICLE_DATE;
                            assetSwapList.Add(assetSwapModel);
                        }
                        db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                        var assetInfoList = new List<Business_AssetMaintenanceInfo>();
                        foreach (var reviewItem in reviewList)
                        {
                            var asset = new Business_AssetMaintenanceInfo();
                            asset.VGUID = reviewItem.VGUID;
                            asset.ORIGINALID = reviewItem.ORIGINALID;
                            asset.GROUP_ID = reviewItem.GROUP_ID;
                            asset.PLATE_NUMBER = reviewItem.PLATE_NUMBER;
                            asset.TAG_NUMBER = reviewItem.TAG_NUMBER;
                            asset.VEHICLE_SHORTNAME = reviewItem.VEHICLE_SHORTNAME;
                            asset.ORGANIZATION_NUM = reviewItem.ORGANIZATION_NUM;
                            asset.MANAGEMENT_COMPANY = reviewItem.MANAGEMENT_COMPANY;
                            asset.BELONGTO_COMPANY = reviewItem.BELONGTO_COMPANY;
                            asset.ASSET_ID = reviewItem.ASSET_ID;
                            asset.VEHICLE_STATE = reviewItem.VEHICLE_STATE;
                            asset.OPERATING_STATE = reviewItem.OPERATING_STATE;
                            asset.DESCRIPTION = reviewItem.DESCRIPTION;
                            asset.ENGINE_NUMBER = reviewItem.ENGINE_NUMBER;
                            asset.CHASSIS_NUMBER = reviewItem.CHASSIS_NUMBER;
                            asset.PRODUCTION_DATE = reviewItem.PRODUCTION_DATE;
                            asset.PURCHASE_DATE = reviewItem.PURCHASE_DATE;
                            asset.LISENSING_DATE = reviewItem.LISENSING_DATE;
                            asset.COMMISSIONING_DATE = reviewItem.COMMISSIONING_DATE;
                            asset.VEHICLE_AGE = reviewItem.VEHICLE_AGE;
                            asset.BACK_CAR_DATE = reviewItem.BACK_CAR_DATE;
                            asset.FUEL_TYPE = reviewItem.FUEL_TYPE;
                            asset.DELIVERY_INFORMATION = reviewItem.DELIVERY_INFORMATION;
                            asset.QUANTITY = reviewItem.QUANTITY;
                            asset.ASSET_COST = reviewItem.ASSET_COST;
                            asset.NUDE_CAR_FEE = reviewItem.NUDE_CAR_FEE;
                            asset.PURCHASE_TAX = reviewItem.PURCHASE_TAX;
                            asset.LISENSING_FEE = reviewItem.LISENSING_FEE;
                            asset.OUT_WAREHOUSE_FEE = reviewItem.OUT_WAREHOUSE_FEE;
                            asset.DOME_LIGHT_FEE = reviewItem.DOME_LIGHT_FEE;
                            asset.ANTI_ROBBERY_FEE = reviewItem.ANTI_ROBBERY_FEE;
                            asset.LOADING_FEE = reviewItem.LOADING_FEE;
                            asset.INNER_ROOF_FEE = reviewItem.INNER_ROOF_FEE;
                            asset.TAXIMETER_FEE = reviewItem.TAXIMETER_FEE;
                            asset.ASSET_DISPOSITION_TYPE = reviewItem.ASSET_DISPOSITION_TYPE;
                            asset.SCRAP_INFORMATION = reviewItem.SCRAP_INFORMATION;
                            asset.DISPOSAL_AMOUNT = reviewItem.DISPOSAL_AMOUNT;
                            asset.DISPOSAL_TAX = reviewItem.DISPOSAL_TAX;
                            asset.DISPOSAL_PROFIT_LOSS = reviewItem.DISPOSAL_PROFIT_LOSS;
                            asset.BAK_CAR_AGE = reviewItem.BAK_CAR_AGE;
                            asset.ASSET_CATEGORY_MAJOR = reviewItem.ASSET_CATEGORY_MAJOR;
                            asset.ASSET_CATEGORY_MINOR = reviewItem.ASSET_CATEGORY_MINOR;
                            asset.SALVAGE_TYPE = reviewItem.SALVAGE_TYPE;
                            asset.SALVAGE_PERCENT = reviewItem.SALVAGE_PERCENT;
                            asset.SALVAGE_VALUE = reviewItem.SALVAGE_VALUE;
                            asset.AMORTIZATION_FLAG = reviewItem.AMORTIZATION_FLAG;
                            asset.METHOD = reviewItem.METHOD;
                            asset.LIFE_YEARS = reviewItem.LIFE_YEARS;
                            asset.LIFE_MONTHS = reviewItem.LIFE_MONTHS;
                            asset.BOOK_TYPE_CODE = reviewItem.BOOK_TYPE_CODE;
                            asset.ASSET_COST_ACCOUNT = reviewItem.ASSET_COST_ACCOUNT;
                            asset.ASSET_SETTLEMENT_ACCOUNT = reviewItem.ASSET_SETTLEMENT_ACCOUNT;
                            asset.DEPRECIATION_EXPENSE_SEGMENT = reviewItem.DEPRECIATION_EXPENSE_SEGMENT;
                            asset.ACCT_DEPRECIATION_ACCOUNT = reviewItem.ACCT_DEPRECIATION_ACCOUNT;
                            asset.YTD_DEPRECIATION = reviewItem.YTD_DEPRECIATION;
                            asset.ACCT_DEPRECIATION = reviewItem.ACCT_DEPRECIATION;
                            asset.EXP_ACCOUNT_SEGMENT = reviewItem.EXP_ACCOUNT_SEGMENT;
                            asset.MODEL_MAJOR = reviewItem.MODEL_MAJOR;
                            asset.MODEL_MINOR = reviewItem.MODEL_MINOR;
                            asset.START_VEHICLE_DATE = reviewItem.START_VEHICLE_DATE;
                            asset.CREATE_DATE = DateTime.Now;
                            asset.CREATE_USER = cache[PubGet.GetUserKey].LoginName;
                            reviewItem.ISVERIFY = true;
                            assetInfoList.Add(asset);
                        }
                        db.Insertable<Business_AssetMaintenanceInfo>(assetInfoList).ExecuteCommand();
                        db.Updateable<Business_AssetReview>(reviewList).UpdateColumns(it => new { it.ISVERIFY }).ExecuteCommand();
                        resultModel.IsSuccess = true;
                        resultModel.Status = "1";
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(string.Format("Data:{0},result:{1}", ex.Data, ex.InnerException));
                    }
                }
                else
                {
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                    resultModel.ResultInfo = "标签号不允许为空";
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateTagNumber(Business_AssetReview review)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                db.Updateable<Business_AssetReview>(review).UpdateColumns(x => new {x.TAG_NUMBER}).ExecuteCommand();
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public string GetNewVehicleAsset(string YearMonth)
        {
            YearMonth = YearMonth.Replace("-", "");
            var url = ConfigSugar.GetAppString("NewVehicleAssetUrl");
            var data = "{" + "\"YearMonth\":\"{YearMonth}\"".Replace("{YearMonth}", YearMonth) + "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), "POST", data);
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
                return resultData;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
                return "";
            }
        }
        public string GetNewVehicleAssetByEngineChassis(string CHASSIS_NUMBER, string ENGINE_NUMBER)
        {
            var url = ConfigSugar.GetAppString("NewVehicleAssetUrl");
            var data = "{" + "\"CHASSIS_NUMBER\":\"{CHASSIS_NUMBER}\"".Replace("{CHASSIS_NUMBER}", CHASSIS_NUMBER) + ",\"ENGINE_NUMBER\":\"{ENGINE_NUMBER}\"".Replace("{ENGINE_NUMBER}", ENGINE_NUMBER) + "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), "POST", data);
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