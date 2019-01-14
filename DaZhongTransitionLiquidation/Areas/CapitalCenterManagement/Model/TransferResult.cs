using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class TransferResult
    {
        public bool success { get; set; }
        public string errmsg { get; set; }
        public TransferResultData data { get; set; }
    }
    public class TransferResultData
    {
        public string serialNo { get; set; }
        public string T24D { get; set; }
        public string RECO { get; set; }
        public string REMG { get; set; }
        public string T24S { get; set; }
    }
}