using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity
{
    public class usp_GetTotalAmount
    {
        public decimal? RevenueArrearsTotalAccount { get; set; }
        public decimal? RevenuePaymentTotalAccount { get; set; }
        public decimal? RevenueSystemTotalAccount { get; set; }

        public decimal? T1DataArrearsTotalAccount { get; set; }
        public decimal? T1DataPaymentTotalAccount { get; set; }

        public decimal? DepositArrearsTotalAccount { get; set; }
        public decimal? DepositPaymentTotalAccount { get; set; }

        public decimal? BankTotalAccount { get; set; }
    }
}
