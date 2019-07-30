using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class Business_SettlementCount
    {
        public Guid VGUID { get; set; }
        public string Model { get; set; }
        public string ClassType { get; set; }
        public string CarType { get; set; }
        public string Business { get; set; }
        public string BusinessKey { get; set; }
        public string BusinessType { get; set; }
        public string YearMonth { get; set; }
        public string DAYS { get; set; }
        public decimal? Money { get; set; }
        public decimal? Account { get; set; }
        public string Founder { get; set; }
        public DateTime? CreatTime { get; set; }
    }
}