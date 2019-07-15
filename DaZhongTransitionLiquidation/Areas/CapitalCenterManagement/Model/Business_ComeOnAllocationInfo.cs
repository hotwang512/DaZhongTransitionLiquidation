using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class Business_ComeOnAllocationInfo
    {
        public Guid VGUID { get; set; }
        public string No { get; set; }
        public string OrderNo { get; set; }
        public string TurnInAccountModeName { get; set; }
        public string TurnInAccountModeCode { get; set; }
        public string TurnOutAccountModeName { get; set; }
        public string TurnOutAccountModeCode { get; set; }
        public DateTime? ApplyDate { get; set; }
        public string TurnInCompanyName { get; set; }
        public string TurnInCompanyCode { get; set; }
        public string TurnOutCompanyName { get; set; }
        public string TurnOutCompanyCode { get; set; }
        public decimal? Money { get; set; }
        public decimal? TurnInMoney { get; set; }
        public string Remark { get; set; }
        public string Cashier { get; set; }
        public string Auditor { get; set; }
        public string Founder { get; set; }
        public string Status { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Changer { get; set; }
        public DateTime? ChangeTime { get; set; }
    }
}