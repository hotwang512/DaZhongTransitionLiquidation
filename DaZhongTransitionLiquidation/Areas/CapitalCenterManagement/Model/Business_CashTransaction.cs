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
        /// Desc:支付日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? TransactionDate { get; set; }
        /// <summary>
        /// Desc:支付金额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? TurnOut { get; set; }
        /// <summary>
        /// Desc:可用余额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? UseBalance { get; set; }
        /// <summary>
        /// Desc:剩余余额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? Balance { get; set; }
        /// <summary>
        /// Desc:报销人员
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ReimbursementMan { get; set; }
        /// <summary>
        /// Desc:报销部门
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ReimbursementOrgName { get; set; }
        /// <summary>
        /// Desc:报销部门Code
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ReimbursementOrgCode { get; set; }
        /// <summary>
        /// Desc:用途
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Purpose { get; set; }
        /// <summary>
        /// Desc:交易流水号
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
        public string AccountModeCode { get; set; }
        public string AccountModeName { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
    }
}