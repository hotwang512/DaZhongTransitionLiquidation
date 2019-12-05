using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models
{
    public class VehicleChangeReport
    {
        public string PeriodType { get; set; }
        public string CompanyType { get; set; }
        public string VehicleModel { get; set; }
        public string CompanyName { get; set; }
        public int Quantity { get; set; }
        public string YearMonth { get; set; }
        public int VehicleID { get; set; }
        public int CompanyID { get; set; }
    }
}