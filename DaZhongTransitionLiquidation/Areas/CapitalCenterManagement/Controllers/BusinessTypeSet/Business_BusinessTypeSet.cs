using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BusinessTypeSet
{
    public class Business_BusinessTypeSet
    {
        public Guid VGUID { get; set; }
        public string Code { get; set; }
        public string BusinessName { get; set; }
        public string ParentVGUID { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Founder { get; set; }
    }
}