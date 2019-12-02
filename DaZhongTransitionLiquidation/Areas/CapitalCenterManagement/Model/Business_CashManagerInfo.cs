using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class Business_CashManagerInfo
    {
        public Guid VGUID { get; set; }
        public string No { get; set; }
        public string AccountModeName { get; set; }
        public string CompanyName { get; set; }
        public string AccountModeCode { get; set; }
        public string CompanyCode { get; set; }
        public DateTime? ApplyDate { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccount { get; set; }
        public string BankName { get; set; }
        public decimal? Money { get; set; }
        public string CheckNo { get; set; }
        public string Remark { get; set; }
        public string Cashier { get; set; }
        public string Auditor { get; set; }
        public string Founder { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Changer { get; set; }
        public DateTime? ChangeTime { get; set; }
        public string Status { get; set; }
        public DateTime? CashTime { get; set; }
    }
}