using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity
{
    public class Api_ResultModel
    {
    }
    public class JsonResultFileModelApi<T>
    {
        public T[] data { get; set; }
        public int code { get; set; }
        public string message { get; set; }
    }
    public class JsonResultModelApi<T>
    {
        public T data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }
    public class JsonResultListApi<T>
    {
        public List<T> data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }
}
