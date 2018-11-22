using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.FundReconciliation
{
    public class Business_FundReconciliation
    {
        public Guid VGUID { get; set; }
        /// <summary>
        /// Desc:银行余额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? BankBalance { get; set; }
        /// <summary>
        /// Desc:余额日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? BalanceDate { get; set; }
        /// <summary>
        /// Desc:对账人
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Reconcilianter { get; set; }
        /// <summary>
        /// Desc:对账日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? ReconciliantDate { get; set; }
        /// <summary>
        /// Desc:对账状态
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ReconciliantStatus { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string BankAccount { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
    }
}