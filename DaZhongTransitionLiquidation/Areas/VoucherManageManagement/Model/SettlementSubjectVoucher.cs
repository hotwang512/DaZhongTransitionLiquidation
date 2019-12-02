using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class SettlementSubjectVoucher
    {
        public Guid VGUID { get; set; }
        public string AccountModeName { get; set; }
        public string CompanyName { get; set; }
        public string Borrow { get; set; }
        public string Loan { get; set; }
        public string AccountModeCode { get; set; }
        public string CompanyCode { get; set; }
        public string AccountModeNameOther { get; set; }
        public string CompanyNameOther { get; set; }
        public string AccountModeCodeOther { get; set; }
        public string CompanyCodeOther { get; set; }
        public string SettlementVGUID { get; set; }
        public string Remark { get; set; }
        public string BusinessType { get; set; }
        public string BusinessTypeKey { get; set; }
    }
}