using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Models
{
    public class Excel_PurchaseAssignModel
    {
        public string VehicleModel { get; set; }
        public string EngineNumber { get; set; }
        public string ChassisNumber { get; set; }
        public string AssetManagementCompany { get; set; }
        public string BelongToCompany { get; set; }
        public string UseDepartment { get; set; }
        public string StartVehicleDate { get; set; }
    }

    public class Excel_PurchaseAssignCompare
    {
        public string EngineNumber_ChassisNumber { get; set; }
    }
}