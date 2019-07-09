using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Common
{
    public class ModifyVehicleApiModel
    {
        public string ORIGINALID{ set; get; }
        public string PLATE_NUMBER{ set; get; }
        public string TAG_NUMBER{ set; get; }
        public string VEHICLE_SHORTNAME{ set; get; }
        public string MANAGEMENT_COMPANY{ set; get; }
        public string BELONGTO_COMPANY{ set; get; }
        public string VEHICLE_STATE{ set; get; }
        public string OPERATING_STATE{ set; get; }
        public string ENGINE_NUMBER{ set; get; }
        public string CHASSIS_NUMBER{ set; get; }
        public string MODEL_MINOR { set; get; }
    }

    public class ModifyVehicleApiResult
    {
        public bool success { get; set; }
        public string errmsg { get; set; }
        public List<ModifyVehicleApiModel> data
        {
            get
            {
                return list;
            }
        }
        public List<ModifyVehicleApiModel> list = new List<ModifyVehicleApiModel>();
    }
}
