using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.BankData;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                    //var tradingEndDate = DateTime.Parse("2018-11-12");
                    //bankFlowList = ShanghaiBankAPI.GetShangHaiBankHistoryTradingFlow(tradingStartDate, tradingEndDate);
                    bankFlowList = ShanghaiBankAPI.GetShangHaiBankTradingFlow();
                    success = WirterSyncBankFlow(bankFlowList);
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
                if (DateTime.Now.ToString("HH:mm:ss") == "00:30:00")
                {
                    List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
                    var success = 0;
                    try
                    {
                        bankFlowList = ShanghaiBankAPI.GetShangHaiBankYesterdayTradingFlow();
                        success = WirterSyncBankFlow(bankFlowList);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(string.Format("Data:{0},result:{1}", success, ex.ToString()));
                    }
                    Thread.Sleep((int)(1000 * 1));
                }
            }
        }


        public static int WirterSyncBankFlow(List<Business_BankFlowTemplate> bankFlowList)
        {
            SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
            int success = 0;
            foreach (var item in bankFlowList)
            {
                var isAny = _db.Queryable<Business_BankFlowTemplate>().Any(x => x.Batch == item.Batch);
                if (isAny)
                {
                    continue;
                }
                success = _db.Insertable(item).ExecuteCommand();
            }
            new Thread(new ThreadStart(BankDataPack.SyncBackFlowAndReconciliation)).Start();
            return success;
        }
    }
}