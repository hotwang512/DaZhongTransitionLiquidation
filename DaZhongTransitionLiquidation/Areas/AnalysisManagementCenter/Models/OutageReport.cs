﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models
{
    public partial class OutageReport
    {
        public string VehicleModel { get; set; }
        public string Period { get; set; }
        public string CompanyName { get; set; }
        public int Quantity { get; set; }
    }
}