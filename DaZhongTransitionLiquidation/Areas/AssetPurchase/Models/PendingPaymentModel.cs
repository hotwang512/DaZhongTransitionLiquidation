using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Models
{
    public class PendingPaymentModel
    {
        public string IdentityToken { get; set; }
        public string FunctionSiteId { get; set; }
        public string OperatorIP { get; set; }
        public string ServiceCategory { get; set; }
        public string BusinessProject { get; set; }
        public string invoiceNumber { get; set; }
        public string numberOfAttachments { get; set; }
        public string Amount { get; set; }
        public string Summary { get; set; }
        public string PaymentReceipt { get; set; }
        public string InvoiceReceipt { get; set; }
        public string ApprovalReceipt { get; set; }
        public string Contract { get; set; }
        public string DetailList { get; set; }
        public string OtherReceipt { get; set; }
        public string PaymentCompany { get; set; }
        public string CollectBankAccountName { get; set; }
        public string CollectBankAccouont { get; set; }
        public string CollectBankName { get; set; }
        public string CollectBankNo { get; set; }
        public string PaymentMethod { get; set; }
    }
}