using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SyntacticSugar;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.IntangibleAssetsOrder
{

    public class IntangibleAssetsOrderDetailController : BaseController
    {
        // GET: AssetManagement/IntangibleAssetsOrderDetail
        public IntangibleAssetsOrderDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            var vguid = Request["VGUID"].TryToGuid();
            if (vguid != Guid.Empty)
            {
                ViewBag.SubmitStatus = GetSubmitStatus(Request["VGUID"].TryToGuid());
            }
            return View();
        }
        public JsonResult SaveIntangibleAssetsOrder(Business_IntangibleAssetsOrder sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var model = db.Queryable<Business_IntangibleAssetsOrder>().Where(c => c.VGUID == sevenSection.VGUID);
                    if (model.Count() == 0)
                    {
                        var orderNumberLeft = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                        //查出当前日期数据库中最大的订单号
                        var currentDayFixedAssetOrderList = db.Queryable<Business_FixedAssetsOrder>()
                            .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new { c.OrderNumber }).ToList();
                        var currentDayTaxFeeOrderList = db.Queryable<Business_TaxFeeOrder>()
                            .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new { c.OrderNumber }).ToList();
                        var currentDayIntangibleAssetsOrderList = db.Queryable<Business_IntangibleAssetsOrder>()
                            .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new { c.OrderNumber }).ToList();
                        var currentDayList = currentDayFixedAssetOrderList.Union(currentDayIntangibleAssetsOrderList).Union(currentDayTaxFeeOrderList).ToList();
                        var maxOrderNumRight = 0;
                        if (currentDayList.Any())
                        {
                            maxOrderNumRight = currentDayList.OrderByDescending(c => c.OrderNumber.Replace(orderNumberLeft, "").TryToInt()).First().OrderNumber.Replace(orderNumberLeft, "").TryToInt();
                        }
                        maxOrderNumRight = maxOrderNumRight + 1;
                        sevenSection.OrderNumber = orderNumberLeft + maxOrderNumRight.ToString().PadLeft(4, '0');
                        sevenSection.CreateDate = DateTime.Now;
                        sevenSection.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        sevenSection.SubmitStatus = IntangibleAssetsSubmitStatusEnum.FirstPaymentUnSubmit.TryToInt();
                        sevenSection.ISVerify = false;
                        db.Insertable<Business_IntangibleAssetsOrder>(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.ChangeDate = DateTime.Now;
                        sevenSection.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                        db.Updateable<Business_IntangibleAssetsOrder>(sevenSection).IgnoreColumns(x => new { x.CreateDate, x.CreateUser, x.SubmitStatus,x.OrderNumber }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
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
        public JsonResult PendingPaymentAttachmentUpload(Guid PaymentVoucherVguid, Guid Vguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                //请求清算平台、待付款请求生成支付凭证接口
                var pendingPaymentmodel = new PendingPaymentModel();
                pendingPaymentmodel.IdentityToken = cache[PubGet.GetUserKey].Token;
                pendingPaymentmodel.FunctionSiteId = "61";
                pendingPaymentmodel.OperatorIP = GetSystemInfo.GetClientLocalIPv4Address();
                //统计附件信息
                var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>()
                    .Where(x => x.AssetOrderVGUID == Vguid).ToList();
                pendingPaymentmodel.PaymentReceipt =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "付款凭证").ToList());
                pendingPaymentmodel.InvoiceReceipt =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList());
                pendingPaymentmodel.ApprovalReceipt =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "OA审批单").ToList());
                pendingPaymentmodel.Contract =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "合同").ToList());
                pendingPaymentmodel.DetailList =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "清单、清册").ToList());
                pendingPaymentmodel.OtherReceipt =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "其他").ToList());

                var apiReault = PendingPaymentAttachmentApi(pendingPaymentmodel, PaymentVoucherVguid);
                var pendingRedult = apiReault.JsonToModel<PendingResultModel>();
                resultModel.IsSuccess = pendingRedult.success;
                resultModel.Status = pendingRedult.success ? "1" : "0";
                resultModel.ResultInfo = pendingRedult.code;
                resultModel.ResultInfo2 = pendingRedult.message;
                if (!pendingRedult.success)
                {
                    LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public string PendingPaymentAttachmentApi(PendingPaymentModel model, Guid PaymentVoucherVguid)
        {
            var url = ConfigSugar.GetAppString("PendingPaymentAttachmentUrl");
            var data = "{" +
                       "\"IdentityToken\":\"{IdentityToken}\",".Replace("{IdentityToken}", model.IdentityToken) +
                       "\"FunctionSiteId\":\"{FunctionSiteId}\",".Replace("{FunctionSiteId}", "61") +
                       "\"OperatorIP\":\"{OperatorIP}\",".Replace("{OperatorIP}",
                           GetSystemInfo.GetClientLocalIPv4Address()) +
                       "\"vguid\":\"{vguid}\",".Replace("{vguid}", PaymentVoucherVguid.ToString());
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
        public JsonResult GetIntangibleAssetsOrder(Guid vguid)
        {
            Business_IntangibleAssetsOrder model = new Business_IntangibleAssetsOrder();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                model = db.Queryable<Business_IntangibleAssetsOrder>().Single(x => x.VGUID == vguid);
            });
            return Json(model, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult UploadContractFile(Guid Vguid, HttpPostedFileBase File)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            if (File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "PurchaseContract\\" + newFileName;
                var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath;
                try
                {
                    File.SaveAs(filePath);
                    DbBusinessDataService.Command(db =>
                    {
                        var result = db.Ado.UseTran(() =>
                        {
                            var sevenSection = db.Queryable<Business_IntangibleAssetsOrder>().Where(c => c.VGUID == Vguid).First();
                            if (sevenSection != null)
                            {
                                sevenSection.ContractFilePath = uploadPath;
                                sevenSection.ContractName = File.FileName;
                                sevenSection.ChangeDate = DateTime.Now;
                                sevenSection.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                                db.Updateable(sevenSection).UpdateColumns(x => new {
                                    x.ChangeDate,
                                    x.ChangeUser,
                                    x.ContractFilePath,
                                    x.ContractName
                                }).ExecuteCommand();
                            }
                        });
                        resultModel.IsSuccess = result.IsSuccess;
                        resultModel.ResultInfo = uploadPath;
                        resultModel.ResultInfo2 = File.FileName;
                        resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", filePath, ex.ToString()));
                }
            }
            return Json(resultModel);
        }
        //public JsonResult GetUseDepartment()
        //{
        //    var departmentData = new List<Business_SevenSection>();
        //    DbBusinessDataService.Command(db =>
        //    {
        //        departmentData = db.Queryable<Business_SevenSection>().Where(x =>
        //            x.SectionVGUID == "D63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == "1002" &&
        //            x.CompanyCode == "01" && x.Status == "1" && x.Code.StartsWith("10")).ToList();
        //    });
        //    return Json(departmentData, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult SubmitIntangibleAssetsOrder(Guid vguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
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
                        db.Updateable<Business_IntangibleAssetsOrder>(orderModel).UpdateColumns(x => new { x.PaymentVoucherUrl, x.PaymentVoucherVguid, x.SubmitStatus }).ExecuteCommand();
                        resultModel.ResultInfo = pendingRedult.data.url;
                        resultModel.IsSuccess = true;
                        resultModel.Status = "1";
                    }
                    else
                    {
                        LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                    }
                });
            });
            return Json(resultModel);
        }
        public int GetSubmitStatus(Guid vguid)
        {
            Business_IntangibleAssetsOrder model = new Business_IntangibleAssetsOrder();
            DbBusinessDataService.Command(db =>
            {
                model = db.Queryable<Business_IntangibleAssetsOrder>().Single(x => x.VGUID == vguid);
            });
            return model.SubmitStatus.TryToInt();
        }
        public JsonResult GetPurchaseGoods(int OrderCategory, Guid[] PurchaseDepartment)
        {
            var PurchaseDepartmentStr = "";

            var orderTypeData = new List<Business_PurchaseOrderSetting>();
            DbBusinessDataService.Command(db =>
            {
                if (PurchaseDepartment == null)
                {
                    orderTypeData = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.OrderCategory == OrderCategory).ToList();
                }
                else
                {
                    foreach (var str in PurchaseDepartment)
                    {
                        PurchaseDepartmentStr = PurchaseDepartmentStr + str + "','";
                    }

                    PurchaseDepartmentStr = PurchaseDepartmentStr.Substring(0, PurchaseDepartmentStr.Length - 3);
                    orderTypeData = db.SqlQueryable<Business_PurchaseOrderSetting>(@"SELECT DISTINCT bpos.* FROM  Business_PurchaseOrderSetting bpos INNER JOIN
                    Business_PurchaseDepartment bpd ON bpos.VGUID = bpd.PurchaseOrderSettingVguid
                    WHERE OrderCategory = '1' And bpd.DepartmentVguid IN ('" + PurchaseDepartmentStr + "')").ToList();
                }
            });
            return Json(orderTypeData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ObsoleteIntangibleAssetsOrder(Business_IntangibleAssetsOrder sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var isAnySubmited = db.Queryable<Business_IntangibleAssetsOrder>().Any(c => c.VGUID == sevenSection.VGUID && c.SubmitStatus > IntangibleAssetsSubmitStatusEnum.FirstPaymentUnSubmit.TryToInt());
                    if (isAnySubmited)
                    {
                        resultModel.ResultInfo = "此状态下不允许作废!";
                        resultModel.IsSuccess = false;
                        resultModel.Status = "2";
                    }
                    else
                    {
                        db.Updateable<Business_IntangibleAssetsOrder>()
                            .UpdateColumns(it => new Business_IntangibleAssetsOrder() { SubmitStatus = 8 })
                            .Where(it => it.VGUID == sevenSection.VGUID).ExecuteCommand();
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                    }
                });
            });
            return Json(resultModel);
        }

    }
}