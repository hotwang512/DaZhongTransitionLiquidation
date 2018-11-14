using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity
{
    public class U_RevenuePayment_Search
    {
        public string Name { get; set; }

        public string IDNumber { get; set; }

        public string JobNumber { get; set; }

        public string PhoneNumber { get; set; }

        public string TransactionID { get; set; }

        public string Department { get; set; }

        public string ReasonStatus { get; set; }
        public string Level { get; set; }

        public string Channel { get; set; }

        public string Status { get; set; }
        public string PayDateFrom { get; set; }

        public string PayDateTo { get; set; }

        public string PaymentStatus { get; set; }
    }
}
