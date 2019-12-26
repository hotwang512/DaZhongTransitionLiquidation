using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models
{
    public class LicenseAmountModel
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string Period { get; set; }
        public int LicenseAmount { get; set; }
    }
}