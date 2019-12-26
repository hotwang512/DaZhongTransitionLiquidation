using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models
{
    public class UnUsedVehicleStatistics
    {

        public string Period { get; set; }
        public string OrganizationType { get; set; }
        public string CompanyType { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public int Quantity { get; set; }
        public int StopVehicleQuantity { get; set; }
        public int StopLicence { get; set; }
        public int StopAll
        {
            get { return StopVehicleQuantity + StopLicence; }
        }
    }
}