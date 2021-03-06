﻿using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Models;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderListDraft
{
    public class OrderListDraftController : BaseController
    {
        public OrderListDraftController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/OrderListDraft
        public ActionResult Index()
        {
            ViewBag.GetAttachmentUrl = ConfigSugar.GetAppString("GetAttachmentUrl");
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("d046976c-8faf-4b54-b7ec-0ec1b9b71aa1");
            return View();
        }
        public JsonResult GetOrderListDatas(Business_OrderListDraft searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_OrderListDraft>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                DateTime endDate = new DateTime();
                if (searchParams.FillingDate != null)
                {
                    endDate = (searchParams.FillingDate.TryToString().Split(" ")[0] + " " + "23:59:59").TryToDate();
                }
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_OrderListDraft>()
                .Where(i => i.Status == searchParams.Status)
                .Where(i=>i.AccountModeCode == UserInfo.AccountModeCode && i.CompanyCode == UserInfo.CompanyCode)
                .WhereIF(searchParams.FillingDate != null, i => i.FillingDate > searchParams.FillingDate && i.FillingDate < endDate)
                .OrderBy(i => i.CreateTime, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteOrderListInfo(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_OrderListDraft>(x => x.VGUID == item).ExecuteCommand();
                    //删除附件信息
                    db.Deleteable<Business_VoucherAttachmentList>(x => x.VoucherVGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult UpdataOrderListInfo(List<Guid> vguids, string status,string PayBank, string PayAccount, string PayBankAccountName)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0", ResultInfo = "" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    var resultInfo = "";
                    int saveChanges = 1;
                    //更新主表信息 
                    var orderInfo = db.Queryable<Business_OrderListDraft>().Single(x => x.VGUID == item);
                    orderInfo.OrderBankName = PayBank;
                    orderInfo.OrderBankAccount = PayAccount;
                    orderInfo.OrderBankAccountName = PayBankAccountName;
                    db.Updateable<Business_OrderListDraft>(orderInfo).Where(x => x.VGUID == item).ExecuteCommand();
                    Regex rgx = new Regex(@"[\w|\W]{2,4}银行");
                    var rgsOrderBankName = rgx.Match(orderInfo.OrderBankName).Value;
                    var rgsCollectBankName = rgx.Match(orderInfo.CollectBankName).Value;
                    var collectBankAccountName = "";
                    var isAnyBank = db.Queryable<Business_CustomerBankInfo>().Any(x => x.BankAccountName == orderInfo.CollectBankAccountName);
                    if (isAnyBank)
                    {
                        collectBankAccountName = orderInfo.CollectBankAccountName;
                    }
                    else
                    {
                        if(orderInfo.CollectBankAccountName.TryToGuid() == Guid.Empty)
                        {
                            collectBankAccountName = orderInfo.CollectBankAccountName;
                        }
                        else
                        {
                            collectBankAccountName = db.Queryable<Business_CustomerBankInfo>().Single(x => x.VGUID == orderInfo.CollectBankAccountName.TryToGuid()).BankAccountName;
                        }
                    }
                    if (rgsOrderBankName == rgsCollectBankName)
                    {
                        #region 同行
                        Guid vguid = item.TryToGuid();
                        var url = ConfigSugar.GetAppString("BankPreAuthURL");
                        var data = "{" +
                                        "\"ACON\":\"{ACON}\",".Replace("{ACON}", orderInfo.OrderBankAccount) +
                                        "\"OPAC\":\"{OPAC}\",".Replace("{OPAC}", orderInfo.CollectBankAccount) +
                                        "\"OPACName\":\"{OPACName}\",".Replace("{OPACName}", collectBankAccountName) +
                                        "\"OPACTRAM\":\"{OPACTRAM}\",".Replace("{OPACTRAM}", orderInfo.Money.TryToString()) +
                                        "\"USAG\":\"{USAG}\",".Replace("{USAG}", "") +
                                        "\"REMK\":\"{REMK}\"".Replace("{REMK}", "") +
                                        "}";
                        try
                        {
                            WebClient wc = new WebClient();
                            wc.Headers.Clear();
                            wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                            wc.Encoding = System.Text.Encoding.UTF8;
                            var resultData = wc.UploadString(new Uri(url), data);
                            var modelData = resultData.JsonToModel<BankPreAuthResult>();
                            if (modelData.success)
                            {
                                orderInfo.Status = "2";
                                orderInfo.OSNO = modelData.data.serialNo;
                                saveChanges = db.Updateable(orderInfo).Where(it => it.VGUID == vguid).ExecuteCommand();
                                var orderInfoTwo = db.Queryable<Business_OrderListDraft>().Single(x => x.VGUID == item);
                                if (orderInfoTwo.Status == "1")
                                {
                                    db.Updateable(orderInfo).Where(it => it.VGUID == vguid).ExecuteCommand();
                                }
                                var json = orderInfo.ModelToJson();
                                //查询银行返回状态
                                LogHelper.WriteLog(string.Format("orderInfo:{0},status:{1}", json, status));
                                List<Business_OrderListDraft> changeOrderList = new List<Business_OrderListDraft>();
                                AutoSyncBankFlow.CheckTransferResult(orderInfo, db, changeOrderList);
                                var changeOrderListNew = changeOrderList;
                            }
                            else
                            {
                                resultInfo = modelData.errmsg;
                            }
                            LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
                        }
                        #endregion
                    }
                    else
                    {
                        #region 跨行
                        var resultData = "";
                        Guid vguid = item.TryToGuid();
                        var url = ConfigSugar.GetAppString("CrossBankPreAuthURL");
                        var data = "{" +
                                        "\"ACON\":\"{ACON}\",".Replace("{ACON}", orderInfo.OrderBankAccount) +
                                        "\"OPAC\":\"{OPAC}\",".Replace("{OPAC}", orderInfo.CollectBankAccount) +
                                        "\"OPACName\":\"{OPACName}\",".Replace("{OPACName}", collectBankAccountName) +
                                        "\"OPACPBNO\":\"{OPACPBNO}\",".Replace("{OPACPBNO}", orderInfo.CollectBankNo) +
                                        "\"OPACTRAM\":\"{OPACTRAM}\",".Replace("{OPACTRAM}", orderInfo.Money.TryToString()) +
                                        "\"USAG\":\"{USAG}\",".Replace("{USAG}", "") +
                                        "\"REMK\":\"{REMK}\"".Replace("{REMK}", "") +
                                        "}";
                        try
                        {
                            WebClient wc = new WebClient();
                            wc.Headers.Clear();
                            wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                            wc.Encoding = System.Text.Encoding.UTF8;
                            resultData = wc.UploadString(new Uri(url), data);
                            var modelData = resultData.JsonToModel<BankPreAuthResult>();
                            if (modelData.success)
                            {
                                resultInfo = "成功";
                                orderInfo.Status = "2";
                                orderInfo.OSNO = modelData.data.serialNo;
                                saveChanges = db.Updateable(orderInfo).Where(it => it.VGUID == vguid).ExecuteCommand();
                                var orderInfoTwo = db.Queryable<Business_OrderListDraft>().Single(x => x.VGUID == item);
                                if(orderInfoTwo.Status == "1")
                                {
                                    db.Updateable(orderInfo).Where(it => it.VGUID == vguid).ExecuteCommand();
                                }
                                var json = orderInfo.ModelToJson();
                                LogHelper.WriteLog(string.Format("orderInfo:{0},status:{1}", json, status));
                                //查询银行返回状态
                                List<Business_OrderListDraft> changeOrderList = new List<Business_OrderListDraft>();
                                AutoSyncBankFlow.CheckTransferResult(orderInfo, db, changeOrderList);
                                //返回changeOrderList
                            }
                            else
                            {
                                resultInfo = modelData.errmsg;
                            }
                            LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
                        }
                        #endregion
                    }
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                    resultModel.ResultInfo = resultInfo;
                }
            });
            return Json(resultModel);
        }

        public JsonResult GetAttachmentInfo(string PaymentVGUID)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = true, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var VGUID = PaymentVGUID.TryToGuid();
                var url = ConfigSugar.GetAppString("GetAttachmentUrl");
                var data = "{" +
                                        "\"PaymentVGUID\":\"{PaymentVGUID}\"".Replace("{PaymentVGUID}", PaymentVGUID) +
                                        "}";
                try
                {
                    WebClient wc = new WebClient();
                    wc.Headers.Clear();
                    wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                    wc.Encoding = System.Text.Encoding.UTF8;
                    var resultData = wc.UploadString(new Uri(url), data);
                    var modelData = resultData.JsonToModel<AttachmentResult>();
                    if (modelData.success)
                    {
                        var attachInfo = "";
                        db.Deleteable<Business_VoucherAttachmentList>(x => x.VoucherVGUID == VGUID).ExecuteCommand();
                        List<Business_VoucherAttachmentList> VAList = new List<Business_VoucherAttachmentList>();
                        if (modelData.data.PaymentReceipt.Count != 0)
                        {
                            attachInfo += "付款凭证" + modelData.data.PaymentReceipt.Count + "张;";
                            for (int i = 0; i < modelData.data.PaymentReceipt.Count; i++)
                            {
                                Business_VoucherAttachmentList VA = new Business_VoucherAttachmentList();
                                VA.VGUID = Guid.NewGuid();
                                VA.VoucherVGUID = VGUID;
                                VA.Attachment = modelData.data.PaymentReceipt[i];
                                VA.AttachmentType = "付款凭证";
                                VA.CreateTime = DateTime.Now;
                                VA.CreatePerson = UserInfo.LoginName;
                                VAList.Add(VA);
                            }
                        }
                        if (modelData.data.InvoiceReceipt.Count != 0)
                        {
                            attachInfo += "发票" + modelData.data.InvoiceReceipt.Count + "张;";
                            for (int i = 0; i < modelData.data.InvoiceReceipt.Count; i++)
                            {
                                Business_VoucherAttachmentList VA = new Business_VoucherAttachmentList();
                                VA.VGUID = Guid.NewGuid();
                                VA.VoucherVGUID = VGUID;
                                VA.Attachment = modelData.data.InvoiceReceipt[i];
                                VA.AttachmentType = "发票";
                                VA.CreateTime = DateTime.Now;
                                VA.CreatePerson = UserInfo.LoginName;
                                VAList.Add(VA);
                            }
                        }
                        if (modelData.data.ApprovalReceipt.Count != 0)
                        {
                            attachInfo += "OA审批单" + modelData.data.ApprovalReceipt.Count + "张;";
                            for (int i = 0; i < modelData.data.ApprovalReceipt.Count; i++)
                            {
                                Business_VoucherAttachmentList VA = new Business_VoucherAttachmentList();
                                VA.VGUID = Guid.NewGuid();
                                VA.VoucherVGUID = VGUID;
                                VA.Attachment = modelData.data.ApprovalReceipt[i];
                                VA.AttachmentType = "OA审批单";
                                VA.CreateTime = DateTime.Now;
                                VA.CreatePerson = UserInfo.LoginName;
                                VAList.Add(VA);
                            }
                        }
                        if (modelData.data.Contract.Count != 0)
                        {
                            attachInfo += "合同" + modelData.data.Contract.Count + "张;";
                            for (int i = 0; i < modelData.data.Contract.Count; i++)
                            {
                                Business_VoucherAttachmentList VA = new Business_VoucherAttachmentList();
                                VA.VGUID = Guid.NewGuid();
                                VA.VoucherVGUID = VGUID;
                                VA.Attachment = modelData.data.Contract[i];
                                VA.AttachmentType = "合同";
                                VA.CreateTime = DateTime.Now;
                                VA.CreatePerson = UserInfo.LoginName;
                                VAList.Add(VA);
                            }
                        }
                        if (modelData.data.DetailList.Count != 0)
                        {
                            attachInfo += "清单、清册" + modelData.data.DetailList.Count + "张;";
                            for (int i = 0; i < modelData.data.DetailList.Count; i++)
                            {
                                Business_VoucherAttachmentList VA = new Business_VoucherAttachmentList();
                                VA.VGUID = Guid.NewGuid();
                                VA.VoucherVGUID = VGUID;
                                VA.Attachment = modelData.data.DetailList[i];
                                VA.AttachmentType = "清单、清册";
                                VA.CreateTime = DateTime.Now;
                                VA.CreatePerson = UserInfo.LoginName;
                                VAList.Add(VA);
                            }
                        }
                        if (modelData.data.OtherReceipt.Count != 0)
                        {
                            attachInfo += "其他" + modelData.data.OtherReceipt.Count + "张;";
                            for (int i = 0; i < modelData.data.OtherReceipt.Count; i++)
                            {
                                Business_VoucherAttachmentList VA = new Business_VoucherAttachmentList();
                                VA.VGUID = Guid.NewGuid();
                                VA.VoucherVGUID = VGUID;
                                VA.Attachment = modelData.data.OtherReceipt[i];
                                VA.AttachmentType = "其他";
                                VA.CreateTime = DateTime.Now;
                                VA.CreatePerson = UserInfo.LoginName;
                                VAList.Add(VA);
                            }
                        }
                        if (VAList != null && VAList.Count > 0)
                        {
                            db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                            {
                                AttachmentInfo = attachInfo,
                            }).Where(it => it.VGUID == VGUID).ExecuteCommand();
                            db.Insertable(VAList).ExecuteCommand();
                            resultModel.ResultInfo = modelData.message;
                        }
                        else
                        {
                            resultModel.IsSuccess = false;
                            resultModel.ResultInfo = "未找到附件!";
                        }
                    }
                    else
                    {
                        resultModel.IsSuccess = false;
                        resultModel.ResultInfo = modelData.message;
                    }
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
                }
                catch (Exception ex)
                {
                    resultModel.IsSuccess = false;
                    resultModel.ResultInfo = ex.Message;
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
                }
            });
            return Json(resultModel);
        }
    }
}