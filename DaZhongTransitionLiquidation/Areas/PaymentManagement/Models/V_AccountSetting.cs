using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Models
{
    public class V_AccountSetting
    {
        public Guid VGUID { get; set; }
        public string BankName { get; set; }
        public string BankAccount { get; set; }
        public string CompanyCode { get; set; }
        public string BankAccountName { get; set; }
        public string AccountType { get; set; }
        public bool IsChecked { get; set; }
        public string CompanyName { get; set; }
    }
}