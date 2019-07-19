using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class Business_SettlementImport
    {
        public Guid VGUID { get; set; }
        public string Model { get; set; }
        public string ClassType { get; set; }
        public string CarType { get; set; }
        public string Business { get; set; }
        public string BusinessType { get; set; }
        public decimal? Money { get; set; }
        public string Founder { get; set; }
        public DateTime? CreatTime { get; set; }
    }
}