using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class AttachmentResult
    {
        public bool success { get; set; }
        public string message { get; set; }
        public AttachmentResultData data { get; set; }
    }
    public class AttachmentResultData
    {
        public List<string> PaymentReceipt { get; set; }
        public List<string> InvoiceReceipt { get; set; }
        public List<string> ApprovalReceipt { get; set; }
        public List<string> Contract { get; set; }
        public List<string> DetailList { get; set; }
        public List<string> OtherReceipt { get; set; }
    }
}