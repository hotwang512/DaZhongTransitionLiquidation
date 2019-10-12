using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class Business_VehicleUnitList
    {
        public Guid VGUID { get; set; }
        public string ORIGINALID { get; set; }
        public string YearMonth { get; set; }
        public string PLATE_NUMBER { get; set; }
        public decimal MODEL_DAYS { get; set; }
        public string MODEL_MINOR { get; set; }
        public string Founder { get; set; }
        public DateTime CreatTime { get; set; }
        public string MANAGEMENT_COMPANY { get; set; }
        public string BELONGTO_COMPANY { get; set; }
        public string CarType { get; set; }
        public string MODEL_MAJOR { get; set; }
    }
}