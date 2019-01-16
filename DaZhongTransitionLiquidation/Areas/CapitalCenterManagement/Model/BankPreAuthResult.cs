using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class BankPreAuthResult
    {
        public bool success { get; set; }
        public string errmsg { get; set; }
        public BankPreAuthResultData data { get; set; }
    }
    public class BankPreAuthResultData
    {
        public string serialNo { get; set; }
    }
}