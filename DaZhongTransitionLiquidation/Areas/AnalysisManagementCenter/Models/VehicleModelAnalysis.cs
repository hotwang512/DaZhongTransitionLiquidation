using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models
{
    public class VehicleModelAnalysis
    {
        public string CompanyType { get; set; }
        public string CompanyName { get; set; }
        public string YearMonth { get; set; }
        public string CompanyID { get; set; }
        public string VehicleID { get; set; }
        public string VehicleModel { get; set; }
        public int Quantity { get; set; }
    }
}