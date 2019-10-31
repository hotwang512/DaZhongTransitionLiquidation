using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
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

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.FundClearing
{
    public class VehicleLiquidationReviewController : BaseController
    {
        // GET: AssetPurchase/FundClearing
        public VehicleLiquidationReviewController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetPurchase/PurchaseAssign
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("2dc09688-a87d-4ea8-8515-8e3548d30c7d");
            return View();
        }
        public JsonResult RejectLiquidation(Guid FundClearingVguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var fundClearingModel = db.Queryable<Business_FundClearing>().Where(x => x.VGUID == FundClearingVguid).First();
                    if (fundClearingModel.LiquidationStatus == 0)
                    {
                        fundClearingModel.SubmitStatus = 2;//驳回
                        fundClearingModel.LiquidationStatus = 2;//驳回
                        db.Updateable<Business_FundClearing>(fundClearingModel).ExecuteCommand();
                        resultModel.IsSuccess = true;
                        resultModel.Status = "1";
                    }
                    else
                    {
                        resultModel.IsSuccess = false;
                        resultModel.Status = "2";
                        resultModel.ResultInfo = "清算已提交不允许退回！";
                    }
                });
            });
            return Json(resultModel);
        }
        public JsonResult GetListDatas(Business_FundClearing searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_FundClearing>();

            DbBusinessDataService.Command(db =>
            {
                var SubmitStatus = 2;
                if (searchParams.AssetType == "Intangible")
                {
                    SubmitStatus = 6;
                    int pageCount = 0;
                    para.pagenum = para.pagenum + 1;
                    jsonResult.Rows = db.SqlQueryable<Business_FundClearing>("SELECT pa.* FROM Business_FundClearing pa INNER JOIN (select * from Business_IntangibleAssetsOrder where SubmitStatus = " + SubmitStatus + ") fao ON pa.FixedAssetsOrderVguid = fao.VGUID")//WHERE fao.SubmitStatus = 1
                        .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                        .WhereIF(searchParams.LiquidationStatus != -1, i => i.LiquidationStatus == searchParams.LiquidationStatus)
                        .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                    jsonResult.TotalRows = pageCount;
                }
                else
                {
                    int pageCount = 0;
                    para.pagenum = para.pagenum + 1;
                    jsonResult.Rows = db.SqlQueryable<Business_FundClearing>("SELECT pa.* FROM Business_FundClearing pa INNER JOIN (select * from Business_FixedAssetsOrder where SubmitStatus = " + SubmitStatus + " and OrderType = '" + searchParams.AssetType + "') fao ON pa.FixedAssetsOrderVguid = fao.VGUID")//WHERE fao.SubmitStatus = 1
                        .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                        .WhereIF(searchParams.LiquidationStatus != -1, i => i.LiquidationStatus == searchParams.LiquidationStatus)
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

        public JsonResult GetAssignCompany(Guid Vguid)
        {
            var list = new List<Business_LiquidationDistribution>();
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_LiquidationDistribution>().Where(x => x.FundClearingVguid == Vguid).ToList();
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddAssign(Guid FundClearingVguid, Guid CompanyVguid, string Company, int AssetNum)
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
        public JsonResult SubmitAssign(Guid FundClearingVguid,string OrderType)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var fundClearingModel = db.Queryable<Business_FundClearing>().Where(x => x.VGUID == FundClearingVguid).First();
                    if (fundClearingModel.LiquidationStatus == 0)
                    {
                        //供应商信息
                        var bankInfoList = db.SqlQueryable<v_Business_CustomerBankInfo>(
                                @"select a.*,b.Isable,b.OrderVGUID from Business_CustomerBankInfo as a 
                                                left join Business_CustomerBankSetting as b on a.VGUID = b.CustomerID
                                                left join v_Business_BusinessTypeSet as c on c.VGUID = b.OrderVGUID where b.Isable = '1'")
                            .OrderBy(i => i.CreateTime, OrderByType.Desc).ToList();
                        //生成支付订单
                        var assets = db.Queryable<Business_LiquidationDistribution>()
                            .Where(x => x.FundClearingVguid == FundClearingVguid && x.AssetNum != 0).ToList();
                        var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                            x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                        var fundClearingOrderList = new List<Business_FixedAssetsOrder>();
                        var fundClearingIntangibleOrderList = new List<Business_IntangibleAssetsOrder>();
                        var assetReviewList = new List<Business_AssetReview>();
                        if (OrderType == "Vehicle" || OrderType == "Office")
                        {
                            var assetOrder = db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == fundClearingModel.FixedAssetsOrderVguid).First();
                            var orderSetting = db.Queryable<Business_PurchaseOrderSetting>()
                                .Where(x => x.VGUID == assetOrder.PurchaseGoodsVguid).First();
                            var assetsCategoryList = db.Queryable<Business_AssetsCategory>().ToList();
                            foreach (var asset in assets)
                            {
                                var company = ssList.Where(x => x.Abbreviation == asset.Company).First().Descrption;
                                //var assetOrder = db.Queryable<Business_FixedAssetsOrder>()
                                //.Where(x => x.VGUID == asset.AssetsOrderVguid).First();
                                if (assetOrder.PayCompany != company)
                                {
                                    var fundClearingOrder = new Business_FixedAssetsOrder();
                                    fundClearingOrder.VGUID = Guid.NewGuid();
                                    var autoID = "FixedAssetsOrder";
                                    var no = CreateNo.GetCreateNo(db, autoID);
                                    fundClearingOrder.OrderNumber = no;
                                    fundClearingOrder.OrderFromVguid = assetOrder.VGUID;
                                    fundClearingOrder.PurchaseDepartmentIDs = assetOrder.PurchaseDepartmentIDs;
                                    fundClearingOrder.PurchaseGoods = assetOrder.PurchaseGoods;
                                    fundClearingOrder.PurchaseGoodsVguid = assetOrder.PurchaseGoodsVguid;
                                    fundClearingOrder.OrderQuantity = asset.AssetNum;
                                    fundClearingOrder.PurchasePrices = assetOrder.PurchasePrices;
                                    fundClearingOrder.ContractAmount = asset.AssetNum * assetOrder.PurchasePrices;
                                    fundClearingOrder.AssetDescription = assetOrder.AssetDescription;
                                    fundClearingOrder.PaymentDate = assetOrder.PaymentDate;
                                    fundClearingOrder.ContractName = assetOrder.ContractName;
                                    fundClearingOrder.ContractFilePath = assetOrder.ContractFilePath;
                                    fundClearingOrder.PayType = assetOrder.PayType;
                                    //根据付款项目填充供应商信息和付款信息
                                    var BusinessSubItem = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.VGUID == assetOrder.PurchaseGoodsVguid).First().BusinessSubItem;
                                    var OrderVguid = db.Queryable<v_Business_BusinessTypeSet>().Where(x => x.BusinessSubItem1 == BusinessSubItem).First().VGUID.ToString();
                                    if (bankInfoList.Any(x => x.OrderVGUID == OrderVguid) && bankInfoList.Count(x => x.OrderVGUID == OrderVguid) == 1)
                                    {
                                        var bankInfo = bankInfoList.First(x => x.OrderVGUID == OrderVguid);
                                        fundClearingOrder.PaymentInformationVguid = bankInfo.VGUID;
                                        fundClearingOrder.PaymentInformation = bankInfo.BankAccountName;
                                        fundClearingOrder.SupplierBankAccountName = bankInfo.BankAccountName;
                                        fundClearingOrder.SupplierBankAccount = bankInfo.BankAccount;
                                        fundClearingOrder.SupplierBankNo = bankInfo.BankNo;
                                        fundClearingOrder.SupplierBank = bankInfo.Bank;
                                    }
                                    //付款信息
                                    //var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
                                    var companylist = db.Queryable<Business_UserCompanySetDetail>().Where(x => x.OrderVGUID == OrderVguid && x.Isable)
                                        .OrderBy(i => i.CompanyCode).ToList();
                                    if (companylist.Any(x => x.OrderVGUID == OrderVguid))
                                    {
                                        var companyInfo = companylist.First(x => x.OrderVGUID == OrderVguid && x.CompanyName == company);
                                        fundClearingOrder.PayCompanyVguid = companyInfo.VGUID;
                                        fundClearingOrder.PayCompany = companyInfo.PayBankAccountName;
                                        fundClearingOrder.CompanyBankName = companyInfo.PayBank;
                                        fundClearingOrder.CompanyBankAccountName = companyInfo.PayBankAccountName;
                                        fundClearingOrder.CompanyBankAccount = companyInfo.PayAccount;
                                        fundClearingOrder.AccountType = companyInfo.AccountType;
                                    }
                                    fundClearingOrder.CreateDate = DateTime.Now;
                                    fundClearingOrder.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                    fundClearingOrder.SubmitStatus = 0;
                                    fundClearingOrder.OrderFrom = "分配清算";
                                    fundClearingOrder.OrderType = OrderType;
                                    fundClearingOrderList.Add(fundClearingOrder);
                                }
                                if (OrderType == "Office")
                                {
                                    for (int i = 0; i <= asset.AssetNum; i++)
                                    {
                                        var assetReview = new Business_AssetReview();
                                        assetReview.VGUID = Guid.NewGuid();
                                        assetReview.OBDSTATUS = false;
                                        var autoID = "FixedAssetID";
                                        var no = CreateNo.GetCreateNo(db, autoID);
                                        assetReview.ASSET_ID = no;
                                        assetReview.GROUP_ID = assetOrder.PurchaseGoods;
                                        //assetReview.PLATE_NUMBER = item.PlateNumber;
                                        //assetReview.CHASSIS_NUMBER = item.EquipmentNumber;
                                        assetReview.VEHICLE_SHORTNAME = "OBD";
                                        assetReview.DESCRIPTION = assetOrder.AssetDescription;
                                        //assetReview.LISENSING_DATE = item.LisensingDate;
                                        assetReview.TAG_NUMBER = assetReview.ASSET_ID.Replace("CZ", "BG");
                                        assetReview.PURCHASE_DATE = DateTime.Now;
                                        assetReview.QUANTITY = 1;
                                        assetReview.ASSET_COST = asset.PurchasePrices;
                                        //资产主类次类 根据采购物品获取
                                        assetReview.ASSET_CATEGORY_MAJOR = orderSetting.AssetCategoryMajor;
                                        assetReview.ASSET_CATEGORY_MINOR = orderSetting.AssetCategoryMinor;
                                        //根据主类子类从折旧方法表中获取
                                        var assetsCategoryInfo = assetsCategoryList.First(x => x.ASSET_CATEGORY_MAJOR == assetReview.ASSET_CATEGORY_MAJOR &&
                                                                                               x.ASSET_CATEGORY_MINOR == assetReview.ASSET_CATEGORY_MINOR);
                                        assetReview.LIFE_YEARS = assetsCategoryInfo.LIFE_YEARS;
                                        assetReview.LIFE_MONTHS = assetsCategoryInfo.LIFE_MONTHS;
                                        assetReview.AMORTIZATION_FLAG = "N";
                                        assetReview.METHOD = assetsCategoryInfo.METHOD;
                                        assetReview.BOOK_TYPE_CODE = assetsCategoryInfo.BOOK_TYPE_CODE;
                                        assetReview.ASSET_COST_ACCOUNT = assetsCategoryInfo.ASSET_COST_ACCOUNT;
                                        assetReview.ASSET_SETTLEMENT_ACCOUNT = assetsCategoryInfo.ASSET_SETTLEMENT_ACCOUNT;
                                        assetReview.DEPRECIATION_EXPENSE_SEGMENT = assetsCategoryInfo.DEPRECIATION_EXPENSE_SEGMENT;
                                        assetReview.ACCT_DEPRECIATION_ACCOUNT = assetsCategoryInfo.ACCT_DEPRECIATION_ACCOUNT;
                                        assetReview.SALVAGE_PERCENT = assetsCategoryInfo.SALVAGE_PERCENT;
                                        assetReview.ISVERIFY = false;
                                        assetReview.OBDSTATUS = false;
                                        assetReview.YTD_DEPRECIATION = 0;
                                        assetReview.ACCT_DEPRECIATION = 0;
                                        assetReview.FIXED_ASSETS_ORDERID = assetOrder.VGUID;
                                        assetReview.CREATE_USER = cache[PubGet.GetUserKey].LoginName;
                                        assetReview.CREATE_DATE = DateTime.Now;
                                        assetReview.BELONGTO_COMPANY = asset.Company;
                                        assetReview.MANAGEMENT_COMPANY = asset.ManageCompany;
                                        assetReview.MODEL_MAJOR = "无";
                                        assetReview.MODEL_MINOR = "无";
                                        var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                                            x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.OrgID == assetReview.BELONGTO_COMPANY_CODE).First();
                                        var accountMode = db.Queryable<Business_SevenSection>().Where(x =>
                                            x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" &&
                                            x.Code == ssModel.AccountModeCode).First().Descrption;
                                        assetReview.EXP_ACCOUNT_SEGMENT = accountMode;
                                        //assetReview.VEHICLE_STATE = assetInfo.VEHICLE_STATE;
                                        //assetReview.OPERATING_STATE = assetInfo.OPERATING_STATE;
                                        assetReview.ORGANIZATION_NUM = asset.Department;
                                        assetReviewList.Add(assetReview);
                                    }
                                }
                            }
                            foreach (var fundClearingOrder in fundClearingOrderList)
                            {
                                //请求清算平台、待付款请求生成支付凭证接口
                                var pendingPaymentmodel = new PendingPaymentModel();
                                //统计附件信息
                                var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>().Where(x => x.AssetOrderVGUID == fundClearingOrder.OrderFromVguid).ToList();
                                pendingPaymentmodel.PaymentReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "付款凭证").ToList());
                                pendingPaymentmodel.InvoiceReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList());
                                pendingPaymentmodel.ApprovalReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "OA审批单").ToList());
                                pendingPaymentmodel.Contract = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "合同").ToList());
                                pendingPaymentmodel.DetailList = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "清单、清册").ToList());
                                pendingPaymentmodel.OtherReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "其他").ToList());
                                var goodsData = db.Queryable<Business_PurchaseOrderSetting>()
                                    .Where(x => x.VGUID == fundClearingOrder.PurchaseGoodsVguid).First();
                                var orderListData = db.Queryable<v_Business_BusinessTypeSet>()
                                    .Where(x => x.BusinessSubItem1 == goodsData.BusinessSubItem).First();
                                pendingPaymentmodel.ServiceCategory = orderListData.BusinessProject;
                                pendingPaymentmodel.BusinessProject = orderListData.BusinessSubItem1.Split("|")[0] + "|"
                                                                      + orderListData.BusinessSubItem1.Substring(orderListData.BusinessSubItem1.LastIndexOf("|") + 1, orderListData.BusinessSubItem1.Length - orderListData.BusinessSubItem1.LastIndexOf("|") - 1);
                                //根据供应商账号找到供应商类别
                                pendingPaymentmodel.PaymentCompany = db.Queryable<Business_CustomerBankInfo>()
                                    .Where(x => x.BankAccount == fundClearingOrder.SupplierBankAccount).First().CompanyOrPerson; ;
                                pendingPaymentmodel.CollectBankAccountName = fundClearingOrder.SupplierBankAccountName;
                                pendingPaymentmodel.CollectBankAccouont = fundClearingOrder.SupplierBankAccount;
                                pendingPaymentmodel.CollectBankName = fundClearingOrder.SupplierBank;
                                pendingPaymentmodel.CollectBankNo = fundClearingOrder.SupplierBankNo;
                                pendingPaymentmodel.PaymentMethod = fundClearingOrder.PayType;
                                pendingPaymentmodel.IdentityToken = cache[PubGet.GetUserKey].Token;
                                pendingPaymentmodel.FunctionSiteId = "61";
                                pendingPaymentmodel.OperatorIP = GetSystemInfo.GetClientLocalIPv4Address();
                                pendingPaymentmodel.invoiceNumber = assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count().ToString();
                                pendingPaymentmodel.numberOfAttachments = (assetAttachmentList.Count() - assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count()).ToString();
                                pendingPaymentmodel.Amount = fundClearingOrder.ContractAmount.ToString();
                                pendingPaymentmodel.Summary = fundClearingOrder.AssetDescription;
                                pendingPaymentmodel.AccountSetCode = cache[PubGet.GetUserKey].AccountModeCode + "|" + cache[PubGet.GetUserKey].CompanyCode;
                                var apiReault = PendingPaymentApi(pendingPaymentmodel);
                                var pendingRedult = apiReault.JsonToModel<JsonResultModelApi<Api_PendingPayment>>();
                                if (pendingRedult.success)
                                {
                                    fundClearingOrder.PaymentVoucherVguid = pendingRedult.data.vguid;
                                    fundClearingOrder.PaymentVoucherUrl = pendingRedult.data.url;
                                    //db.Updateable<Business_FixedAssetsOrder>(orderModel).UpdateColumns(x => new { x.PaymentVoucherUrl, x.PaymentVoucherVguid }).ExecuteCommand();
                                    fundClearingOrder.SubmitStatus = FixedAssetsSubmitStatusEnum.UnPay.TryToInt();
                                    fundClearingOrder.SubmitDate = DateTime.Now;
                                    fundClearingOrder.SubmitUser = cache[PubGet.GetUserKey].LoginName;
                                    //db.Updateable<Business_FixedAssetsOrder>(fundClearingOrder).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                                    //resultModel.ResultInfo = pendingRedult.data.url;
                                }
                                else
                                {
                                    LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                                }
                            }
                            db.Insertable<Business_AssetReview>(assetReviewList).ExecuteCommand();
                            db.Insertable<Business_FixedAssetsOrder>(fundClearingOrderList).ExecuteCommand();
                            fundClearingModel.LiquidationStatus = 1;
                            fundClearingModel.SubmitDate = DateTime.Now;
                            fundClearingModel.SubmitUser = cache[PubGet.GetUserKey].LoginName;
                            db.Updateable<Business_FundClearing>(fundClearingModel)
                                .UpdateColumns(x => new { x.SubmitDate, x.LiquidationStatus, x.SubmitUser }).ExecuteCommand();
                            resultModel.IsSuccess = true;
                            resultModel.Status = "1";
                        }
                        else
                        {
                            foreach (var asset in assets)
                            {
                                var company = ssList.Where(x => x.Abbreviation == asset.Company).First().Descrption;
                                var assetOrder = db.Queryable<Business_IntangibleAssetsOrder>().Where(x => x.VGUID == asset.AssetsOrderVguid).First();
                                if (assetOrder.PayCompany != company)
                                {
                                    var fundClearingOrder = new Business_IntangibleAssetsOrder();
                                    fundClearingOrder.VGUID = Guid.NewGuid();
                                    var autoID = "IntAssetsOrder";
                                    var no = CreateNo.GetCreateNo(db, autoID);
                                    fundClearingOrder.OrderNumber = no;
                                    fundClearingOrder.OrderFromVguid = assetOrder.VGUID;
                                    fundClearingOrder.PurchaseDepartmentIDs = assetOrder.PurchaseDepartmentIDs;
                                    fundClearingOrder.PurchaseGoods = assetOrder.PurchaseGoods;
                                    fundClearingOrder.PurchaseGoodsVguid = assetOrder.PurchaseGoodsVguid;
                                    fundClearingOrder.SumPayment = assetOrder.SumPayment;
                                    fundClearingOrder.FirstPayment = assetOrder.FirstPayment;
                                    fundClearingOrder.InterimPayment = assetOrder.InterimPayment;
                                    fundClearingOrder.TailPayment = assetOrder.TailPayment;
                                    fundClearingOrder.AssetDescription = assetOrder.AssetDescription;
                                    fundClearingOrder.ContractName = assetOrder.ContractName;
                                    fundClearingOrder.ContractFilePath = assetOrder.ContractFilePath;
                                    fundClearingOrder.PayType = assetOrder.PayType;
                                    //根据付款项目填充供应商信息和付款信息
                                    var BusinessSubItem = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.VGUID == assetOrder.PurchaseGoodsVguid).First().BusinessSubItem;
                                    var OrderVguid = db.Queryable<v_Business_BusinessTypeSet>().Where(x => x.BusinessSubItem1 == BusinessSubItem).First().VGUID.ToString();
                                    if (bankInfoList.Any(x => x.OrderVGUID == OrderVguid) && bankInfoList.Count(x => x.OrderVGUID == OrderVguid) == 1)
                                    {
                                        var bankInfo = bankInfoList.First(x => x.OrderVGUID == OrderVguid);
                                        fundClearingOrder.PaymentInformationVguid = bankInfo.VGUID;
                                        fundClearingOrder.PaymentInformation = bankInfo.BankAccountName;
                                        fundClearingOrder.SupplierBankAccountName = bankInfo.BankAccountName;
                                        fundClearingOrder.SupplierBankAccount = bankInfo.BankAccount;
                                        fundClearingOrder.SupplierBankNo = bankInfo.BankNo;
                                        fundClearingOrder.SupplierBank = bankInfo.Bank;
                                    }
                                    //付款信息
                                    //var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
                                    var companylist = db.Queryable<Business_UserCompanySetDetail>().Where(x => x.OrderVGUID == OrderVguid && x.Isable)
                                        .OrderBy(i => i.CompanyCode).ToList();
                                    if (companylist.Any(x => x.OrderVGUID == OrderVguid))
                                    {
                                        var companyInfo = companylist.First(x => x.OrderVGUID == OrderVguid && x.CompanyName == company);
                                        fundClearingOrder.PayCompanyVguid = companyInfo.VGUID;
                                        fundClearingOrder.PayCompany = companyInfo.PayBankAccountName;
                                        fundClearingOrder.CompanyBankName = companyInfo.PayBank;
                                        fundClearingOrder.CompanyBankAccountName = companyInfo.PayBankAccountName;
                                        fundClearingOrder.CompanyBankAccount = companyInfo.PayAccount;
                                        fundClearingOrder.AccountType = companyInfo.AccountType;
                                    }
                                    fundClearingOrder.CreateDate = DateTime.Now;
                                    fundClearingOrder.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                    fundClearingOrder.SubmitStatus = 0;
                                    fundClearingOrder.OrderFrom = "分配清算";
                                    fundClearingIntangibleOrderList.Add(fundClearingOrder);
                                }
                            }
                            foreach (var model in fundClearingIntangibleOrderList)
                            {
                                //请求清算平台、待付款请求生成支付凭证接口
                                var pendingPaymentmodel = new PendingPaymentModel();
                                //var model = db.Queryable<Business_IntangibleAssetsOrder>().Where(c => c.VGUID == fundClearingOrder.VGUID).First();
                                if (model.SubmitStatus == IntangibleAssetsSubmitStatusEnum.FirstPaymentUnSubmit.TryToInt())
                                {
                                    model.SubmitStatus = IntangibleAssetsSubmitStatusEnum.FirstPaymentUnPay.TryToInt();
                                    pendingPaymentmodel.Amount = model.FirstPayment.ToString();
                                }
                                else if (model.SubmitStatus == IntangibleAssetsSubmitStatusEnum.InterimPaymentUnSubmit.TryToInt())
                                {
                                    model.SubmitStatus = IntangibleAssetsSubmitStatusEnum.InterimPaymentUnPay.TryToInt();
                                    pendingPaymentmodel.Amount = model.InterimPayment.ToString();
                                }
                                else if (model.SubmitStatus == IntangibleAssetsSubmitStatusEnum.TailPaymentUnSubmit.TryToInt())
                                {
                                    model.SubmitStatus = IntangibleAssetsSubmitStatusEnum.TailPaymentUnPay.TryToInt();
                                    pendingPaymentmodel.Amount = model.TailPayment.ToString();
                                }
                                var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>().Where(x => x.AssetOrderVGUID == model.OrderFromVguid).ToList();
                                pendingPaymentmodel.PaymentReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "付款凭证").ToList());
                                pendingPaymentmodel.InvoiceReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList());
                                pendingPaymentmodel.ApprovalReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "OA审批单").ToList());
                                pendingPaymentmodel.Contract = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "合同").ToList());
                                pendingPaymentmodel.DetailList = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "清单、清册").ToList());
                                pendingPaymentmodel.OtherReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "其他").ToList());

                                pendingPaymentmodel.IdentityToken = cache[PubGet.GetUserKey].Token;
                                pendingPaymentmodel.FunctionSiteId = "61";
                                pendingPaymentmodel.OperatorIP = GetSystemInfo.GetClientLocalIPv4Address();
                                var goodsData = db.Queryable<Business_PurchaseOrderSetting>()
                                    .Where(x => x.VGUID == model.PurchaseGoodsVguid).First();

                                var orderListData = db.Queryable<v_Business_BusinessTypeSet>()
                                    .Where(x => x.BusinessSubItem1 == goodsData.BusinessSubItem).First();

                                pendingPaymentmodel.ServiceCategory = orderListData.BusinessProject;
                                pendingPaymentmodel.BusinessProject = orderListData.BusinessSubItem1.Split("|")[0] + "|" + orderListData.BusinessSubItem1.Substring(orderListData.BusinessSubItem1.LastIndexOf("|") + 1, orderListData.BusinessSubItem1.Length - orderListData.BusinessSubItem1.LastIndexOf("|") - 1);
                                //根据供应商账号找到供应商类别
                                pendingPaymentmodel.PaymentCompany = db.Queryable<Business_CustomerBankInfo>()
                                    .Where(x => x.BankAccount == model.SupplierBankAccount).First().CompanyOrPerson; ;
                                pendingPaymentmodel.CollectBankAccountName = model.SupplierBankAccountName;
                                pendingPaymentmodel.CollectBankAccouont = model.SupplierBankAccount;
                                pendingPaymentmodel.CollectBankName = model.SupplierBank;
                                pendingPaymentmodel.CollectBankNo = model.SupplierBankNo;
                                pendingPaymentmodel.PaymentMethod = model.PayType;
                                pendingPaymentmodel.AccountSetCode = cache[PubGet.GetUserKey].AccountModeCode + "|" + cache[PubGet.GetUserKey].CompanyCode;
                                pendingPaymentmodel.invoiceNumber = assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count().ToString();
                                pendingPaymentmodel.numberOfAttachments = (assetAttachmentList.Count() - assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count()).ToString();
                                //pendingPaymentmodel.Amount = model.SumPayment.ToString();
                                pendingPaymentmodel.Summary = model.AssetDescription;

                                var apiReault = PendingPaymentApi(pendingPaymentmodel);
                                var pendingRedult = apiReault.JsonToModel<JsonResultModelApi<Api_PendingPayment>>();
                                if (pendingRedult.success)
                                {
                                    model.PaymentVoucherVguid = pendingRedult.data.vguid;
                                    model.PaymentVoucherUrl = pendingRedult.data.url;
                                    model.SubmitStatus = 1;
                                    //db.Updateable<Business_IntangibleAssetsOrder>(orderModel).UpdateColumns(x => new { x.PaymentVoucherUrl, x.PaymentVoucherVguid, x.SubmitStatus }).ExecuteCommand();
                                    resultModel.ResultInfo = pendingRedult.data.url;
                                    resultModel.IsSuccess = true;
                                    resultModel.Status = "1";
                                }
                                else
                                {
                                    LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                                }
                            }
                            db.Insertable<Business_IntangibleAssetsOrder>(fundClearingIntangibleOrderList).ExecuteCommand();
                            fundClearingModel.LiquidationStatus = 1;
                            fundClearingModel.SubmitDate = DateTime.Now;
                            fundClearingModel.SubmitUser = cache[PubGet.GetUserKey].LoginName;
                            db.Updateable<Business_FundClearing>(fundClearingModel)
                                .UpdateColumns(x => new { x.SubmitDate, x.LiquidationStatus, x.SubmitUser }).ExecuteCommand();
                            resultModel.IsSuccess = true;
                            resultModel.Status = "1";
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
        public string JoinStr(List<Business_AssetAttachmentList> list)
        {
            var strArr = "";
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    strArr = strArr + item.Attachment + ",";
                }
                strArr = strArr.Substring(0, strArr.Length - 1);
                return strArr;
            }
            else
            {
                return strArr;
            }
        }
        public string PendingPaymentApi(PendingPaymentModel model)
        {
            var url = ConfigSugar.GetAppString("PendingPaymentUrl");
            var data = "{" +
                       "\"IdentityToken\":\"{IdentityToken}\",".Replace("{IdentityToken}", model.IdentityToken) +
                       "\"FunctionSiteId\":\"{FunctionSiteId}\",".Replace("{FunctionSiteId}", "61") +
                       "\"OperatorIP\":\"{OperatorIP}\",".Replace("{OperatorIP}", GetSystemInfo.GetClientLocalIPv4Address()) +
                       "\"AccountSetCode\":\"{AccountSetCode}\",".Replace("{AccountSetCode}", model.AccountSetCode) +
                       "\"ServiceCategory\":\"{ServiceCategory}\",".Replace("{ServiceCategory}", model.ServiceCategory) +
                       "\"BusinessProject\":\"{BusinessProject}\",".Replace("{BusinessProject}", model.BusinessProject) +
                       "\"PaymentCompany\":\"{PaymentCompany}\",".Replace("{PaymentCompany}", model.PaymentCompany) +
                       "\"CollectBankAccountName\":\"{CollectBankAccountName}\",".Replace("{CollectBankAccountName}", model.CollectBankAccountName) +
                       "\"CollectBankAccouont\":\"{CollectBankAccouont}\",".Replace("{CollectBankAccouont}", model.CollectBankAccouont) +
                       "\"CollectBankName\":\"{CollectBankName}\",".Replace("{CollectBankName}", model.CollectBankName) +
                       "\"CollectBankNo\":\"{CollectBankNo}\",".Replace("{CollectBankNo}", model.CollectBankNo) +
                       "\"PaymentMethod\":\"{PaymentMethod}\",".Replace("{PaymentMethod}", model.PaymentMethod) +
                       "\"invoiceNumber\":\"{invoiceNumber}\",".Replace("{invoiceNumber}", model.invoiceNumber) +
                       "\"numberOfAttachments\":\"{numberOfAttachments}\",".Replace("{numberOfAttachments}", model.numberOfAttachments) +
                       "\"Amount\":\"{Amount}\",".Replace("{Amount}", model.Amount) +
                       "\"Summary\":\"{Summary}\",".Replace("{Summary}", model.Summary);
            if (model.PaymentReceipt != "")
            {
                data += "\"PaymentReceipt\":\"{PaymentReceipt}\",".Replace("{PaymentReceipt}", model.PaymentReceipt);
            }
            if (model.InvoiceReceipt != "")
            {
                data += "\"InvoiceReceipt\":\"{InvoiceReceipt}\",".Replace("{InvoiceReceipt}", model.InvoiceReceipt);
            }
            if (model.ApprovalReceipt != "")
            {
                data += "\"ApprovalReceipt\":\"{ApprovalReceipt}\",".Replace("{ApprovalReceipt}", model.ApprovalReceipt);
            }
            if (model.Contract != "")
            {
                data += "\"Contract\":\"{Contract}\",".Replace("{Contract}", model.Contract);
            }
            if (model.DetailList != "")
            {
                data += "\"DetailList\":\"{DetailList}\",".Replace("{DetailList}", model.DetailList);
            }
            if (model.OtherReceipt != "")
            {
                data += "\"OtherReceipt\":\"{OtherReceipt}\"".Replace("{OtherReceipt}", model.OtherReceipt);
            }

            data = data.Substring(0, data.Length - 1);
            data = data + "}";
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