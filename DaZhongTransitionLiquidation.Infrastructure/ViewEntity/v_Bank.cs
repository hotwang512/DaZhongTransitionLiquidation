using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class v_Bank
    {
        public decimal? bankamount { get; set; }
        public DateTime? BankDate { get; set; }
        public string Channel_Id { get; set; }
        public string Channel_name { get; set; }
        public DateTime? OperationTime { get; set; }
        public string Operator { get; set; }
        public decimal? PaymentAmount { get; set; }
        public decimal? Remitamount { get; set; }
        public string Status { get; set; }
        public string StatusDESC { get; set; }

    }
}
