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
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetOrderListDatas(Business_OrderListDraft searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_OrderListDraft>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_OrderListDraft>()
                .Where(i => i.Status == searchParams.Status)
                .WhereIF(searchParams.FillingDate != null, i => i.FillingDate == searchParams.FillingDate)
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
        public JsonResult UpdataOrderListInfo(List<Guid> vguids, string status)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0",ResultInfo="" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    var resultInfo = "";
                    int saveChanges = 1;
                    //更新主表信息 
                    var orderInfo = db.Queryable<Business_OrderListDraft>().Single(x => x.VGUID == item);
                    Regex rgx = new Regex(@"[\w|\W]{2,4}银行");
                    var rgsOrderBankName = rgx.Match(orderInfo.OrderBankName).Value;
                    var rgsCollectBankName = rgx.Match(orderInfo.CollectBankName).Value;
                    if (rgsOrderBankName == rgsCollectBankName)
                    {
                        //同行
                        Guid vguid = item.TryToGuid();
                        var url = ConfigSugar.GetAppString("BankPreAuthURL");
                        var data = "{" +
                                        "\"ACON\":\"{ACON}\",".Replace("{ACON}", orderInfo.OrderBankAccouont) +
                                        "\"OPAC\":\"{OPAC}\",".Replace("{OPAC}", orderInfo.CollectBankAccouont) +
                                        "\"OPACName\":\"{OPACName}\",".Replace("{OPACName}", orderInfo.CollectBankAccountName) +
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
                                saveChanges = db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                                {
                                    Status = status,
                                }).Where(it => it.VGUID == item).ExecuteCommand();
                                db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                                {
                                    OSNO = modelData.data.serialNo,
                                }).Where(it => it.VGUID == vguid).ExecuteCommand();
                                //查询银行返回状态
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
                    }
                    else
                    {
                        ////跨行
                        var resultData = "";
                        Guid vguid = item.TryToGuid();
                        var url = ConfigSugar.GetAppString("CrossBankPreAuthURL");
                        var data = "{" +
                                        "\"ACON\":\"{ACON}\",".Replace("{ACON}", orderInfo.OrderBankAccouont) +
                                        "\"OPAC\":\"{OPAC}\",".Replace("{OPAC}", orderInfo.CollectBankAccouont) +
                                        "\"OPACName\":\"{OPACName}\",".Replace("{OPACName}", orderInfo.CollectBankAccountName) +
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
                                saveChanges = db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                                {
                                    Status = status,
                                }).Where(it => it.VGUID == item).ExecuteCommand();
                                db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                                {
                                    OSNO = modelData.data.serialNo,
                                }).Where(it => it.VGUID == vguid).ExecuteCommand();
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
                    }
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                    resultModel.ResultInfo = resultInfo;
                }
            });
            return Json(resultModel);
        }
    }
}