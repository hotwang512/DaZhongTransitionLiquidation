using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class SettlementCountList
    {
        public Guid VGUID { get; set; }
        public string Model { get; set; }
        public string ClassType { get; set; }
        public string CarType { get; set; }
        public string Business { get; set; }
        public string BusinessKey { get; set; }
        public string BusinessType { get; set; }
        public string YearMonth { get; set; }
        public int DAYS { get; set; }
        public decimal? Money { get; set; }
        public decimal? Account { get; set; }
        public decimal? CarAccount1 { get; set; }
        public decimal? CarAccount2 { get; set; }
        public decimal? CarAccount3 { get; set; }
        public decimal? CarAccount4 { get; set; }
        public string Founder { get; set; }
        public DateTime? CreatTime { get; set; }
    }
}