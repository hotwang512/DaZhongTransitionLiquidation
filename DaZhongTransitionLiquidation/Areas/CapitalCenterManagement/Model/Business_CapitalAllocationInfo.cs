using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class Business_CapitalAllocationInfo
    {
        public Guid VGUID { get; set; }
        public string No { get; set; }
        public string TurnInAccountModeName { get; set; }
        public string TurnInAccountModeCode { get; set; }
        public string TurnOutAccountModeName { get; set; }
        public string TurnOutAccountModeCode { get; set; }
        public DateTime? ApplyDate { get; set; }
        public string TurnInCompanyName { get; set; }
        public string TurnInCompanyCode { get; set; }
        public string TurnInBankAccount { get; set; }
        public string TurnInBankName { get; set; }
        public string TurnOutCompanyName { get; set; }
        public string TurnOutCompanyCode { get; set; }
        public string TurnOutBankAccount { get; set; }
        public string TurnOutBankName { get; set; }
        public decimal? Money { get; set; }
        public string Remark { get; set; }
        public string Cashier { get; set; }
        public string Auditor { get; set; }
        public string Founder { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Changer { get; set; }
        public DateTime? ChangeTime { get; set; }
    }
}