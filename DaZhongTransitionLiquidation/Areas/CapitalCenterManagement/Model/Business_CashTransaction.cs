using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class Business_CashTransaction
    {
        public Guid VGUID { get; set; }
        /// <summary>
        /// Desc:交易银行
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string TradingBank { get; set; }
        /// <summary>
        /// Desc:交易日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? TransactionDate { get; set; }
        /// <summary>
        /// Desc:转出（借)
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? TurnOut { get; set; }
        /// <summary>
        /// Desc:转入（贷）
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? TurnIn { get; set; }
        /// <summary>
        /// Desc:币种
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Currency { get; set; }
        /// <summary>
        /// Desc:我方单位
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string PaymentUnit { get; set; }
        /// <summary>
        /// Desc:我方账号
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string PayeeAccount { get; set; }
        /// <summary>
        /// Desc:我方开户机构
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string PaymentUnitInstitution { get; set; }
        /// <summary>
        /// Desc:对方单位名称
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ReceivingUnit { get; set; }
        /// <summary>
        /// Desc:对方账号
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ReceivableAccount { get; set; }
        /// <summary>
        /// Desc:对方开户机构
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ReceivingUnitInstitution { get; set; }
        /// <summary>
        /// Desc:用途
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Purpose { get; set; }
        /// <summary>
        /// Desc:备注
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Remark { get; set; }
        /// <summary>
        /// Desc:T24交易流水号
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Batch { get; set; }
        /// <summary>
        /// Desc:凭证科目
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VoucherSubject { get; set; }
        /// <summary>
        /// Desc:凭证摘要
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VoucherSummary { get; set; }
        public string VoucherSubjectName { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreatePerson { get; set; }
        /// <summary>
        /// Desc:余额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? Balance { get; set; }
        public string BankAccount { get; set; }
        public string AccountModeCode { get; set; }
        public string AccountModeName { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
    }
}