using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.ReviewAsset
{
    public class ReviewIntangibleAssetsController : BaseController
    {
        public ReviewIntangibleAssetsController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetManagement/ReviewIntangibleAssets
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("49d07bc5-66a9-496f-b5ad-2b3e986590d8");
            return View();
        }
        public JsonResult GetIntangibleAssetsOrderListDatas(Business_IntangibleAssetsOrder searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_IntangibleAssetsOrder>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_IntangibleAssetsOrder>().Where(x => x.NeedVerifie && !x.ISVerify)
                    .WhereIF(searchParams.OSNO != null, i => i.OSNO.Contains(searchParams.OSNO))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubmitIntangibleAssets(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            var assetInfoList = new List<Business_AssetMaintenanceInfo>();
            var assetSwapList = new List<AssetMaintenanceInfo_Swap>();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var assetsCategoryList = db.Queryable<Business_AssetsCategory>().ToList();
                    //获取订单部门
                    var departmentList = db.SqlQueryable<PurchaseDepartmentModel>(@"SELECT VGUID,Descrption
                                                                                        FROM Business_SevenSection
                                                                                        WHERE SectionVGUID = 'D63BD715-C27D-4C47-AB66-550309794D43'
                                                                                              AND AccountModeCode = '1002'
                                                                                              AND CompanyCode = '01'
                                                                                              AND Status = '1'
                                                                                              AND Code LIKE '10%'").ToList();
                    foreach (var vguid in vguids)
                    {
                        var assetsOrderInfo =
                        db.Queryable<Business_IntangibleAssetsOrder>().Where(x => x.VGUID == vguid).First();
                        var departmentIDsArr = db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == assetsOrderInfo.VGUID).First().PurchaseDepartmentIDs.Split(",");
                        var strList = new List<Guid>();
                        foreach (var departmentID in departmentIDsArr)
                        {
                            strList.Add(departmentID.TryToGuid());
                        }
                        var departments = departmentList.Where(x => strList.Contains(x.VGUID));
                        var departmentStr = "";
                        foreach (var ditem in departments)
                        {
                            departmentStr = departmentStr + ditem.Descrption + ",";
                        }
                        departmentStr = departmentStr.Substring(0, departmentStr.Length - 1);
                        var orderSetting = db.Queryable<Business_PurchaseOrderSetting>()
                            .Where(x => x.VGUID == assetsOrderInfo.PurchaseGoodsVguid).First();
                        var assetInfo = new Business_AssetMaintenanceInfo();
                        assetInfo.VGUID = Guid.NewGuid();
                        var autoID = "InAssetID";
                        var no = CreateNo.GetCreateNo(db, autoID);
                        assetInfo.ASSET_ID = no;
                        assetInfo.GROUP_ID = "无形资产";
                        //assetInfo.PLATE_NUMBER = item.PlateNumber;
                        //assetInfo.CHASSIS_NUMBER = item.EquipmentNumber;
                        //assetInfo.VEHICLE_SHORTNAME = "";
                        assetInfo.DESCRIPTION = assetInfo.DESCRIPTION;
                        //assetInfo.LISENSING_DATE = item.LisensingDate;
                        assetInfo.TAG_NUMBER = assetInfo.ASSET_ID.Replace("CZ", "WX");
                        //assetInfo.START_VEHICLE_DATE = assetsOrderInfo.LISENSING_DATE;
                        assetInfo.PURCHASE_DATE = assetsOrderInfo.CreateDate;
                        assetInfo.QUANTITY = 1;
                        assetInfo.ASSET_COST = assetsOrderInfo.SumPayment;
                        //资产主类次类 根据采购物品获取
                        assetInfo.ASSET_CATEGORY_MAJOR = orderSetting.AssetCategoryMajor;
                        assetInfo.ASSET_CATEGORY_MINOR = orderSetting.AssetCategoryMinor;
                        //根据主类子类从折旧方法表中获取
                        var assetsCategoryInfo = assetsCategoryList.First(x => x.ASSET_CATEGORY_MAJOR == assetInfo.ASSET_CATEGORY_MAJOR &&
                                                                               x.ASSET_CATEGORY_MINOR == assetInfo.ASSET_CATEGORY_MINOR);
                        assetInfo.LIFE_YEARS = assetsCategoryInfo.LIFE_YEARS;
                        assetInfo.LIFE_MONTHS = assetsCategoryInfo.LIFE_MONTHS;
                        assetInfo.AMORTIZATION_FLAG = "N";
                        assetInfo.METHOD = assetsCategoryInfo.METHOD;
                        assetInfo.BOOK_TYPE_CODE = assetsCategoryInfo.BOOK_TYPE_CODE;
                        assetInfo.ASSET_COST_ACCOUNT = assetsCategoryInfo.ASSET_COST_ACCOUNT;
                        assetInfo.ASSET_SETTLEMENT_ACCOUNT = assetsCategoryInfo.ASSET_SETTLEMENT_ACCOUNT;
                        assetInfo.DEPRECIATION_EXPENSE_SEGMENT = assetsCategoryInfo.DEPRECIATION_EXPENSE_SEGMENT;
                        assetInfo.ACCT_DEPRECIATION_ACCOUNT = assetsCategoryInfo.ACCT_DEPRECIATION_ACCOUNT;
                        assetInfo.SALVAGE_PERCENT = assetsCategoryInfo.SALVAGE_PERCENT;
                        //assetInfo.ISVERIFY = false;
                        assetInfo.YTD_DEPRECIATION = 0;
                        assetInfo.ACCT_DEPRECIATION = 0;
                        //assetInfo.FIXED_ASSETS_ORDERID = vguid;
                        assetInfo.CREATE_USER = cache[PubGet.GetUserKey].LoginName;
                        assetInfo.CREATE_DATE = DateTime.Now;
                        assetInfo.ORGANIZATION_NUM = "财务共享-" + departmentStr;
                        assetInfo.BELONGTO_COMPANY = db.Queryable<Business_SevenSection>()
                            .Where(x => x.Descrption == assetsOrderInfo.PayCompany).First().Abbreviation;
                        assetInfo.MANAGEMENT_COMPANY = assetInfo.BELONGTO_COMPANY;
                        assetInfo.MODEL_MAJOR = "无";
                        assetInfo.MODEL_MINOR = "无";
                        //assetInfo.EXP_ACCOUNT_SEGMENT = assetInfo.EXP_ACCOUNT_SEGMENT;
                        //assetInfo.VEHICLE_STATE = assetInfo.VEHICLE_STATE;
                        //assetInfo.OPERATING_STATE = assetInfo.OPERATING_STATE;
                        assetInfoList.Add(assetInfo);
                        var assetSwapModel = new AssetMaintenanceInfo_Swap();
                        assetSwapModel.TRANSACTION_ID = assetInfo.VGUID;
                        assetSwapModel.BOOK_TYPE_CODE = assetInfo.BOOK_TYPE_CODE;
                        assetSwapModel.TAG_NUMBER = assetInfo.TAG_NUMBER;
                        assetSwapModel.DESCRIPTION = assetInfo.DESCRIPTION;
                        assetSwapModel.QUANTITY = assetInfo.QUANTITY;
                        assetSwapModel.ASSET_CATEGORY_MAJOR = assetInfo.ASSET_CATEGORY_MAJOR;
                        assetSwapModel.ASSET_CATEGORY_MINOR = assetInfo.ASSET_CATEGORY_MINOR;
                        assetSwapModel.ASSET_CREATION_DATE = assetInfo.LISENSING_DATE;
                        assetSwapModel.ASSET_COST = assetInfo.ASSET_COST;
                        assetSwapModel.AMORTIZATION_FLAG = assetInfo.AMORTIZATION_FLAG;
                        assetSwapModel.YTD_DEPRECIATION = assetInfo.YTD_DEPRECIATION;
                        assetSwapModel.ACCT_DEPRECIATION = assetInfo.ACCT_DEPRECIATION;
                        assetSwapModel.PERIOD = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0');
                        assetSwapModel.FA_LOC_1 = assetInfo.BELONGTO_COMPANY;
                        //传入订单选择的部门
                        assetSwapModel.FA_LOC_2 = assetInfo.MANAGEMENT_COMPANY;
                        assetSwapModel.FA_LOC_3 = assetInfo.ORGANIZATION_NUM;
                        assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                        assetSwapModel.CREATE_DATE = DateTime.Now;
                        assetSwapModel.ASSET_ID = assetInfo.ASSET_ID;
                        assetSwapModel.STATUS = "N";
                        var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                            x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Abbreviation == assetInfo.BELONGTO_COMPANY).First();
                        assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                        assetSwapModel.VEHICLE_TYPE = assetInfo.DESCRIPTION;
                        assetSwapModel.MODEL_MAJOR = assetInfo.MODEL_MAJOR;
                        assetSwapModel.MODEL_MINOR = assetInfo.MODEL_MINOR;
                        assetSwapModel.PERIOD = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0');
                        assetSwapModel.PROCESS_TYPE = "NEW_ASSET";
                        assetSwapList.Add(assetSwapModel);
                    }
                    db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                    db.Insertable<Business_AssetMaintenanceInfo>(assetInfoList).ExecuteCommand();
                    db.Updateable<Business_IntangibleAssetsOrder>().UpdateColumns(x => new Business_IntangibleAssetsOrder() { ISVerify = true }).Where(x => vguids.Contains(x.VGUID))
                        .ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}