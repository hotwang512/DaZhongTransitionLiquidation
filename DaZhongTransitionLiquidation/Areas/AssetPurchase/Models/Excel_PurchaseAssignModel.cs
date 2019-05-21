using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Models
{
    public class Excel_PurchaseAssignModel
    {
        public string EngineNumber { get; set; }
        public string ChassisNumber { get; set; }
        public string AssetManagementCompany { get; set; }
        public string BelongToCompany { get; set; }
        public int AssetNum { get; set; }
    }
}