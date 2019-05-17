using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;

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
    public class JsonResultModelApi<T>
    {
        public T[] data { get; set; }
        public int code { get; set; }
        public string message { get; set; }
    }
}
