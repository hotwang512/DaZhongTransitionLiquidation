using DaZhongTransitionLiquidation.Areas.ReportManagement.Controllers.ReconciliationReport;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.BankData
{
    public class BankDataPack
    {
        /// <summary>
        /// 银行数据名是否存在
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="bank">银行数据</param>
        /// <param name="isEdit">是否编辑</param>
        /// <returns></returns>
        public bool IsExistBankData(SqlSugarClient db, T_Bank bank, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<T_Bank>().Any(i => i.ArrivedTime == bank.ArrivedTime && i.VGUID != bank.VGUID);
            }
            return db.Queryable<T_Bank>().Any(i => i.ArrivedTime == bank.ArrivedTime);
        }
        public static void SyncBackFlowAndReconciliation()
        {
            SyncBackFlow(DateTime.Now.AddDays(-1));
            ReconciliationReportPack.AutomaticReconciliation();
        }
        /// <summary>
        /// 同步银行流水到银行数据表
        /// </summary>
        public static void SyncBackFlow(DateTime? date)
        {
            string sql = string.Format(@" select 
                                          NEWID() as VGUID,
                                          rb.Bank as ReceiveBank,
                                          rb.BankAccount as ReceiveBankAccount,
                                          rb.BankAccountName as ReceiveBankAccountName,
                                          convert(datetime,convert(varchar(10),f.TransactionDate,20)) as ArrivedTime,
                                          f.TurnOut as ArrivedTotal,
                                          f.ReceivingUnitInstitution as ExpendBank,
                                          f.ReceivableAccount as ExpendBankAccount,
                                          f.ReceivingUnit as ExpendBankAccountName,
                                          m.Channel as Channel_Id,
                                          f.Batch as temp1
                                          from[dbo].[T_BankChannelMapping] m
                                          left join [Business_BankFlowTemplate] f on m.BankAccount = f.PayeeAccount or m.BankAccount = f.ReceivableAccount
                                          left join [dbo].[T_ReceiveBank] rb on f.ReceivableAccount=rb.BankAccount or rb.BankAccount = f.PayeeAccount
                                          where f.VGUID is not null and (m.IsShow != '1' or m.IsShow is null) and f.TransactionDate>'{0}'", date.ObjToString("yyyy-MM-dd"));
            List<T_Bank> bankFlows = new List<T_Bank>();
            DbBusinessDataService dbBusinessDataService = new DbBusinessDataService();
            dbBusinessDataService.Command(db =>
            {
                bankFlows = db.SqlQueryable<T_Bank>(sql).ToList();
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", sql, bankFlows.ModelToJson()));
            });
            if (bankFlows.Count > 0)
            {
                foreach (var bankFlow in bankFlows)
                {
                    dbBusinessDataService.Command(db =>
                    {
                        var exist = db.Queryable<T_Bank>().Any(c => c.temp1 == bankFlow.temp1);
                        if (!exist)
                        {
                            bankFlow.VCRTTIME = DateTime.Now;
                            bankFlow.VCRTUSER = "admin";
                            db.Insertable(bankFlow).ExecuteCommand();
                        }
                    });
                }
            }
        }


    }
}