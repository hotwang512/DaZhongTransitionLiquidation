using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.ReportManagement.Controllers.ReconciliationReport
{
    public class ReconciliationReportPack
    {
        /// <summary>
        /// 自动对账
        /// </summary>
        /// <returns></returns>
        public static void AutomaticReconciliation()
        {
            DbBusinessDataService dbBusinessDataService = new DbBusinessDataService();
            List<v_Business_Reconciliation> business_Reconciliations = new List<v_Business_Reconciliation>();
            dbBusinessDataService.Command(db =>
            {
                business_Reconciliations = db.Queryable<v_Business_Reconciliation>().Where(c => c.Status == "1" && c.BankBillDate > DateTime.Now.AddDays(-7) && c.Channel_Id != null).ToList();
            });
            if (business_Reconciliations.Count > 0)
            {
                foreach (v_Business_Reconciliation business_Reconciliation in business_Reconciliations)
                {
                    string bankDate = business_Reconciliation.BankBillDate.Value.ToString("yyyy-MM-dd");
                    string revenueDate = business_Reconciliation.BankBillDate.Value.AddDays(-1).ToString("yyyy-MM-dd");
                    ResultModel<usp_GetTotalAmount> resultData = GetTotalAmount(dbBusinessDataService, bankDate, revenueDate, business_Reconciliation.Channel_Id);
                    if (resultData != null)
                    {
                        RevenuepaymentReconciliation(
                            dbBusinessDataService,
                            "admin",
                            business_Reconciliation.BankBillDate.Value,
                            revenueDate,
                            business_Reconciliation.Channel_Id,
                            resultData.ResultInfo.RevenueSystemTotalAccount.Value,
                            resultData.ResultInfo.RevenueArrearsTotalAccount.Value,
                            resultData.ResultInfo.RevenuePaymentTotalAccount.Value,
                            resultData.ResultInfo.T1DataArrearsTotalAccount.Value,
                            resultData.ResultInfo.T1DataPaymentTotalAccount.Value,
                            resultData.ResultInfo.DepositArrearsTotalAccount.Value,
                            resultData.ResultInfo.DepositPaymentTotalAccount.Value,
                            resultData.ResultInfo.BankTotalAccount.Value);
                    }
                }
            }

            for (int i = 1; i < 9; i++)
            {
                dbBusinessDataService.Command(db =>
                {
                    business_Reconciliations = db.Queryable<v_Business_Reconciliation>().Where(c => c.Status == "3").ToList();
                });
                if (business_Reconciliations.Count == 0)
                {
                    break;
                }
                foreach (v_Business_Reconciliation business_Reconciliation in business_Reconciliations)
                {
                    string bankDate = business_Reconciliation.BankBillDate.Value.ToString("yyyy-MM-dd");
                    DateTime revenueDateData = business_Reconciliation.BankBillDate.Value.AddDays(-1);
                    string revenueDate = revenueDateData.ToString("yyyy-MM-dd");
                    for (int k = 1; k < i; k++)
                    {
                        revenueDate += "," + revenueDateData.AddDays(-k).ToString("yyyy-MM-dd");
                    }
                    ResultModel<usp_GetTotalAmount> resultData = GetTotalAmount(dbBusinessDataService, bankDate, revenueDate, business_Reconciliation.Channel_Id);
                    if (resultData != null)
                    {
                        RevenuepaymentReconciliation(
                            dbBusinessDataService,
                            "admin",
                            business_Reconciliation.BankBillDate.Value,
                            revenueDate,
                            business_Reconciliation.Channel_Id,
                            resultData.ResultInfo.RevenueSystemTotalAccount.Value,
                            resultData.ResultInfo.RevenueArrearsTotalAccount.Value,
                            resultData.ResultInfo.RevenuePaymentTotalAccount.Value,
                            resultData.ResultInfo.T1DataArrearsTotalAccount.Value,
                            resultData.ResultInfo.T1DataPaymentTotalAccount.Value,
                            resultData.ResultInfo.DepositArrearsTotalAccount.Value,
                            resultData.ResultInfo.DepositPaymentTotalAccount.Value,
                            resultData.ResultInfo.BankTotalAccount.Value);
                    }
                }
            }

        }
        /// <summary>
        /// 获取对账数据，包含营收系统总额，营收总额、T+1总额，银行总额
        /// </summary>
        /// <param name="date"></param>
        /// <param name="Channel_Id"></param>
        /// <returns></returns>
        public static ResultModel<usp_GetTotalAmount> GetTotalAmount(DbBusinessDataService DbBusinessDataService, string BankDate, string RevenueDate, string Channel_Id)
        {
            var resultModel = new ResultModel<usp_GetTotalAmount>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var outputResult = db.Ado.UseStoredProcedure<dynamic>(() =>
                {
                    string spName = "usp_GetTotalAmount";

                    var p1 = new SugarParameter("@BankDate", BankDate);
                    var p2 = new SugarParameter("@PayDate", RevenueDate);
                    var p3 = new SugarParameter("@Channel_Id", Channel_Id);
                    resultModel.ResultInfo = db.Ado.SqlQuerySingle<usp_GetTotalAmount>(spName, new SugarParameter[] { p1, p2, p3 });
                    resultModel.IsSuccess = true;
                    return resultModel;
                });
            });
            resultModel.ResultInfo.RevenueSystemTotalAccount = GetRevenueSystemAmount(DbBusinessDataService, BankDate, RevenueDate, Channel_Id, resultModel.ResultInfo.RevenueArrearsTotalAccount.ToString());
            return resultModel;
        }

        /// <summary>
        /// 获取应收系统金额
        /// </summary>
        /// <param name="revenuepayments">营收数据</param>
        /// <returns></returns>
        public static decimal GetRevenueSystemAmount(DbBusinessDataService DbBusinessDataService, string BankDate, string revenueDate, string channel_Id, string revenueAmount)
        {
            decimal total = 0;
            string revenueVguid = string.Empty;
            //DateTime sDataTime = DateTime.Parse(date + " 00:00:00");
            //DateTime eDataTime = DateTime.Parse(date + " 23:59:59");
            List<V_Revenuepayment_Information_Date> revenuepayments = new List<V_Revenuepayment_Information_Date>();
            T_Channel channel = new T_Channel();
            DbBusinessDataService.Command(db =>
            {
                revenueDate.Replace(" ", "");
                string[] revenueDates = revenueDate.Split(",", StringSplitOptions.RemoveEmptyEntries);
                var query = db.Queryable<V_Revenuepayment_Information_Date>().Where(i => i.Channel_Id == channel_Id).In(i => i.PayDateStr, revenueDates);
                revenuepayments = query.ToList();
                channel = db.Queryable<T_Channel>().Where(c => c.Id == channel_Id).ToList()[0];
            });

            foreach (var revenuepayment in revenuepayments)
            {
                revenueVguid += string.Format("\"{0}\",", revenuepayment.VGUID.ToString());
            }
            if (!string.IsNullOrEmpty(revenueVguid))
            {
                revenueVguid = revenueVguid.Remove(revenueVguid.Length - 1, 1);
                string data = "{"
                            + " \"ReceiptCategory\":{0},".Replace("{0}", channel.PaymentEncoding)
                            + " \"ReconciliationsDate\":\"{0}\",".Replace("{0}", BankDate)
                            + " \"TotalAmount\":\"{0}\",".Replace("{0}", revenueAmount)
                            + " \"ClearingPlatformReconciliations\":[{0}]".Replace("{0}", revenueVguid)
                            + " }";
                try
                {
                    WebClient wc = new WebClient();
                    string result = wc.UploadString(ConfigSugar.GetAppString("RevenueSystemTotalPath"), data);
                    object obj = result.JsonToModel<object>();
                    bool success = Convert.ToBoolean(((Dictionary<string, object>)obj)["success"]);
                    if (success)
                    {
                        object val = ((Dictionary<string, object>)((Dictionary<string, object>)obj)["data"])["TotalAmount"];
                        total = Convert.ToDecimal(val);
                    }
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, result));
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
                }
            }
            return total;
        }

        public static ResultModel<string> RevenuepaymentReconciliation(
            DbBusinessDataService DbBusinessDataService,
            string loginName,
            DateTime BankDate,
            string RevenueDate,
            string Channel_Id,
            decimal RevenueSystemTotal,
            decimal ArrearsRevenueTotal,
            decimal RevenueTotal,
            decimal ArrearsChannelTotal,
            decimal ChannelTotal,
            decimal ArrearsDepositTotal,
            decimal DepositTotal,
            decimal BankTotal)
        {

            var resultModel = Validate(RevenueSystemTotal, ArrearsRevenueTotal, RevenueTotal, ArrearsChannelTotal, ChannelTotal, ArrearsDepositTotal, DepositTotal, BankTotal);
            bool isAccountingRevenue = true;
            string batchBillNo = "";
            if (resultModel.IsSuccess)
            {
                isAccountingRevenue = AccountingRevenueSystem(DbBusinessDataService, loginName, BankDate, RevenueDate, Channel_Id, ref batchBillNo);
            }
            if (!isAccountingRevenue)
            {
                resultModel.IsSuccess = false;
                resultModel.ResultInfo = "营收系统入账错误！";
            }
            Business_Reconciliation reconciliation = null;
            DbBusinessDataService.Command(db =>
            {
                var data = db.Queryable<Business_Reconciliation>().Where(c => c.BankBillDate == BankDate && c.Channel_Id == Channel_Id).ToList();
                if (data.Count == 1)
                {
                    reconciliation = data[0];
                }
            });
            if (reconciliation == null)
            {
                reconciliation = new Business_Reconciliation();
                reconciliation.VGUID = Guid.NewGuid();
                reconciliation.CreatedDate = DateTime.Now;
                reconciliation.CreatedUser = loginName;
            }
            else
            {
                reconciliation.ChangeDate = DateTime.Now;
                reconciliation.ChangeUser = loginName;
            }
            reconciliation.Channel_Id = Channel_Id;
            reconciliation.BankBillDate = BankDate;
            reconciliation.BankBillTotalAmount = BankTotal;
            reconciliation.ReconciliationDate = DateTime.Now;
            reconciliation.ReconciliationUser = loginName;
            reconciliation.BatchBillNo = batchBillNo;
            reconciliation.AbnormalReason = resultModel.ResultInfo;
            reconciliation.Status = "2";
            if (!resultModel.IsSuccess)
            {
                reconciliation.Status = "3";
            }
            List<Business_ReconciliationDetail> reconciliationDetails = new List<Business_ReconciliationDetail>();
            RevenueDate = RevenueDate.Replace(" ", "");
            string[] revenueDates = RevenueDate.Split(",", StringSplitOptions.RemoveEmptyEntries);
            foreach (string date in revenueDates)
            {
                Business_ReconciliationDetail reconciliationDetail = new Business_ReconciliationDetail();
                reconciliationDetail.VGUID = Guid.NewGuid();
                reconciliationDetail.Business_ReconciliationVGUID = reconciliation.VGUID;
                reconciliationDetail.RevenueDate = Convert.ToDateTime(date);
                reconciliationDetail.CreatedDate = DateTime.Now;
                reconciliationDetail.CreatedUser = loginName;
                reconciliationDetail.ChangeDate = DateTime.Now;
                reconciliationDetail.ChangeUser = loginName;
                reconciliationDetails.Add(reconciliationDetail);
            }

            DbBusinessDataService.Command(db =>
            {
                db.Ado.UseTran(delegate
                {
                    db.Deleteable<Business_Reconciliation>(c => c.BankBillDate == BankDate && c.Channel_Id == Channel_Id).ExecuteCommand();
                    db.Deleteable<Business_ReconciliationDetail>(c => c.Business_ReconciliationVGUID == reconciliation.VGUID).ExecuteCommand();
                    if (reconciliation.VGUID != null)
                    {
                        db.Insertable(reconciliation).ExecuteCommand();
                    }
                    db.Insertable(reconciliationDetails).ExecuteCommand();
                });
            });
            return resultModel;
        }

        private static bool AccountingRevenueSystem(
            DbBusinessDataService DbBusinessDataService,
            string loginName,
            DateTime BankDate,
            string RevenueDate,
            string Channel_Id,
            ref string batchBillNo
            )
        {
            bool surccss = false;
            List<usp_GetSubjectAmount> usp_GetSubjectAmounts = new List<usp_GetSubjectAmount>();
            T_Channel channel = new T_Channel();
            DbBusinessDataService.Command(db =>
            {
                channel = db.Queryable<T_Channel>().Where(c => c.Id == Channel_Id).ToList()[0];
            });
            DbBusinessDataService.Command(db =>
            {
                var outputResult = db.Ado.UseStoredProcedure<dynamic>(() =>
                {
                    string spName = "usp_GetSubjectAmount";
                    var p1 = new SugarParameter("@PayDate", RevenueDate);
                    var p2 = new SugarParameter("@Channel_Id", Channel_Id);
                    usp_GetSubjectAmounts = db.Ado.SqlQuery<usp_GetSubjectAmount>(spName, new SugarParameter[] { p1, p2 });

                    return "";
                });
            });
            string subjects = "";
            foreach (var usp_GetSubjectAmount in usp_GetSubjectAmounts)
            {
                string subject = "{"
                                  + "\"Subject_ID\":\"{0}\",".Replace("{0}", usp_GetSubjectAmount.SubjectId)
                                  + "\"RecordCount\":{0},".Replace("{0}", usp_GetSubjectAmount.SubjectCounts.ToString())
                                  + "\"TotalAmount\":{0}".Replace("{0}", usp_GetSubjectAmount.SubjectAmount.ToString())
                                  + "},";
                subjects += subject;
            }
            subjects = subjects.Remove(subjects.Length - 1, 1);
            string data = "{"
                           + " \"ReceiptCategory\":{0},".Replace("{0}", channel.PaymentEncoding)
                           + " \"Channel\":\"{0}\",".Replace("{0}", Channel_Id)
                           + " \"ReconciliationsDate\":\"{0}\",".Replace("{0}", BankDate.ToString("yyyy-MM-dd HH:mm:ss"))
                           + " \"RecordCount\":{0},".Replace("{0}", usp_GetSubjectAmounts[0].Counts.ToString())
                           + " \"TotalAmount\":{0},".Replace("{0}", usp_GetSubjectAmounts[0].RevenuepaymentTotal.ToString())
                           + " \"Operator\":\"{0}\",".Replace("{0}", loginName)
                           + " \"Subject\":[{0}]".Replace("{0}", subjects)
                           + " }";
            try
            {
                WebClient wc = new WebClient();
                string result = wc.UploadString(ConfigSugar.GetAppString("RevenueSystemAccountingPath"), data);
                object obj = result.JsonToModel<object>();
                Dictionary<string, object> dicList = (Dictionary<string, object>)obj;
                object val = dicList["success"];
                surccss = Convert.ToBoolean(val);
                if (dicList.ContainsKey("batchbillno"))
                {
                    batchBillNo = ((Dictionary<string, object>)obj)["batchbillno"].ToString();
                }
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, result));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
            }
            return surccss;
        }



        private static ResultModel<string> Validate(
            decimal RevenueSystemTotal,
            decimal ArrearsRevenueTotal,
            decimal RevenueTotal,
            decimal ArrearsChannelTotal,
            decimal ChannelTotal,
            decimal ArrearsDepositTotal,
            decimal DepositTotal,
            decimal BankTotal)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0", ResultInfo = "对账成功！" };

            if (RevenueSystemTotal != ArrearsRevenueTotal)
            {
                resultModel.ResultInfo = "营收数据与营收系统数据不一致！";
                return resultModel;
            }
            if (RevenueTotal != ChannelTotal || ArrearsRevenueTotal != ArrearsChannelTotal)
            {
                resultModel.ResultInfo = "营收数据与T+1数据不一致！";
                return resultModel;
            }
            if (RevenueTotal + DepositTotal != BankTotal)
            {
                resultModel.ResultInfo = "营收数据与银行数据不一致！";
                return resultModel;
            }
            resultModel.IsSuccess = true;
            return resultModel;
        }

    }
}