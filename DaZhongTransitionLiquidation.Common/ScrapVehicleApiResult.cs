using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Common
{
    public class ScrapVehicleApiModel
    {
        public string ORIGINALID { get; set; }
        public string PLATE_NUMBER { get; set; }
        public DateTime BACK_CAR_DATE { get; set; }
    }
    public class ScrapVehicleApiResult
    {
        public bool success { get; set; }
        public string errmsg { get; set; }
        public List<ScrapVehicleApiModel> data
        {
            get
            {
                return list;
            }
        }
        public List<ScrapVehicleApiModel> list = new List<ScrapVehicleApiModel>();
    }
}
