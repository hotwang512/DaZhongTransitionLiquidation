using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
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
                    saveChanges = db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                    {
                        Status = status,
                    }).Where(it => it.VGUID == item).ExecuteCommand();
                    var orderInfo = db.Queryable<Business_OrderListDraft>().Single(x => x.VGUID == item);
                    Regex rgx = new Regex(@"[\w|\W]{2,4}银行");
                    var rgsOrderBankName = rgx.Match(orderInfo.OrderBankName).Value;
                    var rgsCollectBankName = rgx.Match(orderInfo.CollectBankName).Value;
                    if (rgsOrderBankName == rgsCollectBankName)
                    {
                        //同行
                        //Thread LogThread = new Thread(new ParameterizedThreadStart(GetBankPreAuth));
                        //object[] paramObj = { db, item, orderInfo };
                        ////设置线程为后台线程,那样进程里就不会有未关闭的程序了  
                        //LogThread.IsBackground = true;
                        //LogThread.Start(paramObj);//起线程  

                        //object[] param = (object[])paramObj;
                        //SqlSugarClient db = (SqlSugarClient)param[0];
                        Guid vguid = item.TryToGuid();
                        //Business_OrderListDraft orderInfo = (Business_OrderListDraft)param[2];
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
                                db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                                {
                                    OSNO = modelData.data.serialNo,
                                }).Where(it => it.VGUID == vguid).ExecuteCommand();
                                Thread LogThread = new Thread(new ParameterizedThreadStart(AuthTransferResult));
                                object[] paramObjs = { db, vguid, modelData.data.serialNo };
                                //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
                                LogThread.IsBackground = true;
                                LogThread.Start(paramObjs);//起线程  
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
                        //Thread LogThread = new Thread(new ParameterizedThreadStart(GetCrossBankPreAuth));
                        //object[] paramObj = { db, item, orderInfo };
                        ////设置线程为后台线程,那样进程里就不会有未关闭的程序了  
                        //LogThread.IsBackground = true;
                        //LogThread.Start(paramObj);//起线程  

                        var resultData = "";
                        //object[] param = (object[])paramObj;
                        //SqlSugarClient db = (SqlSugarClient)param[0];
                        Guid vguid = item.TryToGuid();
                        //Business_OrderListDraft orderInfo = (Business_OrderListDraft)param[2];
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
                                db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                                {
                                    OSNO = modelData.data.serialNo,
                                }).Where(it => it.VGUID == vguid).ExecuteCommand();
                                Thread LogThread = new Thread(new ParameterizedThreadStart(AuthTransferResult));
                                object[] paramObjs = { db, vguid, modelData.data.serialNo };
                                //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
                                LogThread.IsBackground = true;
                                LogThread.Start(paramObjs);//起线程  
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
        //获取OSNO：交易序列号 同行
        //public static void GetBankPreAuth(object paramObj)
        //{
           
        //}
        //获取OSNO：交易序列号 跨行
        //public  string  GetCrossBankPreAuth(object paramObj)
        //{
            
        //}
        //根据OSNO获取银行交易状态
        public static void AuthTransferResult(object paramObjs)
        {
            while (true)
            {
                object[] param = (object[])paramObjs;
                SqlSugarClient db = (SqlSugarClient)param[0];
                Guid vguid = param[1].TryToGuid();
                var osno = param[2].TryToString();
                var url = ConfigSugar.GetAppString("AuthTransferResult");
                var data = "{" +
                                "\"OSNO\":\"{OSNO}\"".Replace("{OSNO}", osno) +
                                "}";
                try
                {
                    WebClient wc = new WebClient();
                    wc.Headers.Clear();
                    wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                    wc.Encoding = System.Text.Encoding.UTF8;
                    var resultData = wc.UploadString(new Uri(url), data);
                    var modelData = resultData.JsonToModel<TransferResult>();
                    if (modelData.success)
                    {
                        db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                        {
                            BankStatus = modelData.data.RECO,
                            BankStatusName = modelData.data.REMG,
                            BankTD = modelData.data.T24D,
                            BankTS = modelData.data.T24S
                        }).Where(it => it.VGUID == vguid).ExecuteCommand();  
                    }
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
                }
                double timeSpan = ConfigSugar.GetAppString("TimeSpanMin").TryToInt();
                Thread.Sleep((int)(timeSpan * 1000 * 60));
            }
        }
    }
}