using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class VehicleCheckShowReport
    {
        public int Sort { get; set; }
        public string PeriodType { get; set; }
        public string CompanyType { get; set; }
        public string Company { get; set; }

        private List<VehicleModelData> VehicleModelList = new List<VehicleModelData>();
        public List<VehicleModelData> ResultVehicleModelList { get { return VehicleModelList; } }
    }
    public class VehicleModelData
    {
        public string VehicleModel { get; set; }
        public int? Quantity { get; set; }   
    }
}