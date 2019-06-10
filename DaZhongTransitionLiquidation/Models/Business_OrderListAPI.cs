using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Models
{
    public class Business_OrderListAPI
    {
        public string PaymentCompany { get; set; }
        public string CollectBankAccountName { get; set; }
        public string CollectBankAccouont { get; set; }
        public string CollectBankName { get; set; }
        public string CollectBankNo { get; set; }
        public string PaymentMethod { get; set; }
        public string ServiceCategory { get; set; }
        public string BusinessProject { get; set; }
        public string Amount { get; set; }
        public string Sponsor { get; set; }
        public string Summary { get; set; }
        public int invoiceNumber { get; set; }
        public int numberOfAttachments { get; set; }
        public string AccountSetCode { get; set; }
        public string VoucherType { get; set; }
    }
}