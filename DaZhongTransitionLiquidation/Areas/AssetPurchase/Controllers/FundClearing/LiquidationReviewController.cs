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
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.FundClearing
{
    public class LiquidationReviewController : BaseController
    {
        // GET: AssetPurchase/LiquidationReview
        public LiquidationReviewController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
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
                jsonResult.Rows = db.SqlQueryable<Business_FundClearing>("SELECT pa.* FROM Business_FundClearing pa INNER JOIN (select * from Business_FixedAssetsOrder where SubmitStatus = 2) fao ON pa.FixedAssetsOrderVguid = fao.VGUID and pa.SubmitStatus = 1")//WHERE fao.SubmitStatus = 1
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetFundClearingOrder(Guid FixedAssetsOrderVguid)
        {
            var list = new List<Business_FundClearingOrder>();
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_FundClearingOrder>().Where(x => x.FixedAssetsOrderVguid == FixedAssetsOrderVguid).ToList();
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SubmitLiquidation(Guid FundClearingVguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var fundClearingModel = db.Queryable<Business_FundClearing>().Where(x => x.VGUID == FundClearingVguid).First();
                var orderList = db.Queryable<Business_FundClearingOrder>()
                    .Where(x => x.FixedAssetsOrderVguid == fundClearingModel.FixedAssetsOrderVguid).ToList();
                if (orderList.Any(x => x.SubmitStatus == 0))
                {
                    foreach (var order in orderList)
                    {
                        //请求清算平台、待付款请求生成支付凭证接口
                        var pendingPaymentmodel = new PendingPaymentModel();
                        //统计附件信息
                        var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>().Where(x => x.AssetOrderVGUID == fundClearingModel.FixedAssetsOrderVguid).ToList();
                        pendingPaymentmodel.PaymentReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "付款凭证").ToList());
                        pendingPaymentmodel.InvoiceReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList());
                        pendingPaymentmodel.ApprovalReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "OA审批单").ToList());
                        pendingPaymentmodel.Contract = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "合同").ToList());
                        pendingPaymentmodel.DetailList = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "清单、清册").ToList());
                        pendingPaymentmodel.OtherReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "其他").ToList());
                        var goodsData = db.Queryable<Business_PurchaseOrderSetting>()
                            .Where(x => x.VGUID == order.PurchaseGoodsVguid).First();
                        var orderListData = db.Queryable<v_Business_BusinessTypeSet>()
                            .Where(x => x.BusinessSubItem1 == goodsData.BusinessSubItem).First();
                        pendingPaymentmodel.ServiceCategory = orderListData.BusinessProject;
                        pendingPaymentmodel.BusinessProject = orderListData.BusinessSubItem1.Split("|")[0] + "|"
                                                              + orderListData.BusinessSubItem1.Substring(orderListData.BusinessSubItem1.LastIndexOf("|") + 1, orderListData.BusinessSubItem1.Length - orderListData.BusinessSubItem1.LastIndexOf("|") - 1);
                        //根据供应商账号找到供应商类别
                        pendingPaymentmodel.PaymentCompany = db.Queryable<Business_CustomerBankInfo>()
                            .Where(x => x.BankAccount == order.SupplierBankAccount).First().CompanyOrPerson; ;
                        pendingPaymentmodel.CollectBankAccountName = order.SupplierBankAccountName;
                        pendingPaymentmodel.CollectBankAccouont = order.SupplierBankAccount;
                        pendingPaymentmodel.CollectBankName = order.SupplierBank;
                        pendingPaymentmodel.CollectBankNo = order.SupplierBankNo;
                        pendingPaymentmodel.PaymentMethod = order.PayType;
                        pendingPaymentmodel.IdentityToken = cache[PubGet.GetUserKey].Token;
                        pendingPaymentmodel.FunctionSiteId = "61";
                        pendingPaymentmodel.OperatorIP = GetSystemInfo.GetClientLocalIPv4Address();
                        pendingPaymentmodel.invoiceNumber = assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count().ToString();
                        pendingPaymentmodel.numberOfAttachments = (assetAttachmentList.Count() - assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count()).ToString();
                        pendingPaymentmodel.Amount = order.ContractAmount.ToString();
                        pendingPaymentmodel.Summary = order.AssetDescription;
                        pendingPaymentmodel.AccountSetCode = cache[PubGet.GetUserKey].AccountModeCode + "|" + cache[PubGet.GetUserKey].CompanyCode;
                        var apiReault = PendingPaymentApi(pendingPaymentmodel);
                        var pendingRedult = apiReault.JsonToModel<JsonResultModelApi<Api_PendingPayment>>();
                        if (pendingRedult.success)
                        {
                            var orderModel = db.Queryable<Business_FundClearingOrder>()
                                .Where(x => x.VGUID == order.VGUID).First();
                            orderModel.PaymentVoucherVguid = pendingRedult.data.vguid;
                            orderModel.PaymentVoucherUrl = pendingRedult.data.url;
                            db.Updateable<Business_FundClearingOrder>(orderModel).UpdateColumns(x => new { x.PaymentVoucherUrl, x.PaymentVoucherVguid }).ExecuteCommand();
                            order.SubmitStatus = FixedAssetsSubmitStatusEnum.UnPay.TryToInt();
                            order.SubmitDate = DateTime.Now;
                            order.SubmitUser = cache[PubGet.GetUserKey].LoginName;
                            db.Updateable<Business_FundClearingOrder>(order).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                            resultModel.ResultInfo = pendingRedult.data.url;
                            resultModel.IsSuccess = true;
                            resultModel.Status = "1";
                        }
                        else
                        {
                            LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                        }
                    }
                }
                else
                {
                    resultModel.ResultInfo = "该状态下不允许发起支付";
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                }
            });
            return Json(resultModel);
        }
        public JsonResult RejectLiquidation(Guid FundClearingVguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var fundClearingModel = db.Queryable<Business_FundClearing>().Where(x => x.VGUID == FundClearingVguid).First();
                fundClearingModel.SubmitStatus = 2;//驳回
                db.Updateable<Business_FundClearing>(fundClearingModel).ExecuteCommand();
                var orderList = db.Queryable<Business_FundClearingOrder>()
                    .Where(x => x.FixedAssetsOrderVguid == fundClearingModel.FixedAssetsOrderVguid).ToList();
                db.Deleteable<Business_FundClearingOrder>(orderList).ExecuteCommand();
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
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