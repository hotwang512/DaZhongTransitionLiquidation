using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.ReviewAsset
{
    public class ReviewAssetController : BaseController
    {
        public ReviewAssetController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/ReviewAsset
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetReviewAssetListDatas(string YearMonth, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_AssetReview>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_AssetReview>()
                    .Where(i => i.START_VEHICLE_DATE == YearMonth && !i.ISVERIFY)
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubmitReviewAsset(string YearMonth)
        {
            var resultModel = new ResultModel<string, List<AssetDifference>>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var reviewList = db.Queryable<Business_AssetReview>()
                    .Where(i => i.START_VEHICLE_DATE == YearMonth && !i.ISVERIFY)
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToList();
                //调用车管系统接口
                var apiReault = GetNewVehicleAsset(YearMonth);
                var resultApiModel = apiReault.JsonToModel<JsonResultListApi<Api_NewVehicleAsset>>();
                var newVehicleList = resultApiModel.data;
                var lista = reviewList.Select(x => new
                    AssetDifference
                    { ENGINE_NUMBER = x.ENGINE_NUMBER, CHASSIS_NUMBER = x.CHASSIS_NUMBER, MANAGEMENT_COMPANY = x.MANAGEMENT_COMPANY_CODE, BELONGTO_COMPANY = x.BELONGTO_COMPANY_CODE }).ToList();
                var listb = newVehicleList.Select(x => new
                    AssetDifference
                    { ENGINE_NUMBER = x.ENGINE_NUMBER, CHASSIS_NUMBER = x.CHASSIS_NUMBER, MANAGEMENT_COMPANY = x.MANAGEMENT_COMPANY, BELONGTO_COMPANY = x.BELONGTO_COMPANY }).ToList();
                //获取所有的公司
                var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                if (reviewList.Count() == resultApiModel.data.Count())
                {
                    //获取所有的经营模式
                    var manageModelList = db.Queryable<Business_ManageModel>().ToList();
                    //对比数据
                    var listc = new List<AssetDifference>();
                    listc = lista.Except(listb).ToList();
                    if (listc.Count <= 0)
                    {
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
                            resultModel.IsSuccess = true;
                            resultModel.Status = "1";
                        }
                        if (resultModel.IsSuccess.TryToBoolean())
                        {
                            db.Updateable<Business_AssetReview>(reviewList).IgnoreColumns(it => new { it.ISVERIFY, it.START_VEHICLE_DATE }).ExecuteCommand();
                            //获取订单部门
                            var fixedAssetId = reviewList.First().FIXED_ASSETS_ORDERID;
                            var departmentIDsArr = db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == fixedAssetId).First().PurchaseDepartmentIDs;
                            var strArr = "";
                            foreach (var departmentID in departmentIDsArr)
                            {
                                strArr = strArr + "'" + departmentID + "',";
                            }
                            strArr = strArr.Substring(0, strArr.Length - 1);
                            var departmentList = db.SqlQueryable<PurchaseDepartmentModel>(@"SELECT VGUID,Descrption
                                                                                        FROM Business_SevenSection
                                                                                        WHERE SectionVGUID = 'D63BD715-C27D-4C47-AB66-550309794D43'
                                                                                              AND AccountModeCode = '1002'
                                                                                              AND CompanyCode = '01'
                                                                                              AND Status = '1'
                                                                                              AND Code LIKE '10%'
                                                                                              AND vguid IN (" + strArr + ")").ToList();
                            var departmentStr = "";
                            foreach (var ditem in departmentList)
                            {
                                departmentStr = departmentStr + ditem + ",";
                            }
                            departmentStr = departmentStr.Substring(0, departmentStr.Length - 1);
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
                                assetSwapModel.PERIOD = item.START_VEHICLE_DATE;
                                assetSwapModel.FA_LOC_1 = item.MANAGEMENT_COMPANY;
                                //传入订单选择的部门
                                assetSwapModel.FA_LOC_2 = departmentStr;
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
                                assetSwapModel.PERIOD = item.START_VEHICLE_DATE;
                                assetSwapList.Add(assetSwapModel);
                            }
                            db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                        }
                    }
                    else
                    {
                        //Code转名称
                        foreach (var data in listc)
                        {
                            data.BELONGTO_COMPANY =
                                ssList.First(x => x.OrgID == data.BELONGTO_COMPANY).Descrption;
                            data.MANAGEMENT_COMPANY =
                                ssList.First(x => x.OrgID == data.MANAGEMENT_COMPANY).Abbreviation;
                        }
                        resultModel.IsSuccess = false;
                        resultModel.Status = "2";
                        resultModel.ResultInfo = "数据对比不一致";
                        resultModel.ResultInfo2 = listc;
                    }
                }
                else
                {
                    var listc = new List<AssetDifference>();
                    var listd = new List<AssetDifference>();
                    listc = lista.Except(listb).ToList();
                    listd = listb.Except(lista).ToList();
                    var liste = listc.Union(listd).ToList<AssetDifference>();
                    //Code转名称
                    foreach (var data in liste)
                    {
                        data.BELONGTO_COMPANY =
                            ssList.First(x => x.OrgID == data.BELONGTO_COMPANY).Descrption;
                        data.MANAGEMENT_COMPANY =
                            ssList.First(x => x.OrgID == data.MANAGEMENT_COMPANY).Abbreviation;
                    }
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                    resultModel.ResultInfo = "平台数量：" + lista.Count + "营收系统数量：" + listb.Count;
                    resultModel.ResultInfo2 = liste;
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public string GetNewVehicleAsset(string YearMonth)
        {
            var url = ConfigSugar.GetAppString("NewVehicleAssetUrl");
            var data = "{"+ "\"YearMonth\":\"{YearMonth}\",".Replace("{YearMonth}", YearMonth) + "}";
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