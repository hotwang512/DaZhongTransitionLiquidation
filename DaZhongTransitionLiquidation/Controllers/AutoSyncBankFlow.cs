using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.BankData;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VehicleBusiness;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherList;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
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
                    using (SqlSugarClient _db = DbBusinessDataConfig.GetInstance())
                    {
                        var companyBankData = _db.Queryable<Business_CompanyBankInfo>().Where(x => x.OpeningDirectBank == true).ToList();
                        foreach (var item in companyBankData)
                        {
                            bankFlowList = ShanghaiBankAPI.GetShangHaiBankTradingFlow(item.BankAccount);
                            if (bankFlowList.Count > 0)
                            {
                                success = WirterSyncBankFlow(bankFlowList);
                            }
                        }
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
                        using (SqlSugarClient _db = DbBusinessDataConfig.GetInstance())
                        {
                            var companyBankData = _db.Queryable<Business_CompanyBankInfo>().Where(x => x.OpeningDirectBank == true).ToList();
                            foreach (var item in companyBankData)
                            {
                                bankFlowList = ShanghaiBankAPI.GetShangHaiBankYesterdayTradingFlow(item.BankAccount);
                                if (bankFlowList.Count > 0)
                                {
                                    success = WirterSyncBankFlow(bankFlowList);
                                }
                            }
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
        public static void AutoSyncBankSeavice()
        {
            Thread LogThread = new Thread(new ThreadStart(DoSyncBankCBCBCM));
            //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
            LogThread.IsBackground = true;
            LogThread.Start();//起线程  
        }
        public static void DoSyncBankCBCBCM()
        {
            while (true)
            {
                //同步交行&建行交易流水
                List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
                var success = 0;
                try
                {
                    using (SqlSugarClient _db = DbBusinessDataConfig.GetInstance())
                    {
                        var bankData = _db.Queryable<BankAndEnterprise_Swap>().Where(x => x.ATTRIBUTE4 != "上海银行" && x.LAST_UPDATE_DATE > DateTime.Now.AddDays(-7)).ToList();
                        //var bankData = _db.Queryable<BankAndEnterprise_Swap>().Where(x => x.ATTRIBUTE4 != "上海银行" && x.LAST_UPDATE_DATE >= "2019-09-02".TryToDate() && x.LAST_UPDATE_DATE < "2019-09-04".TryToDate()).ToList();
                        //查询公司段中已启用的公司的银行信息
                        var bankAccount = _db.SqlQueryable<Business_CompanyBankInfo>(@"select a.* from Business_CompanyBankInfo as a left join Business_SevenSection
                                            as b on a.AccountModeCode = b.AccountModeCode and a.CompanyCode = b.Code  where b.Status='1' 
                                            and b.SectionVGUID ='A63BD715-C27D-4C47-AB66-550309794D43'").ToList();
                        var bankFlowData = _db.Queryable<Business_BankFlowTemplate>().Where(x => x.TradingBank != "上海银行").ToList();
                        bankFlowList = GetBankData(_db, bankData, bankFlowList, bankFlowData, bankAccount);
                        if (bankFlowList.Count > 0)
                        {
                            //按交易日期排序取最小值
                            bankFlowList = bankFlowList.OrderBy(c => c.TransactionDate).ToList();
                            success = WirterSyncBankFlow(bankFlowList);
                            //同步银行流水到银行数据表
                            BankDataPack.SyncBackFlow(bankFlowList[0].TransactionDate.GetValueOrDefault().AddDays(-1));
                        }
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
        public static void AutoVehicleSeavice()
        {
            Thread LogThread = new Thread(new ThreadStart(DoVehicleBusiness));
            //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
            LogThread.IsBackground = true;
            LogThread.Start();//起线程  
        }
        public static void DoVehicleBusiness()
        {
            while (true)
            {
                var success = 0;
                try
                {
                    using (SqlSugarClient _db = DbBusinessDataConfig.GetInstance())
                    {
                        var resultModel = new ResultModel<string>() { IsSuccess = true, Status = "0" };
                        var beginTime = DateTime.Now.AddDays(1 - DateTime.Now.Day).ToLongDateString();
                        var nowDate = DateTime.Now.ToLongDateString();
                        if (nowDate == beginTime)
                        {
                            VehicleBusinessController.SyncVehicleBusiness(_db, resultModel, "admin");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", success, ex.ToString()));
                }
                Thread.Sleep((int)(24 * 1000 * 60 * 60));//一天查一次
            }
        }
        public static void AutoGetVoucherMoneySeavice()
        {
            Thread LogThread = new Thread(new ThreadStart(DoGetVoucherMoney));
            //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
            LogThread.IsBackground = true;
            LogThread.Start();//起线程  
        }
        public static void DoGetVoucherMoney()
        {
            while (true)
            {
                var success = 0;
                try
                {
                    using (SqlSugarClient _db = DbBusinessDataConfig.GetInstance())
                    {
                        //一小时查一次,获取当天每笔凭证的贷方金额
                        var day = DateTime.Now.ToString("yyyy-MM-dd").TryToDate();
                        //测试 day = "2019-08-26".TryToDate();
                        var voucherData = _db.Queryable<Business_VoucherList>().Where(x => x.VoucherDate >= day.AddDays(-7)).ToList();
                        var voucherDetails = _db.Queryable<Business_VoucherDetail>().OrderBy(x=>x.BorrowMoney, OrderByType.Desc).ToList();
                        var accountInfo = _db.Queryable<V_BankChannelMapping>().Where(x => x.IsUnable == "启用" || x.IsUnable == null || x.IsShow == "1").ToList();
                        var accountDetail = _db.Queryable<Business_PaySettingDetail>().ToList();
                        var month = DateTime.Now.ToString("yyyy-MM");
                        var bankFlowList = _db.Ado.SqlQuery<usp_RevenueAmountReport>(@"exec usp_RevenueAmountReport @Month,@Channel", new { Month = month, Channel = "" }).ToList();
                        foreach (var item in voucherData)
                        {
                            var voucherDetail = voucherDetails.Where(x => x.VoucherVGUID == item.VGUID).OrderByDescending(x=>x.ReceivableAccount).ToList();
                            if(voucherDetail.Count < 2)
                            {
                                continue;
                            }
                            if(item.CreditAmountTotal == item.DebitAmountTotal && item.CreditAmountTotal != 0 && item.DebitAmountTotal != 0)
                            {
                                continue;
                            }
                            if(voucherDetail.Count == 2)
                            {
                                //一借一贷
                                var borrowMoney = voucherDetail[0].BorrowMoney;
                                var loanMoney = voucherDetail[0].LoanMoney;
                                if((borrowMoney != 0 && loanMoney == null) || (borrowMoney != 0 && loanMoney == 0))
                                {
                                    voucherDetail[1].LoanMoney = borrowMoney;
                                    voucherDetail[1].LoanMoneyCount = borrowMoney;
                                    item.CreditAmountTotal = borrowMoney;
                                    item.DebitAmountTotal = borrowMoney;
                                    _db.Updateable(voucherDetail[1]).ExecuteCommand();
                                    _db.Updateable(item).ExecuteCommand();
                                }
                                else
                                {
                                    voucherDetail[1].BorrowMoney = loanMoney;
                                    voucherDetail[1].BorrowMoneyCount = loanMoney;
                                    item.CreditAmountTotal = loanMoney;
                                    item.DebitAmountTotal = loanMoney;
                                    _db.Updateable(voucherDetail[1]).ExecuteCommand();
                                    _db.Updateable(item).ExecuteCommand();
                                }
                            }
                            else
                            {
                                //多借多贷
                                var receivableAccount = "";
                                decimal? creditAmountTotal = 0;
                                decimal? debitAmountTotal = 0;
                                foreach (var it in voucherDetail)
                                {
                                    #region 循环借贷明细
                                    if (it.ReceivableAccount != "" && it.ReceivableAccount != null)
                                    {
                                        receivableAccount = it.ReceivableAccount;
                                        if (it.LoanMoney != 0 && it.LoanMoney != null)
                                        {
                                            creditAmountTotal = creditAmountTotal + it.LoanMoney;
                                            continue;
                                        }
                                        //贷配置明细
                                        var subject = it.CompanySection+"."+it.SubjectSection + "." + it.AccountSection + "." + it.CostCenterSection + "." + it.SpareOneSection + "." + it.SpareTwoSection + "." + it.IntercourseSection;
                                        var payVGUID = accountInfo.Where(x => x.BankAccount == it.ReceivableAccount).FirstOrDefault().VGUID.TryToString();
                                        var paySetting = accountDetail.Where(x => x.PayVGUID == payVGUID && x.Loan == subject).FirstOrDefault();
                                        if(paySetting == null)
                                        {
                                            continue;
                                        }
                                        //从金额报表中按配置获取金额
                                        if(paySetting.Channel == "898319841215600")
                                        {
                                            //自助发票机另外处理
                                            continue;
                                        }
                                        var amountReport = bankFlowList.Where(x => x.OrganizationName == paySetting.TransferCompany && x.Channel_Id == paySetting.Channel && x.RevenueDate == item.VoucherDate.TryToDate().AddDays(-1).ToString("yyyy-MM-dd")).ToList();
                                        if (amountReport.Count > 0)
                                        {
                                            it.LoanMoney = 0;
                                            switch (paySetting.TransferType)
                                            {
                                                case "银行收款": it.LoanMoney = amountReport.Sum(x=>x.ActualAmountTotal); break;
                                                case "营收缴款": it.LoanMoney = amountReport.Sum(x => x.PaymentAmountTotal); break;
                                                case "手续费": it.LoanMoney = amountReport.Sum(x => x.CompanyBearsFeesTotal); break;
                                                default:
                                                    break;
                                            }
                                            creditAmountTotal = creditAmountTotal + it.LoanMoney;
                                            _db.Updateable(it).ExecuteCommand();
                                            //BVDetail2.LoanMoneyCount = amountReport[0].ActualAmountTotal + amountReport[0].PaymentAmountTotal + amountReport[0].CompanyBearsFeesTotal;
                                        }
                                       
                                    }
                                    else
                                    {
                                        //借配置明细
                                        if(it.BorrowMoney == null || it.BorrowMoney == 0)
                                        {
                                            var subject = it.CompanySection + "." + it.SubjectSection + "." + it.AccountSection + "." + it.CostCenterSection + "." + it.SpareOneSection + "." + it.SpareTwoSection + "." + it.IntercourseSection;
                                            var payVGUID = accountInfo.Where(x => x.BankAccount == receivableAccount).FirstOrDefault().VGUID.TryToString();
                                            var paySetting = accountDetail.Where(x => x.PayVGUID == payVGUID && x.Borrow == subject).FirstOrDefault();
                                            if (paySetting.Channel == "898319841215600")
                                            {
                                                //自助发票机另外处理
                                                continue;
                                            }
                                            //从金额报表中按配置获取金额
                                            var amountReport = bankFlowList.Where(x => x.Channel_Id == paySetting.Channel && x.RevenueDate == item.VoucherDate.TryToDate().AddDays(-1).ToString("yyyy-MM-dd")).ToList();
                                            if (amountReport.Count > 0)
                                            {
                                                switch (paySetting.TransferType)
                                                {
                                                    case "银行收款": it.BorrowMoney = amountReport.Sum(x => x.ActualAmountTotal); break;
                                                    case "营收缴款": it.BorrowMoney = amountReport.Sum(x => x.PaymentAmountTotal); break;
                                                    case "手续费": it.BorrowMoney = amountReport.Sum(x => x.CompanyBearsFeesTotal); break;
                                                    default:
                                                        break;
                                                }
                                                debitAmountTotal = debitAmountTotal + it.BorrowMoney;
                                                _db.Updateable(it).ExecuteCommand();
                                            }
                                        }
                                        else
                                        {
                                            debitAmountTotal = debitAmountTotal + it.BorrowMoney;
                                        }
                                    }
                                    #endregion
                                }
                                item.CreditAmountTotal = creditAmountTotal;
                                item.DebitAmountTotal = debitAmountTotal;
                                if(item.CreditAmountTotal == item.DebitAmountTotal)
                                {
                                    _db.Updateable(item).ExecuteCommand();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", success, ex.ToString()));
                }
                Thread.Sleep((int)(1 * 1000 * 60 * 60));
            }
        }
        public static void AutoTransferVoucherSeavice()
        {
            Thread LogThread = new Thread(new ThreadStart(DoTransferVoucher));
            //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
            LogThread.IsBackground = true;
            LogThread.Start();//起线程  
        }
        public static void DoTransferVoucher()
        {
            while (true)
            {
                List<usp_RevenueAmountReport> bankFlowList = new List<usp_RevenueAmountReport>();
                var success = 0;
                try
                {
                    using (SqlSugarClient _db = DbBusinessDataConfig.GetInstance())
                    {
                        //每天查一次,生成前一天的转账凭证
                        var month = DateTime.Now.ToString("yyyy-MM");
                        var day = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                        bankFlowList = _db.Ado.SqlQuery<usp_RevenueAmountReport>(@"exec usp_RevenueAmountReport @Month,@Channel", new { Month = month, Channel = "" }).ToList();
                        var bankData = bankFlowList.Where(x => x.RevenueDate == day).ToList();
                        if (bankData.Count > 0)
                        {
                            //根据金额报表数据自动生成凭证
                            BankFlowTemplateController.TransferVoucherList(_db, bankData, "admin");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", success, ex.ToString()));
                }
                //double timeSpan = ConfigSugar.GetAppString("TimeSpan").TryToInt();
                Thread.Sleep((int)(24 * 1000 * 60 * 60));
            }
        }
        private static List<Business_BankFlowTemplate> GetBankData(SqlSugarClient _db, List<BankAndEnterprise_Swap> bankData, List<Business_BankFlowTemplate> bankFlowList, List<Business_BankFlowTemplate> bankFlowData,List<Business_CompanyBankInfo> bankAccount)
        {
            foreach (var details in bankData)
            {
                var isAny = bankFlowData.Any(x => x.Batch == details.TRX_SEQUENCE_ID.TryToString() && x.TradingBank == details.ATTRIBUTE4);
                if (isAny)
                {
                    continue;
                }
                var isAny2 = bankFlowList.Any(x => x.Batch == details.TRX_SEQUENCE_ID.TryToString() && x.TradingBank == details.ATTRIBUTE4);
                if (isAny2)
                {
                    continue;
                }
                var isAny3 = bankAccount.Any(x => x.BankAccount == details.ATTRIBUTE3);
                if (!isAny3)
                {
                    continue;
                }
                Business_BankFlowTemplate bankFlow = new Business_BankFlowTemplate();
                bankFlow.BankAccount = details.ATTRIBUTE3;
                bankFlow.Currency = "人民币";
                bankFlow.ReceivingUnitInstitution = "";
                bankFlow.TradingBank = details.ATTRIBUTE4;
                bankFlow.PaymentUnit = "大众交通(集团)股份有限公司大众出租汽车分公司";//我方
                bankFlow.PayeeAccount = details.ATTRIBUTE3;//我方
                bankFlow.ReceivingUnit = details.TRX_ACCOUNT_NAME;//对方
                bankFlow.ReceivableAccount = details.BANK_ACCOUNT_NUM == "空信息" ? "" : details.BANK_ACCOUNT_NUM;//对方
                if (details.CD_FLAG == "1")
                {
                    bankFlow.TurnOut = 0;
                    bankFlow.TurnIn = details.ENTER_CR.TryToDecimal();
                }
                else
                {
                    bankFlow.TurnOut = details.ENTER_CR.TryToDecimal();
                    bankFlow.TurnIn = 0;
                }
                //bankFlow.TurnIn = details.ENTER_DR.TryToDecimal(); 
                //bankFlow.TurnOut = details.ENTER_CR.TryToDecimal();
                bankFlow.Balance = details.BALANCE_AMOUNT.TryToDecimal();
                bankFlow.VGUID = Guid.NewGuid();
                bankFlow.TransactionDate = details.ATTRIBUTE5.TryToDate();
                bankFlow.PaymentUnitInstitution = "";
                bankFlow.Purpose = details.USE;
                bankFlow.Remark = details.DESCRIPTION;
                bankFlow.Batch = details.TRX_SEQUENCE_ID.TryToString();
                bankFlow.VoucherSubjectName = "Oracle同步数据";
                bankFlowList.Add(bankFlow);
            }
            return bankFlowList;
        }
        public static int WirterSyncBankFlow(List<Business_BankFlowTemplate> bankFlowList)
        {
            int success = 0;
            using (SqlSugarClient _db = DbBusinessDataConfig.GetInstance())
            {
                var bankData = _db.Queryable<Business_CompanyBankInfo>().ToList();
                List<Business_BankFlowTemplate> bankFlowLists = new List<Business_BankFlowTemplate>();
                var sevenData = _db.Queryable<Business_SevenSection>().ToList();
                foreach (var item in bankFlowList)
                {
                    var companyBankData = new Business_CompanyBankInfo();
                    if (item.TradingBank == "上海银行")
                    {
                        companyBankData = _db.Queryable<Business_CompanyBankInfo>().Single(x => x.OpeningDirectBank == true && x.BankAccount == item.BankAccount);
                    }
                    else
                    {
                        companyBankData = _db.Queryable<Business_CompanyBankInfo>().Single(x => x.BankAccount == item.BankAccount);
                    }
                    if (companyBankData == null)
                    {
                        continue;
                    }
                    var paymentUnitInstitution = bankData.Where(x => x.BankAccount == item.BankAccount).First().BankName;
                    item.PaymentUnitInstitution = paymentUnitInstitution;
                    var accountModeName = sevenData.Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == companyBankData.AccountModeCode).Descrption;
                    var companyName = sevenData.Single(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Code == companyBankData.CompanyCode && x.AccountModeCode == companyBankData.AccountModeCode).Descrption;
                    var isAny = _db.Queryable<Business_BankFlowTemplate>().Where(x => x.Batch == item.Batch && x.BankAccount == item.BankAccount).ToList();
                    if (isAny.Count > 0)
                    {
                        item.AccountModeCode = companyBankData.AccountModeCode;
                        item.AccountModeName = accountModeName;
                        item.CompanyCode = companyBankData.CompanyCode;
                        item.CompanyName = companyName;
                        //if(item.Batch == "278906" || item.Batch == "278978")
                        //{
                        //    bankFlowLists.Add(item);
                        //}
                        _db.Updateable(item).Where(x => x.Batch == item.Batch && x.BankAccount == item.BankAccount).ExecuteCommand();
                        continue;
                    }
                    item.AccountModeCode = companyBankData.AccountModeCode;
                    item.AccountModeName = accountModeName;
                    item.CompanyCode = companyBankData.CompanyCode;
                    item.CompanyName = companyName;
                    item.CreateTime = DateTime.Now;
                    item.CreatePerson = "admin";
                    bankFlowLists.Add(item);
                }
                var userData = new List<Sys_User>();
                //using (SqlSugarClient db = DbConfig.GetInstance())
                //{
                //    userData = db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
                //}
                if (bankFlowLists.Count > 0)
                {
                    try
                    {
                        _db.Ado.BeginTran();
                        success = _db.Insertable(bankFlowLists).ExecuteCommand();
                        //根据流水自动生成凭证
                        BankFlowTemplateController.GenerateVoucherList(_db, bankFlowLists, "admin", userData);
                        _db.Ado.CommitTran();
                    }
                    catch (Exception ex)
                    {
                        _db.Ado.RollbackTran();
                        throw ex;
                    }
                }
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
                using (SqlSugarClient db = DbBusinessDataConfig.GetInstance())
                {
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
            var data1 = db.Queryable<Business_FixedAssetsOrder>().ToList();
            var data2 = db.Queryable<Business_IntangibleAssetsOrder>().ToList();
            var data3 = db.Queryable<Business_TaxFeeOrder>().ToList();
            var data4 = db.Queryable<Business_FundClearingOrder>().ToList(); 
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
                        //更新保险订单表
                        item.BankStatus = modelData.data.RECO;
                        item.BankStatusName = modelData.data.REMG;
                        item.BankTD = modelData.data.T24D;
                        item.BankTS = modelData.data.T24S;
                        db.Updateable<Business_OrderListDraft>(item).Where(it => it.VGUID == item.VGUID).ExecuteCommand();
                        changeOrderList.Add(item);
                        //更新资产订单表
                        var isAny1 = data1.Any(x => x.PaymentVoucherVguid == item.VGUID);
                        if (isAny1)
                        {
                            var assets1 = data1.SingleOrDefault(x => x.PaymentVoucherVguid == item.VGUID);
                            if (modelData.data.RECO == "0000")
                            {
                                //订单支付成功
                                assets1.SubmitStatus = 2;
                            }
                            else if (modelData.data.RECO == "0003" || modelData.data.RECO == "0005")
                            {
                                //订单支付失败
                                assets1.SubmitStatus = 3;
                            }
                            assets1.BankStatus = modelData.data.REMG;
                            assets1.OSNO = item.OSNO;
                            db.Updateable(assets1).Where(it => it.VGUID == assets1.VGUID).ExecuteCommand();
                        }
                        var isAny2 = data2.Any(x => x.PaymentVoucherVguid == item.VGUID);
                        if (isAny2)
                        {
                            var assets2 = data2.SingleOrDefault(x => x.PaymentVoucherVguid == item.VGUID);
                            //var status = assets2.SubmitStatus + 1;
                            //if (assets2.InterimPayment == null)
                            //{
                            //    //没有中间价
                            //    status = assets2.SubmitStatus + 3;
                            //}
                            if (modelData.data.RECO == "0000")
                            {
                                //订单支付成功
                                assets2.SubmitStatus = 2;
                            }
                            else if (modelData.data.RECO == "0003" || modelData.data.RECO == "0005")
                            {
                                //订单支付失败
                                assets2.SubmitStatus = 3;
                            }
                            assets2.BankStatus = modelData.data.REMG;
                            assets2.OSNO = item.OSNO;
                            db.Updateable(assets2).Where(it => it.VGUID == assets2.VGUID).ExecuteCommand();
                        }
                        var isAny3 = data3.Any(x => x.PaymentVoucherVguid == item.VGUID);
                        if (isAny3)
                        {
                            var assets3 = data3.SingleOrDefault(x => x.PaymentVoucherVguid == item.VGUID);
                            if (modelData.data.RECO == "0000")
                            {
                                //订单支付成功
                                assets3.SubmitStatus = 2;
                            }
                            else if (modelData.data.RECO == "0003" || modelData.data.RECO == "0005")
                            {
                                //订单支付失败
                                assets3.SubmitStatus = 3;
                            }
                            assets3.BankStatus = modelData.data.REMG;
                            assets3.OSNO = item.OSNO;
                            db.Updateable(assets3).Where(it => it.VGUID == assets3.VGUID).ExecuteCommand();
                        }
                        var isAny4 = data4.Any(x => x.PaymentVoucherVguid == item.VGUID);
                        if (isAny4)
                        {
                            var assets4 = data4.SingleOrDefault(x => x.PaymentVoucherVguid == item.VGUID);
                            if (modelData.data.RECO == "0000")
                            {
                                //订单支付成功
                                assets4.SubmitStatus = 2;
                            }
                            else if (modelData.data.RECO == "0003" || modelData.data.RECO == "0005")
                            {
                                //订单支付失败
                                assets4.SubmitStatus = 3;
                            }
                            assets4.BankStatus = modelData.data.REMG;
                            assets4.OSNO = item.OSNO;
                            db.Updateable(assets4).Where(it => it.VGUID == assets4.VGUID).ExecuteCommand();
                        }
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