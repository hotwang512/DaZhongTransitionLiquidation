using System;

namespace DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity
{
    public class U_NextDay_Search
    {
        public string TransactionID { get; set; }

        public string Channel_Id { get; set; }
        public string Channel_Id2 { get; set; }
        public string PayDateFrom { get; set; }

        public string PayDateTo { get; set; }

    }
}