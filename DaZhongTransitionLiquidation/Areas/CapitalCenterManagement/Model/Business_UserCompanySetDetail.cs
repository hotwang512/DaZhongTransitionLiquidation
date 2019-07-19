using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class Business_UserCompanySetDetail
    {
        public Guid VGUID { get; set; }
        public bool Isable { get; set; }
        public string PayBank { get; set; }
        public string PayAccount { get; set; }
        public string PayBankAccountName { get; set; }
        public string AccountType { get; set; }
        public string Borrow { get; set; }
        public string Loan { get; set; }
        public string KeyData { get; set; }
        public string OrderVGUID { get; set; }
        public string AccountModeCode { get; set; }
        public string AccountModeName { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
    }
}