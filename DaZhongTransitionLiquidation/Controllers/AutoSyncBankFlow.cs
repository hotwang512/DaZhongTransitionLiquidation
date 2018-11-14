﻿using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
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
        public DbService DbService;
        public static SqlSugarClient _db = DbBusinessDataConfig.GetInstance();
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
                    bankFlowList = ShanghaiBankAPI.GetShangHaiBankTradingFlow();
                    foreach (var item in bankFlowList)
                    {
                        var isAny = _db.Queryable<Business_BankFlowTemplate>().Any(x => x.Batch == item.Batch);
                        if (isAny)
                        {
                            continue;
                        }
                        success = _db.Insertable(item).ExecuteCommand();
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
                string now = DateTime.Now.ToString("HH:mm:ss");
                if (now == "00:30:00")
                {
                    List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
                    var success = 0;
                    try
                    {
                        bankFlowList = ShanghaiBankAPI.GetShangHaiBankYesterdayTradingFlow();
                        foreach (var item in bankFlowList)
                        {
                            var isAny = _db.Queryable<Business_BankFlowTemplate>().Any(x => x.Batch == item.Batch);
                            if (isAny)
                            {
                                continue;
                            }
                            success = _db.Insertable(item).ExecuteCommand();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(string.Format("Data:{0},result:{1}", success, ex.ToString()));
                    }
                    double timeSpan = 24;
                    Thread.Sleep((int)(timeSpan * 1000 * 60 * 60));
                }
            }
        }
    }
}