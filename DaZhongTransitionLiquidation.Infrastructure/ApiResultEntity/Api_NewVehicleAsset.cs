using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity
{
    public class Api_NewVehicleAsset
    {
        public string ORIGINALID { get; set; }
        public string PLATE_NUMBER { get; set; }
        public string VEHICLE_SHORTNAME { get; set; }
        public string MANAGEMENT_COMPANY { get; set; }
        public string BELONGTO_COMPANY { get; set; }
        public string VEHICLE_STATE { get; set; }
        public string OPERATING_STATE { get; set; }
        public string MODEL_MINOR { get; set; }
        public string ENGINE_NUMBER { get; set; }
        public string CHASSIS_NUMBER { get; set; }
        public DateTime? PRODUCTION_DATE { get; set; }
        public DateTime? PURCHASE_DATE { get; set; }
        public DateTime? LISENSING_DATE { get; set; }
        public DateTime? COMMISSIONING_DATE { get; set; }
        public string FUEL_TYPE { get; set; }
        public string DELIVERY_INFORMATION { get; set; }
    }
}
