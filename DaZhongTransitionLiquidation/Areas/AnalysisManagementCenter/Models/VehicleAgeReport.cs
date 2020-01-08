using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models
{
    public class VehicleAgeReport
    {
        public string FA_LOC_1 { get; set; }
        public string FA_LOC_2 { get; set; }
        public string DESCRIPTION { get; set; }
        public string PERIOD_CODE { get; set; }
        public string MONTHS { get; set; }
        public int QUANTITY { get; set; }

    }
}