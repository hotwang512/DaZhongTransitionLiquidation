using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;
using Business_AssetOrderDetails = DaZhongTransitionLiquidation.Areas.AssetPurchase.Models.Business_AssetOrderDetails;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.FundClearing
{
    public class FundClearingController : BaseController
    {
        // GET: AssetPurchase/FundClearing
        public FundClearingController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetPurchase/PurchaseAssign
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("2dc09688-a87d-4ea8-8515-8e3548d30c7d");
            return View();
        }
        public JsonResult GetListDatas(Business_FundClearing searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_FundClearing>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                if (searchParams.AssetType == "Vehicle" || searchParams.AssetType == "Office")
                {
                    jsonResult.Rows = db.SqlQueryable<Business_FundClearing>("SELECT pa.* FROM Business_FundClearing pa INNER JOIN (select * from Business_FixedAssetsOrder where SubmitStatus = 2 and OrderType = '" + searchParams.AssetType + "' ) fao ON pa.FixedAssetsOrderVguid = fao.VGUID")//WHERE fao.SubmitStatus = 1
                        .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                        .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                        .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                    jsonResult.TotalRows = pageCount;
                }
                else if (searchParams.AssetType == "Intangible")
                {
                    jsonResult.Rows = db.SqlQueryable<Business_FundClearing>("SELECT pa.* FROM Business_FundClearing pa INNER JOIN (select * from Business_IntangibleAssetsOrder where SubmitStatus = 6 ) fao ON pa.FixedAssetsOrderVguid = fao.VGUID")//WHERE fao.SubmitStatus = 1
                        .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                        .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                        .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                    jsonResult.TotalRows = pageCount;
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCompanyData()
        {
            var list = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_SevenSection>()
                    .Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDepartmentListDatas()
        {
            var list = new List<PurchaseDepartmentModel>();
            DbBusinessDataService.Command(db =>
            {
                list = db.SqlQueryable<PurchaseDepartmentModel>(@"SELECT VGUID,Descrption
                    FROM Business_SevenSection
                    WHERE SectionVGUID = 'D63BD715-C27D-4C47-AB66-550309794D43'
                          AND AccountModeCode = '1002'
                          AND CompanyCode = '01'
                          AND Status = '1'
                          AND Code LIKE '10%'").ToList();
                foreach (var item in list)
                {
                    item.Descrption = "财务共享-" + item.Descrption;
                }
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAssignCompany(Guid Vguid)
        {
            var list = new List<Business_LiquidationDistribution>();
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_LiquidationDistribution>().Where(x => x.FundClearingVguid == Vguid).ToList();
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteAssignCompany(Guid Vguid)
        {
            var resultModel = new ResultModel<string>() {IsSuccess = false,Status = "0"};
            DbBusinessDataService.Command(db =>
            {
                var saveChanges = db.Deleteable<Business_LiquidationDistribution>(x => x.VGUID == Vguid).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateAssignCompany(Guid Vguid, int AssetNum)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var resultModel = new ResultModel<string>() {IsSuccess = false,Status = "0"};
            DbBusinessDataService.Command(db =>
            {
                var distribution = db.Queryable<Business_LiquidationDistribution>().Where(x => x.VGUID == Vguid).First();
                distribution.AssetNum = AssetNum;
                distribution.ContractAmount = distribution.PurchasePrices * AssetNum;
                distribution.ChangeDate = DateTime.Now;
                distribution.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                var saveChanges = db.Updateable<Business_LiquidationDistribution>(distribution).UpdateColumns(x => new {x.AssetNum,x.ContractAmount,x.ChangeDate,x.ChangeUser}).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddAssign(Guid FundClearingVguid,Guid CompanyVguid,string Company,int AssetNum, Guid ManageCompanyVguid, string ManageCompanyName, Guid DepartmentVguid, string Department)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var FundClearingModel = db.Queryable<Business_FundClearing>().Where(x => x.VGUID == FundClearingVguid).First();
                var liquidationDistribution = new Business_LiquidationDistribution();
                liquidationDistribution.VGUID = Guid.NewGuid();
                liquidationDistribution.FundClearingVguid = FundClearingModel.VGUID;
                liquidationDistribution.AssetsOrderVguid = FundClearingModel.FixedAssetsOrderVguid;
                liquidationDistribution.CompanyVguid = CompanyVguid;
                liquidationDistribution.Company = Company;
                liquidationDistribution.ManageCompanyVguid = ManageCompanyVguid;
                liquidationDistribution.ManageCompany = ManageCompanyName;
                liquidationDistribution.DepartmentVguid = DepartmentVguid;
                liquidationDistribution.Department = Department;
                liquidationDistribution.PurchasePrices = FundClearingModel.PurchasePrices;
                liquidationDistribution.AssetNum = AssetNum;
                liquidationDistribution.ContractAmount = FundClearingModel.PurchasePrices * AssetNum;
                liquidationDistribution.CreateDate = DateTime.Now;
                liquidationDistribution.CreateUser = cache[PubGet.GetUserKey].LoginName;
                db.Insertable<Business_LiquidationDistribution>(liquidationDistribution).ExecuteCommand();
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
            });
            return Json(resultModel);
        }
        public JsonResult SubmitAssign(Guid FundClearingVguid)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var fundClearingModel = db.Queryable<Business_FundClearing>().Where(x => x.VGUID == FundClearingVguid).First();
                    var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                        x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                    if (fundClearingModel.SubmitStatus == 0 || fundClearingModel.SubmitStatus == 2)
                    {
                        var liquidationDistributionList = db.Queryable<Business_LiquidationDistribution>()
                            .Where(x => x.FundClearingVguid == FundClearingVguid).ToList();
                        //检查数量是否匹配
                        var assetSum = liquidationDistributionList.Sum(x => x.AssetNum);
                        //var assetOrder = db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == fundClearingModel.FixedAssetsOrderVguid).First();
                        //var orderSetting = db.Queryable<Business_PurchaseOrderSetting>()
                        //    .Where(x => x.VGUID == assetOrder.PurchaseGoodsVguid).First();
                        //var assetsCategoryList = db.Queryable<Business_AssetsCategory>().ToList();
                        if (assetSum == fundClearingModel.OrderQuantity)
                        {
                            //var assetReviewList = new List<Business_AssetReview>();
                            if (fundClearingModel.AssetType == "Office")
                            {
                                //    移到清算
                                //    //无形资产和其它类资产进固定资产审核
                                //    foreach (var liquidation in liquidationDistributionList)
                                //    {
                                //        for (int i = 0; i <= liquidation.AssetNum; i++)
                                //        {
                                //            var assetReview = new Business_AssetReview();
                                //            assetReview.VGUID = Guid.NewGuid();
                                //            assetReview.OBDSTATUS = false;
                                //            var autoID = "FixedAssetID";
                                //            var no = CreateNo.GetCreateNo(db, autoID);
                                //            assetReview.ASSET_ID = no;
                                //            assetReview.GROUP_ID = assetOrder.PurchaseGoods;
                                //            //assetReview.PLATE_NUMBER = item.PlateNumber;
                                //            //assetReview.CHASSIS_NUMBER = item.EquipmentNumber;
                                //            assetReview.VEHICLE_SHORTNAME = "OBD";
                                //            assetReview.DESCRIPTION = assetOrder.AssetDescription;
                                //            //assetReview.LISENSING_DATE = item.LisensingDate;
                                //            assetReview.TAG_NUMBER = assetReview.ASSET_ID.Replace("CZ", "BG");
                                //            assetReview.PURCHASE_DATE = DateTime.Now;
                                //            assetReview.QUANTITY = 1;
                                //            assetReview.ASSET_COST = liquidation.PurchasePrices;
                                //            //资产主类次类 根据采购物品获取
                                //            assetReview.ASSET_CATEGORY_MAJOR = orderSetting.AssetCategoryMajor;
                                //            assetReview.ASSET_CATEGORY_MINOR = orderSetting.AssetCategoryMinor;
                                //            //根据主类子类从折旧方法表中获取
                                //            var assetsCategoryInfo = assetsCategoryList.First(x => x.ASSET_CATEGORY_MAJOR == assetReview.ASSET_CATEGORY_MAJOR &&
                                //                                                                   x.ASSET_CATEGORY_MINOR == assetReview.ASSET_CATEGORY_MINOR);
                                //            assetReview.LIFE_YEARS = assetsCategoryInfo.LIFE_YEARS;
                                //            assetReview.LIFE_MONTHS = assetsCategoryInfo.LIFE_MONTHS;
                                //            assetReview.AMORTIZATION_FLAG = "N";
                                //            assetReview.METHOD = assetsCategoryInfo.METHOD;
                                //            assetReview.BOOK_TYPE_CODE = assetsCategoryInfo.BOOK_TYPE_CODE;
                                //            assetReview.ASSET_COST_ACCOUNT = assetsCategoryInfo.ASSET_COST_ACCOUNT;
                                //            assetReview.ASSET_SETTLEMENT_ACCOUNT = assetsCategoryInfo.ASSET_SETTLEMENT_ACCOUNT;
                                //            assetReview.DEPRECIATION_EXPENSE_SEGMENT = assetsCategoryInfo.DEPRECIATION_EXPENSE_SEGMENT;
                                //            assetReview.ACCT_DEPRECIATION_ACCOUNT = assetsCategoryInfo.ACCT_DEPRECIATION_ACCOUNT;
                                //            assetReview.SALVAGE_PERCENT = assetsCategoryInfo.SALVAGE_PERCENT;
                                //            assetReview.ISVERIFY = false;
                                //            assetReview.OBDSTATUS = false;
                                //            assetReview.YTD_DEPRECIATION = 0;
                                //            assetReview.ACCT_DEPRECIATION = 0;
                                //            assetReview.FIXED_ASSETS_ORDERID = assetOrder.VGUID;
                                //            assetReview.CREATE_USER = cache[PubGet.GetUserKey].LoginName;
                                //            assetReview.CREATE_DATE = DateTime.Now;
                                //            assetReview.BELONGTO_COMPANY = liquidation.Company;
                                //            assetReview.MANAGEMENT_COMPANY = liquidation.ManageCompany;
                                //            assetReview.MODEL_MAJOR = "无";
                                //            assetReview.MODEL_MINOR = "无";
                                //            var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                //                x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.OrgID == assetReview.BELONGTO_COMPANY_CODE).First();
                                //            var accountMode = db.Queryable<Business_SevenSection>().Where(x =>
                                //                x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" &&
                                //                x.Code == ssModel.AccountModeCode).First().Descrption;
                                //            assetReview.EXP_ACCOUNT_SEGMENT = accountMode;
                                //            //assetReview.VEHICLE_STATE = assetInfo.VEHICLE_STATE;
                                //            //assetReview.OPERATING_STATE = assetInfo.OPERATING_STATE;
                                //            assetReview.ORGANIZATION_NUM = liquidation.Department;
                                //            assetReviewList.Add(assetReview);
                                //        }
                                //    }
                                //    db.Insertable<Business_AssetReview>(assetReviewList).ExecuteCommand();
                                //}
                                //fundClearingModel.SubmitStatus = 1;
                                //fundClearingModel.LiquidationStatus = 0;
                                //fundClearingModel.SubmitDate = DateTime.Now;
                                //fundClearingModel.SubmitUser = cache[PubGet.GetUserKey].LoginName;
                                //db.Updateable<Business_FundClearing>(fundClearingModel)
                                //    .UpdateColumns(x => new { x.SubmitDate, x.SubmitStatus,x.LiquidationStatus, x.SubmitUser }).ExecuteCommand();
                                //resultModel.IsSuccess = true;
                                //resultModel.Status = "1";
                            }
                            fundClearingModel.SubmitStatus = 1;
                            fundClearingModel.LiquidationStatus = 0;
                            fundClearingModel.SubmitDate = DateTime.Now;
                            fundClearingModel.SubmitUser = cache[PubGet.GetUserKey].LoginName;
                            db.Updateable<Business_FundClearing>(fundClearingModel)
                                .UpdateColumns(x => new { x.SubmitDate, x.SubmitStatus,x.LiquidationStatus, x.SubmitUser }).ExecuteCommand();
                            resultModel.IsSuccess = true;
                            resultModel.Status = "1";
                        }
                        else
                        {
                            resultModel.Status = "2";
                            resultModel.ResultInfo = "数量不一致";
                        }
                    }
                    else
                    {
                        resultModel.Status = "2";
                        resultModel.ResultInfo = "不能重复提交!";
                    }
                });
            });
            return Json(resultModel);
        }
    }
}