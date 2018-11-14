using System;

namespace DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity
{
    public class usp_Revenuepayment_Reconciliation
    {
        public string Name { get; set; }
        /// <summary>
        /// 实际付款
        /// </summary>
        public decimal? ActualAmount { get; set; }

        /// <summary>
        /// 到账金额
        /// </summary>
        public decimal? Remitamount { get; set; }

        public DateTime? PayDate { get; set; }

        public string Reason { get; set; }

    }
}