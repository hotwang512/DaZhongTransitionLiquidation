using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class v_Business_Data_Total
    {
        public DateTime? OperationTime { get; set; }
        public string Operator { get; set; }
        public DateTime? PayDate { get; set; }
        public decimal? PaymentAmount { get; set; }
        public decimal? Remitamount { get; set; }
        public DateTime? Revenuetime { get; set; }
        public string status { get; set; }

    }
}
