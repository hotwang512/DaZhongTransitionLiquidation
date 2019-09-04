using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using DaZhongTransitionLiquidation.Models;


namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.FixedAssetsOrder
{
    public class FixedAssetsOrderController : BaseController
    {
        // GET: AssetManagement/FixedAssetsOrder
        public FixedAssetsOrderController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("0c8cd244-a13c-41ff-9b5e-65bf21cd35f6");
            return View();
        }
        public JsonResult GetFixedAssetsOrderListDatas(Business_FixedAssetsOrder searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_FixedAssetsOrder>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_FixedAssetsOrder>()
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .WhereIF(searchParams.OSNO != null, i => i.OSNO.Contains(searchParams.OSNO))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteFixedAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //查看是否有提交的订单，如果有提示提交后不可以删除
                var isAnySubmited = db.Queryable<Business_FixedAssetsOrder>().Any(c => vguids.Contains(c.VGUID) && (c.SubmitStatus == FixedAssetsSubmitStatusEnum.Submited.TryToInt() || c.SubmitStatus == FixedAssetsSubmitStatusEnum.UnPay.TryToInt()));
                if (isAnySubmited)
                {
                    resultModel.ResultInfo = "存在已提交的订单，订单提交后不允许删除";
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                }
                else
                {
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_FixedAssetsOrder>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult SubmitFixedAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    foreach (var vguid in vguids)
                    {
                        var model = db.Queryable<Business_FixedAssetsOrder>().Where(c => c.VGUID == vguid).First();
                        if (model.SubmitStatus == FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt())
                        {
                            //请求清算平台、待付款请求生成支付凭证接口
                            var pendingPaymentmodel = new PendingPaymentModel();
                            //统计附件信息
                            var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>().Where(x => x.AssetOrderVGUID == vguid).ToList();
                            pendingPaymentmodel.PaymentReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "付款凭证").ToList());
                            pendingPaymentmodel.InvoiceReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList());
                            pendingPaymentmodel.ApprovalReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "OA审批单").ToList());
                            pendingPaymentmodel.Contract = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "合同").ToList());
                            pendingPaymentmodel.DetailList = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "清单、清册").ToList());
                            pendingPaymentmodel.OtherReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "其他").ToList());
                            var goodsData = db.Queryable<Business_PurchaseOrderSetting>()
                                .Where(x => x.VGUID == model.PurchaseGoodsVguid).First();
                            var orderListData = db.Queryable<v_Business_BusinessTypeSet>()
                                .Where(x => x.BusinessSubItem1 == goodsData.BusinessSubItem).First();
                            pendingPaymentmodel.ServiceCategory = orderListData.BusinessProject;
                            pendingPaymentmodel.BusinessProject = orderListData.BusinessSubItem1.Split("|")[0] + "|"
                                                                  + orderListData.BusinessSubItem1.Substring(orderListData.BusinessSubItem1.LastIndexOf("|") + 1, orderListData.BusinessSubItem1.Length - orderListData.BusinessSubItem1.LastIndexOf("|") - 1);
                            //根据供应商账号找到供应商类别
                            pendingPaymentmodel.PaymentCompany = db.Queryable<Business_CustomerBankInfo>()
                                .Where(x => x.BankAccount == model.SupplierBankAccount).First().CompanyOrPerson; ;
                            pendingPaymentmodel.CollectBankAccountName = model.SupplierBankAccountName;
                            pendingPaymentmodel.CollectBankAccouont = model.SupplierBankAccount;
                            pendingPaymentmodel.CollectBankName = model.SupplierBank;
                            pendingPaymentmodel.CollectBankNo = model.SupplierBankNo;
                            pendingPaymentmodel.PaymentMethod = model.PayType;
                            pendingPaymentmodel.IdentityToken = cache[PubGet.GetUserKey].Token;
                            pendingPaymentmodel.FunctionSiteId = "61";
                            pendingPaymentmodel.OperatorIP = GetSystemInfo.GetClientLocalIPv4Address();
                            pendingPaymentmodel.invoiceNumber = assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count().ToString();
                            pendingPaymentmodel.numberOfAttachments = (assetAttachmentList.Count() - assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count()).ToString();
                            pendingPaymentmodel.Amount = model.ContractAmount.ToString();
                            pendingPaymentmodel.Summary = model.AssetDescription;
                            pendingPaymentmodel.AccountSetCode = cache[PubGet.GetUserKey].AccountModeCode + "|" + cache[PubGet.GetUserKey].CompanyCode;
                            var apiReault = PendingPaymentApi(pendingPaymentmodel);
                            var pendingRedult = apiReault.JsonToModel<JsonResultModelApi<Api_PendingPayment>>();
                            if (pendingRedult.success)
                            {
                                var orderModel = db.Queryable<Business_FixedAssetsOrder>()
                                    .Where(x => x.VGUID == model.VGUID).First();
                                orderModel.PaymentVoucherVguid = pendingRedult.data.vguid;
                                orderModel.PaymentVoucherUrl = pendingRedult.data.url;
                                db.Updateable<Business_FixedAssetsOrder>(orderModel).UpdateColumns(x => new { x.PaymentVoucherUrl, x.PaymentVoucherVguid }).ExecuteCommand();
                                model.SubmitStatus = FixedAssetsSubmitStatusEnum.UnPay.TryToInt();
                                model.SubmitDate = DateTime.Now;
                                model.SubmitUser = cache[PubGet.GetUserKey].LoginName;
                                db.Updateable<Business_FixedAssetsOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                                //提交完后写入采购分配表
                                var purchaseAssignmodel = new Business_PurchaseAssign();
                                purchaseAssignmodel.VGUID = Guid.NewGuid();
                                purchaseAssignmodel.CreateDate = DateTime.Now;
                                purchaseAssignmodel.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                purchaseAssignmodel.FixedAssetsOrderVguid = model.VGUID;
                                purchaseAssignmodel.PurchaseGoodsVguid = model.PurchaseGoodsVguid;
                                purchaseAssignmodel.PurchaseGoods = model.PurchaseGoods;
                                purchaseAssignmodel.OrderQuantity = model.OrderQuantity;
                                purchaseAssignmodel.PurchasePrices = model.PurchasePrices;
                                purchaseAssignmodel.ContractAmount = model.ContractAmount;
                                purchaseAssignmodel.AssetDescription = model.AssetDescription;
                                db.Insertable<Business_PurchaseAssign>(purchaseAssignmodel).ExecuteCommand();
                                var fundClearingModel = Mapper.Map<Business_FundClearing>(purchaseAssignmodel);
                                fundClearingModel.VGUID = Guid.NewGuid();
                                db.Insertable<Business_FundClearing>(fundClearingModel).ExecuteCommand();
                                var companys = db.Queryable<Business_PurchaseManagementCompany>()
                                    .Where(x => x.PurchaseOrderSettingVguid == orderModel.PurchaseGoodsVguid && x.IsCheck).ToList();
                                var liquidationDistributionList = new List<Business_LiquidationDistribution>();
                                foreach (var company in companys)
                                {
                                    var liquidationDistribution = new Business_LiquidationDistribution();
                                    liquidationDistribution.VGUID = Guid.NewGuid();
                                    liquidationDistribution.FundClearingVguid = fundClearingModel.VGUID;
                                    liquidationDistribution.AssetsOrderVguid = fundClearingModel.FixedAssetsOrderVguid;
                                    liquidationDistribution.CompanyVguid = company.ManagementCompanyVguid;
                                    liquidationDistribution.Company = company.ManagementCompany;
                                    liquidationDistribution.PurchasePrices = fundClearingModel.PurchasePrices;
                                    liquidationDistribution.AssetNum = 0;
                                    liquidationDistribution.ContractAmount = 0;
                                    liquidationDistribution.CreateDate = DateTime.Now;
                                    liquidationDistribution.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                    liquidationDistributionList.Add(liquidationDistribution);
                                }
                                db.Insertable<Business_LiquidationDistribution>(liquidationDistributionList).ExecuteCommand();
                                resultModel.ResultInfo = pendingRedult.data.url;
                                resultModel.IsSuccess = true;
                                resultModel.Status = "1";
                            }
                            else
                            {
                                LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                            }
                        }
                        else
                        {
                            resultModel.ResultInfo = "该状态下不允许发起支付";
                            resultModel.IsSuccess = false;
                            resultModel.Status = "2";
                        }
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