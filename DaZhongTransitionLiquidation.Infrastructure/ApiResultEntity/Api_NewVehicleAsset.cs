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
        public string PRODUCTION_DATE { get; set; }
        public string PURCHASE_DATE { get; set; }
        public string LISENSING_DATE { get; set; }
        public string COMMISSIONING_DATE { get; set; }
        public string FUEL_TYPE { get; set; }
        public string DELIVERY_INFORMATION { get; set; }
    }
    public class Api_VehicleAssetResult<T1, T2>
    {
        public List<T1> COLUMNS;
        public List<List<T2>> DATA;
    }
    public class Api_ScrapVehicleAsset
    {
        public string ORIGINALID { get; set; }
        public string VEHICLE_STATE { get; set; }
        public string OPERATING_STATE { get; set; }
        public string PLATE_NUMBER { get; set; }
        public string BACK_CAR_DATE { get; set; }
        public string MODEL_MINOR { get; set; }
    }
    public class Api_ModifyVehicleAsset
    {
        public string ORIGINALID { set; get; }
        public string PLATE_NUMBER { set; get; }
        public string TAG_NUMBER { set; get; }
        public string VEHICLE_SHORTNAME { set; get; }
        public string MANAGEMENT_COMPANY { set; get; }
        public string BELONGTO_COMPANY { set; get; }
        public string VEHICLE_STATE { set; get; }
        public string OPERATING_STATE { set; get; }
        public string ENGINE_NUMBER { set; get; }
        public string CHASSIS_NUMBER { set; get; }
        public string MODEL_MAJOR { set; get; }
        public string MODEL_MINOR { set; get; }
    }
}