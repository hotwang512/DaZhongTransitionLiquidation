using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Models
{
    public class Excel_PurchaseOBDAssignModel
    {
        public string PlateNumber { get; set; }
        public string EquipmentNumber { get; set; }
    }
    public class Excel_PurchaseOBDAssignCompare
    {
        public string PlateNumber_EquipmentNumber { get; set; }
    }
}