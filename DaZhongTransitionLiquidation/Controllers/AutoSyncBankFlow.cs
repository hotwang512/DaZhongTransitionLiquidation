using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.BankData;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Models;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class AutoSyncBankFlow : Controller
    {

        public static void AutoSyncSeavice()
        {
            Thread LogThread = new Thread(new ThreadStart(DoSyncBank));
            //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
            LogThread.IsBackground = true;
            LogThread.Start();//起线程  
        }
        public static void DoSyncBank()
        {
            while (true)
            {
                List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
                var success = 0;
                try
                {
                    //var tradingStartDate = DateTime.Parse("2018-11-01");
                    //var tradingEndDate = DateTime.Parse("2018-11-27");
                    //bankFlowList = ShanghaiBankAPI.GetShangHaiBankHistoryTradingFlow(tradingStartDate, tradingEndDate);
                    SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
                    var companyBankData = _db.Queryable<Business_CompanyBankInfo>().Where(x => x.OpeningDirectBank == true).ToList();
                    foreach (var item in companyBankData)
                    {
                        bankFlowList = ShanghaiBankAPI.GetShangHaiBankTradingFlow(item.BankAccount);
                        success = WirterSyncBankFlow(bankFlowList);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", success, ex.ToString()));
                }
                double timeSpan = ConfigSugar.GetAppString("TimeSpan").TryToInt();
                Thread.Sleep((int)(timeSpan * 1000 * 60 * 60));
            }
        }
        public static void AutoSyncYesterdaySeavice()
        {
            Thread LogThread = new Thread(new ThreadStart(DoSyncYesterdayBank));
            //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
            LogThread.IsBackground = true;
            LogThread.Start();//凌晨12点半开起线程
        }
        public static void DoSyncYesterdayBank()
        {
            while (true)
            {
                if (DateTime.Now.ToString("HH:mm:ss") == "10:00:00")
                {
                    List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
                    var success = 0;
                    try
                    {
                        SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
                        var companyBankData = _db.Queryable<Business_CompanyBankInfo>().Where(x => x.OpeningDirectBank == true).ToList();
                        foreach (var item in companyBankData)
                        {
                            bankFlowList = ShanghaiBankAPI.GetShangHaiBankYesterdayTradingFlow(item.BankAccount);
                            success = WirterSyncBankFlow(bankFlowList);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(string.Format("Data:{0},result:{1}", success, ex.ToString()));
                    }
                }
                Thread.Sleep((int)(1000));
            }
        }
        public static int WirterSyncBankFlow(List<Business_BankFlowTemplate> bankFlowList)
        {
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            int success = 0;
            foreach (var item in bankFlowList)
            {
                var companyBankData = _db.Queryable<Business_CompanyBankInfo>().Single(x => x.OpeningDirectBank == true && x.BankAccount == item.BankAccount);
                var accountModeName = _db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == companyBankData.AccountModeCode).Descrption;
                var isAny = _db.Queryable<Business_BankFlowTemplate>().Where(x => x.Batch == item.Batch && x.BankAccount == item.BankAccount).ToList();
                if (isAny.Count > 0)
                {
                    item.AccountModeCode = companyBankData.AccountModeCode;
                    item.AccountModeName = accountModeName;
                    item.CompanyCode = companyBankData.CompanyCode;
                    _db.Updateable(item).Where(x => x.Batch == item.Batch && x.BankAccount == item.BankAccount).ExecuteCommand();
                    continue;
                }
                item.AccountModeCode = companyBankData.AccountModeCode;
                item.AccountModeName = accountModeName;
                item.CompanyCode = companyBankData.CompanyCode;
                item.CreateTime = DateTime.Now;
                item.CreatePerson = "sysAdmin";
                success = _db.Insertable(item).ExecuteCommand();
            }
            BankDataPack.SyncBackFlowAndReconciliation();
            return success;
        }
        public static void AutoBankTransferResult()
        {
            Thread LogThread = new Thread(new ThreadStart(AuthTransferResult));
            //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
            LogThread.IsBackground = true;
            LogThread.Start();//凌晨12点半开起线程
        }
        //根据OSNO获取银行交易状态
        public static void AuthTransferResult()
        {
            bool isDo = true;
            while (isDo)
            {
                SqlSugarClient db = DbBusinessDataConfig.GetInstance();
                var orderList = db.Queryable<Business_OrderListDraft>().Where(x => x.Status == "2" && ((x.BankStatus != "0000" && x.BankStatus != "0003" && x.BankStatus != "0005") || x.BankStatus == null)).ToList();
                if (orderList != null)
                {
                    List<Business_OrderListDraft> changeOrderList = new List<Business_OrderListDraft>();
                    foreach (var item in orderList)
                    {
                        if (item.OSNO != null && item.OSNO != "")
                        {
                            CheckTransferResult(item, db, changeOrderList);
                        }
                    }
                    //返回changeOrderList
                }
                double timeSpan = ConfigSugar.GetAppString("TimeSpanMin").TryToInt();
                Thread.Sleep((int)(timeSpan * 1000 * 60));
            }
        }
        public static void CheckTransferResult(Business_OrderListDraft item, SqlSugarClient db, List<Business_OrderListDraft> changeOrderList)
        {
            var url = ConfigSugar.GetAppString("AuthTransferResult");
            var data = "{" +
                                  "\"OSNO\":\"{OSNO}\"".Replace("{OSNO}", item.OSNO) +
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
                    if (item.BankStatus != modelData.data.RECO)
                    {
                        item.BankStatus = modelData.data.RECO;
                        item.BankStatusName = modelData.data.REMG;
                        item.BankTD = modelData.data.T24D;
                        item.BankTS = modelData.data.T24S;
                        db.Updateable<Business_OrderListDraft>(item).Where(it => it.VGUID == item.VGUID).ExecuteCommand();
                        changeOrderList.Add(item);
                    }
                }
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
            }
        }
    }
}