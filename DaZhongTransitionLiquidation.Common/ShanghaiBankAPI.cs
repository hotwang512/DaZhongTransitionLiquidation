using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Net;

namespace DaZhongTransitionLiquidation.Common
{
    public class ShanghaiBankAPI
    {
        public static List<Business_BankFlowTemplate> GetShangHaiBankTradingFlow()
        {
            List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
            var capitalAccount = ConfigSugar.GetAppString("CapitalAccount");
            var url = ConfigSugar.GetAppString("TradingFlowUrl");
            var data = "{" +
                            "\"CapitalAccount\":\"{CapitalAccount}\"".Replace("{CapitalAccount}", capitalAccount) +
                            "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();    
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), data);
                var modelData = resultData.JsonToModel<BankFlowResult>();
                if (modelData.success)
                {
                    bankFlowList = SaveBankFlow(modelData.data);
                }
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
            }
            return bankFlowList;
        }
        public static List<Business_BankFlowTemplate> GetShangHaiBankYesterdayTradingFlow()
        {
            List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
            var tradingStartDate = DateTime.Now.AddDays(-1);
            var tradingEndDate = DateTime.Now.AddDays(-1);
            bankFlowList = GetShangHaiBankHistoryTradingFlow(tradingStartDate, tradingEndDate);
            return bankFlowList;
        }
        public static List<Business_BankFlowTemplate> GetShangHaiBankHistoryTradingFlow(DateTime tradingStartDate, DateTime tradingEndDate)
        {
            List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
            var capitalAccount = ConfigSugar.GetAppString("CapitalAccount");
            var url = ConfigSugar.GetAppString("HistoryTradingFlowUrl");
            var data = "{" +
                            "\"CapitalAccount\":\"{CapitalAccount}\",".Replace("{CapitalAccount}", capitalAccount) +
                            "\"TradingStartDate\":\"{TradingStartDate}\",".Replace("{TradingStartDate}", tradingStartDate.ToString("yyyyMMdd")) +
                            "\"TradingEndDate\":\"{TradingEndDate}\"".Replace("{TradingEndDate}", tradingEndDate.ToString("yyyyMMdd")) +
                            "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), data);
                var modelData = resultData.JsonToModel<BankFlowResult>();
                if (modelData.success)
                {
                    bankFlowList = SaveBankFlow(modelData.data);
                }
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
            }
            return bankFlowList;
        }
        public static List<Business_BankFlowTemplate> SaveBankFlow(BankFlowData modelData)
        {
            List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
            foreach (var details in modelData.Detail)
            {
                Business_BankFlowTemplate bankFlow = new Business_BankFlowTemplate();
                bankFlow.Currency = modelData.BIZH;
                bankFlow.ReceivingUnitInstitution = "";
                bankFlow.TradingBank = "上海银行";
                if (details.CDFG == "1")
                {
                    //转出（借）
                    bankFlow.TurnOut = details.FSJE.TryToDecimal();
                    bankFlow.TurnIn = 0;
                    bankFlow.PaymentUnit = modelData.HUMI;
                    bankFlow.PayeeAccount = modelData.ACNO;
                    bankFlow.ReceivingUnit = details.DFHM;
                    bankFlow.ReceivableAccount = details.DFZH;
                }
                else
                {
                    //转入（贷）
                    bankFlow.TurnIn = details.FSJE.TryToDecimal();
                    bankFlow.TurnOut = 0;
                    bankFlow.PaymentUnit = details.DFHM;
                    bankFlow.PayeeAccount = details.DFZH;
                    bankFlow.ReceivingUnit = modelData.HUMI;
                    bankFlow.ReceivableAccount = modelData.ACNO;
                }
                bankFlow.VGUID = Guid.NewGuid();
                string dateString = (details.JYRQ + " " + details.FSSJ);
                dateString = dateString.Replace("年", "-");
                dateString = dateString.Replace("月", "-");
                dateString = dateString.Replace("日", "");
                bankFlow.TransactionDate = dateString.TryToDate();
                bankFlow.PaymentUnitInstitution = "";
                bankFlow.Purpose = details.YOTU;
                bankFlow.Remark = details.BEZH;
                bankFlow.Batch = details.T24F;
                bankFlowList.Add(bankFlow);
            }
            //新增插入银行流水表
            return bankFlowList;
        }
    }
}
