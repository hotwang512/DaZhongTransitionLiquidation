using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity
{
    public class U_Business_Revenuepayment_Information2
    {
        public decimal? ActualAmount { get; set; }
        public string Channel_Id { get; set; }
        public decimal? CompanyAccount { get; set; }
        public decimal? copeFee { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateUser { get; set; }
        public string JobNumber { get; set; }
        public string Name { get; set; }
        public string Parameter1 { get; set; }
        public string Parameter2 { get; set; }
        public string Parameter3 { get; set; }
        public DateTime? PayDate { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string PaymentBrokers { get; set; }
        public string PaymentStatus { get; set; }
        public string Subject_Id { get; set; }
        public string TransactionID { get; set; }
        public Guid VGUID { get; set; }


    }
}
