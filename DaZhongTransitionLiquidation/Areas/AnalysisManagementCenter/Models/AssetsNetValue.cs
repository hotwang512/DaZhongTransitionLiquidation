using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models
{
    public class AssetsNetValue
    {
        public string YearMonth { get; set; }
        public string MAJOR { get; set; }
        public string MINOR { get; set; }
        public string VMODEL { get; set; }
        public string ASSETCOUNT { get; set; }
        public decimal COST { get; set; }
        public decimal ACCT { get; set; }
        public decimal DEVALUE { get; set; }

        public decimal NETALUE
        {
            get { return COST - ACCT; }
        }

    }
}