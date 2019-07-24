using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class VehicleResult
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<VehicleResultData> data { get; set; }
    }
    public class VehicleResultData
    {
        public List<string> COLUMNS { get; set; }
        public List<List<string>> DATA { get; set; }
    }
}