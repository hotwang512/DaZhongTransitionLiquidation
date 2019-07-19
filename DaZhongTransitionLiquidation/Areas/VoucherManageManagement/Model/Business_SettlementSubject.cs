using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class Business_SettlementSubject
    {
        public Guid VGUID { get; set; }
        public string BusinessType { get; set; }
        public Guid ParentVGUID { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Founder { get; set; }
    }
}