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
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using DaZhongTransitionLiquidation.Models;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.IntangibleAssetsOrder
{

    public class IntangibleAssetsOrderController : BaseController
    {
        public IntangibleAssetsOrderController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetManagement/IntangibleAssetsOrder
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
                jsonResult.Rows = db.Queryable<Business_IntangibleAssetsOrder>()
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .WhereIF(searchParams.OrderNumber != null, i => i.OrderNumber.Contains(searchParams.OrderNumber))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteIntangibleAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                var isAnySubmited = db.Queryable<Business_IntangibleAssetsOrder>().Any(c => vguids.Contains(c.VGUID) && c.SubmitStatus != IntangibleAssetsSubmitStatusEnum.FirstPaymentUnSubmit.TryToInt());
                if (isAnySubmited)
                {
                    resultModel.ResultInfo = "存在已提交的订单，订单提交后不允许删除";
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                }
                else
                {
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_IntangibleAssetsOrder>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult SubmitIntangibleAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    foreach (var vguid in vguids)
                    {
                        //请求清算平台、待付款请求生成支付凭证接口
                        var pendingPaymentmodel = new PendingPaymentModel();
                        var model = db.Queryable<Business_IntangibleAssetsOrder>().Where(c => c.VGUID == vguid).First();
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
                        else
                        {
                            resultModel.ResultInfo = "该状态下不能发起支付";
                            resultModel.IsSuccess = false;
                            resultModel.Status = "2";
                            return;
                        }
                        //统计附件信息
                        var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>().Where(x => x.AssetOrderVGUID == vguid).ToList();
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
                            var orderModel = db.Queryable<Business_IntangibleAssetsOrder>()
                                .Where(x => x.VGUID == model.VGUID).First();
                            orderModel.PaymentVoucherVguid = pendingRedult.data.vguid;
                            orderModel.PaymentVoucherUrl = pendingRedult.data.url;
                            orderModel.SubmitStatus = model.SubmitStatus;
                            db.Updateable<Business_IntangibleAssetsOrder>(orderModel).UpdateColumns(x => new { x.PaymentVoucherUrl, x.PaymentVoucherVguid,x.SubmitStatus }).ExecuteCommand();
                            resultModel.ResultInfo = pendingRedult.data.url;
                            resultModel.IsSuccess = true;
                            resultModel.Status = "1";
                        }
                        else
                        {
                            LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                        }
                    }
                });
            });
            return Json(resultModel);
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
    }
}