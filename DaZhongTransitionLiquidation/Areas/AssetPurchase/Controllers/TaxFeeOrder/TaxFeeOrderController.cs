using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using DaZhongTransitionLiquidation.Models;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.TaxFeeOrder
{
    public class TaxFeeOrderController : BaseController
    {
        public TaxFeeOrderController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetPurchase/TaxFeeOrder
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetOrderListDatas(Business_TaxFeeOrder searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_TaxFeeOrder>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_TaxFeeOrder>()
                    .WhereIF(searchParams.VehicleModelCode != null, i => i.VehicleModelCode == searchParams.VehicleModelCode)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .WhereIF(searchParams.OSNO != null, i => i.OSNO.Contains(searchParams.OSNO))
                    .WhereIF(searchParams.PayItemCode != "-1", i => i.PayItemCode == searchParams.PayItemCode)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteTaxFeeOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                var isAnySubmited = db.Queryable<Business_TaxFeeOrder>().Any(c => vguids.Contains(c.VGUID) && (c.SubmitStatus != FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt()));
                if (isAnySubmited)
                {
                    resultModel.ResultInfo = "存在已提交的订单，订单提交后不允许删除";
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                }
                else
                {
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_TaxFeeOrder>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                    //删除订单数量关联表信息
                    db.Deleteable<Business_PurchaseOrderNum>(x => vguids.Contains(x.FaxOrderVguid)).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult CompareTaxFeeOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                DbBusinessDataService.Command(c =>
                {
                    //查看付款项目，供应商信息，付款信息是否一致
                    var feeOrderList = db.Queryable<Business_TaxFeeOrder>().Where(x => vguids.Contains(x.VGUID)).Select(x => new TaxFeeOrderCompare { PaymentInformationVguid = x.PaymentInformationVguid, PayCompanyVguid = x.PayCompanyVguid, PayItemCode = x.PayItemCode }).ToList();
                    if (!feeOrderList.Any(x => x.PaymentInformationVguid == null || x.PayCompanyVguid == null))
                    {
                        var count = feeOrderList.GroupBy(x => new { x.PaymentInformationVguid, x.PayItemCode, x.PayCompanyVguid }).Count();
                        if (count == 1)
                        {
                            resultModel.IsSuccess = true;
                            resultModel.ResultInfo = "匹配一致";
                            resultModel.Status = "1";
                        }
                        else
                        {
                            resultModel.ResultInfo = "您选择的订单不可以合并支付!";
                        }
                    }
                    else
                    {
                        resultModel.ResultInfo = "您选择的订单信息不完整!";
                    }
                });
            });
            return Json(resultModel);
        }

        public JsonResult SubmitTaxFeeOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var orderList = db.Queryable<Business_TaxFeeOrder>().Where(x => vguids.Contains(x.VGUID)).ToList();
                    if (orderList.Any(x =>
                        x.SubmitStatus == FixedAssetsSubmitStatusEnum.UnPay.TryToInt() ||
                        x.SubmitStatus == FixedAssetsSubmitStatusEnum.Submited.TryToInt()))
                    {
                        resultModel.ResultInfo = "该支付状态不允许提交";
                        resultModel.IsSuccess = false;
                        resultModel.Status = "2";
                        return;
                    }
                    //if (orderList.Count > 2)
                    //{
                    //    //判断是否能合并支付
                    //    var set = new HashSet<string>();
                    //    orderList.ForEach(x => {
                    //        set.Add(x.PayItemCode + x.PaymentInformationVguid.ToString() + x.PayCompanyVguid.ToString());//付款项目，供应商，付款公司ID
                    //    });
                    //    if (set.Count != 1)
                    //    {
                    //        resultModel.ResultInfo = "选择的订单不能合并支付";
                    //        resultModel.IsSuccess = false;
                    //        resultModel.Status = "0";
                    //        return;
                    //    }
                    //}
                    //请求清算平台、待付款请求生成支付凭证接口
                    var pendingPaymentmodel = new PendingPaymentModel();
                    //统计附件信息
                    var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>().Where(x => vguids.Contains(x.AssetOrderVGUID)).ToList();
                    pendingPaymentmodel.PaymentReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "付款凭证").ToList());
                    pendingPaymentmodel.InvoiceReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList());
                    pendingPaymentmodel.ApprovalReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "OA审批单").ToList());
                    pendingPaymentmodel.Contract = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "合同").ToList());
                    pendingPaymentmodel.DetailList = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "清单、清册").ToList());
                    pendingPaymentmodel.OtherReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "其他").ToList());
                    var order = orderList.First();
                    var orderData = db.Queryable<v_Business_BusinessTypeSet>()
                        .Where(x => x.BusinessSubItem1 == order.PayItemCode).First();

                    pendingPaymentmodel.ServiceCategory = orderData.BusinessProject;
                    pendingPaymentmodel.BusinessProject = orderData.BusinessSubItem1.Split("|")[0] + "|"
                                                          + orderData.BusinessSubItem1.Substring(orderData.BusinessSubItem1.LastIndexOf("|") + 1, orderData.BusinessSubItem1.Length - orderData.BusinessSubItem1.LastIndexOf("|") - 1);
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
                    pendingPaymentmodel.Amount = orderList.Sum(x => x.SumPayment).ToString();//order.SumPayment.ToString();
                    if (orderList.Count > 1)
                    {
                        pendingPaymentmodel.Summary = "合并支付:";
                        foreach (var item in orderList)
                        {
                            pendingPaymentmodel.Summary += item.PurchaseDescription + "|";
                        }
                    }
                    else
                    {
                        pendingPaymentmodel.Summary = order.PurchaseDescription;
                    }
                    pendingPaymentmodel.AccountSetCode = cache[PubGet.GetUserKey].AccountModeCode + "|" + cache[PubGet.GetUserKey].CompanyCode;

                    var apiReault = PendingPaymentApi(pendingPaymentmodel);
                    var pendingRedult = apiReault.JsonToModel<JsonResultModelApi<Api_PendingPayment>>();
                    if (pendingRedult.success)
                    {
                        db.Updateable<Business_TaxFeeOrder>().UpdateColumns(x => new Business_TaxFeeOrder() { PaymentVoucherUrl = pendingRedult.data.url, PaymentVoucherVguid = pendingRedult.data.vguid }).Where(it => vguids.Contains(it.VGUID)).ExecuteCommand();
                        resultModel.ResultInfo = pendingRedult.data.url;
                        resultModel.IsSuccess = true;
                        resultModel.Status = "1";
                    }
                    else
                    {
                        LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                    }
                    foreach (var vguid in vguids)
                    {
                        var model = db.Queryable<Business_TaxFeeOrder>().Where(c => c.VGUID == vguid).First();
                        if (model.SubmitStatus == FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt())
                        {
                            model.SubmitStatus = FixedAssetsSubmitStatusEnum.UnPay.TryToInt();
                            model.SubmitDate = DateTime.Now;
                            model.SubmitUser = cache[PubGet.GetUserKey].UserName;
                            db.Updateable<Business_TaxFeeOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
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