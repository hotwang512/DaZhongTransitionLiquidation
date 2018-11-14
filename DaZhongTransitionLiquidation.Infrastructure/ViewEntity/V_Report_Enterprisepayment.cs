using System;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class V_Report_Enterprisepayment
    {
        public string Channel_Id { get; set; }
        public string ChannelName { get; set; }
        public string Department { get; set; }
        public string name { get; set; }
        public string OrganizationName { get; set; }
        public DateTime? PayDate { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string Reason { get; set; }
        public bool? ReasonStatus { get; set; }
        public string Reconciliationstate { get; set; }
        public decimal? Remitamount { get; set; }
        public DateTime? Revenuetime { get; set; }
        public string TransactionID { get; set; }
        public Guid VGUID { get; set; }

    }
}