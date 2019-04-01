using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    public class v_BankPurchaseSupplier
    {
        public Guid PurchaseOrderSettingVguid { get; set; }
        public string CompanyOrPerson { get; set; }
        public string BankAccount { get; set; }
        public string BankAccountName { get; set; }
        public string Bank { get; set; }
        public string BankNo { get; set; }
        public string Founder { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}