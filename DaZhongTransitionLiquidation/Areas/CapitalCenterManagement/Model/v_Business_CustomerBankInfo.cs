using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class v_Business_CustomerBankInfo
    {
        public Guid VGUID { get; set; }
        public string CompanyOrPerson { get; set; }
        public string BankAccount { get; set; }
        public string BankAccountName { get; set; }
        public string Bank { get; set; }
        public string BankNo { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Founder { get; set; }
        public bool Isable { get; set; }
        public string OrderVGUID { get; set; }
    }
}