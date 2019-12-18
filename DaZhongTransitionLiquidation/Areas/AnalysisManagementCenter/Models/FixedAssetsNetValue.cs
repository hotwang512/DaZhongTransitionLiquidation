using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models
{
    public class FixedAssetsNetValue
    {
        public string MainCategory { get; set; }
        public string CategoryType { get; set; }
        public string CalculationType { get; set; }
        public decimal StartPeriod { get; set; }
        public decimal AddedPeriod { get; set; }
        public decimal ReducePeriod { get; set; }

        public decimal EndPeriod
        {
            get { return StartPeriod + AddedPeriod - ReducePeriod; }
        }

        public int Zorder { get; set; }
    }
    public class FixedAssetsNetValueModel
    {
        public string MAJOR { get; set; }
        public string PeriodType { get; set; }
        public decimal COST { get; set; }
        public decimal ACCT { get; set; }
    }
}