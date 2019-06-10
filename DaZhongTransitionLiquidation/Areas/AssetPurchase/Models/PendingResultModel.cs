using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Models
{
    public class PendingResultModel
    {
        public string code { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }
}