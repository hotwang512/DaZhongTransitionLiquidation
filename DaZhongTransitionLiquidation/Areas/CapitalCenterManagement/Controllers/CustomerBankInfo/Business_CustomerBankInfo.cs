using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo
{
    public class Business_CustomerBankInfo
    {
        public Guid VGUID { get; set; }
        public string CompanyOrPerson { get; set; }
        public string BankAccount { get; set; }
        public string BankAccountName { get; set; }
        public string Bank { get; set; }
        public string BankNo { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Founder { get; set; }
    }

    public class v_BankInfoSetting
    {
        public Guid VGUID { get; set; }
        public string CompanyOrPerson { get; set; }
        public string BankAccount { get; set; }
        public string BankAccountName { get; set; }
        public string Bank { get; set; }
        public string BankNo { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Founder { get; set; }
        public string IsCheck { get; set; }
    }
}