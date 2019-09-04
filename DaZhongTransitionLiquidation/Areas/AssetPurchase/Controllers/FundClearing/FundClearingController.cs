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
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetListDatas(Business_FundClearing searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_FundClearing>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.SqlQueryable<Business_FundClearing>("SELECT pa.* FROM Business_FundClearing pa INNER JOIN (select * from Business_FixedAssetsOrder where SubmitStatus = 2 and PurchaseGoods = '出租车') fao ON pa.FixedAssetsOrderVguid = fao.VGUID")//WHERE fao.SubmitStatus = 1
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
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
        public JsonResult AddAssign(Guid FundClearingVguid,Guid CompanyVguid,string Company,int AssetNum)
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
        public JsonResult SubmitAssign(Guid FundClearingVguid)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var fundClearingModel = db.Queryable<Business_FundClearing>().Where(x => x.VGUID == FundClearingVguid).First();
                if (fundClearingModel.SubmitStatus == 0 || fundClearingModel.SubmitStatus == 2)
                {
                    //检查数量是否匹配
                    var assetSum = db.Queryable<Business_LiquidationDistribution>()
                        .Where(x => x.FundClearingVguid == FundClearingVguid).Sum(x => x.AssetNum);
                    if (assetSum == fundClearingModel.OrderQuantity)
                    {
                        //供应商信息
                        var bankInfoList = db.SqlQueryable<v_Business_CustomerBankInfo>(
                                @"select a.*,b.Isable,b.OrderVGUID from Business_CustomerBankInfo as a 
                                                left join Business_CustomerBankSetting as b on a.VGUID = b.CustomerID
                                                left join v_Business_BusinessTypeSet as c on c.VGUID = b.OrderVGUID where b.Isable = '1'")
                            .OrderBy(i => i.CreateTime, OrderByType.Desc).ToList();
                        //生成支付订单
                        var assets = db.Queryable<Business_LiquidationDistribution>()
                            .Where(x => x.FundClearingVguid == FundClearingVguid).ToList();
                        foreach (var asset in assets)
                        {
                            var assetOrder = db.Queryable<Business_FixedAssetsOrder>()
                                .Where(x => x.VGUID == asset.AssetsOrderVguid).First();
                            if (assetOrder.PayCompany != asset.Company)
                            {
                                var fundClearingOrder = new Business_FundClearingOrder();
                                fundClearingOrder.VGUID = Guid.NewGuid();
                                fundClearingOrder.OrderNumber = assetOrder.OrderNumber;
                                fundClearingOrder.FixedAssetsOrderVguid = assetOrder.VGUID;
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
                                    var companyInfo = companylist.First(x => x.OrderVGUID == OrderVguid && x.CompanyName == asset.Company);
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
                                db.Insertable<Business_FundClearingOrder>(fundClearingOrder).ExecuteCommand();
                            }
                        }
                        fundClearingModel.SubmitStatus = 1;
                        fundClearingModel.SubmitDate = DateTime.Now;
                        fundClearingModel.SubmitUser = cache[PubGet.GetUserKey].LoginName;
                        db.Updateable<Business_FundClearing>(fundClearingModel)
                            .UpdateColumns(x => new { x.SubmitDate, x.SubmitStatus, x.SubmitUser }).ExecuteCommand();
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
            return Json(resultModel);
        }
    }
}