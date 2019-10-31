using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherList
{
    public class Business_VoucherList
    {
        public Guid VGUID { get; set; }
        /// <summary>
        /// Desc:营运公司Code
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CompanyCode { get; set; }
        /// <summary>
        /// Desc:营运公司Name
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CompanyName { get; set; }
        /// <summary>
        /// Desc:会计期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? AccountingPeriod { get; set; }
        /// <summary>
        /// Desc:币种
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Currency { get; set; }
        /// <summary>
        /// Desc:批名
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BatchName { get; set; }
        /// <summary>
        /// Desc:凭证号码
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VoucherNo { get; set; }
        /// <summary>
        /// Desc:凭证日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? VoucherDate { get; set; }
        /// <summary>
        /// Desc:凭证类型
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VoucherType { get; set; }
        /// <summary>
        /// Desc:借方金额合计
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? DebitAmountTotal { get; set; }
        /// <summary>
        /// Desc:贷方金额合计
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? CreditAmountTotal { get; set; }
        /// <summary>
        /// Desc:财务主管
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string FinanceDirector { get; set; }
        /// <summary>
        /// Desc:记账
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Bookkeeping { get; set; }
        /// <summary>
        /// Desc:审核
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Auditor { get; set; }
        /// <summary>
        /// Desc:制单
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string DocumentMaker { get; set; }
        /// <summary>
        /// Desc:出纳
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Cashier { get; set; }
        /// <summary>
        /// Desc:审核状态
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Status { get; set; }
        public string AttachmentDetail { get; set; }
        public DateTime? CreateTime { get; set; }
        public string AccountModeName { get; set; }
        public string Automatic { get; set; }
        public string OracleStatus { get; set; }
        public string OracleMessage { get; set; }
        public string TradingBank { get; set; }
        public string ReceivingUnit { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string Batch { get; set; }
    }
}