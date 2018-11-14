using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity
{
    public class JsonResultModel<T>
    {
        public List<T> Rows { get; set; }
        public int TotalRows { get; set; }

        public string Parameter { get; set; }
        public string Parameter2 { get; set; }
    }

    public class JsonResultModelExt<T> : JsonResultModel<T>
    {
        public decimal SumPaymentAmount { get; set; }

        public decimal SumRemitamount { get; set; }
    }
}
