using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    ///<summary>
    ///车辆类资产修改审核表
    ///</summary>
    [SugarTable("Business_ModifyVehicle")]
    public class Business_ModifyVehicle
    {
        public Business_ModifyVehicle()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public Guid VGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ORIGINALID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PLATE_NUMBER { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TAG_NUMBER { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string VEHICLE_SHORTNAME { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MANAGEMENT_COMPANY { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BELONGTO_COMPANY { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string VEHICLE_STATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string OPERATING_STATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ENGINE_NUMBER { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CHASSIS_NUMBER { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MODEL_MAJOR { get; set; }

        /// <summary>
        /// Desc:车牌号，经营模式,存放地点，资产原值，OBD绑定车牌号变更
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MODIFY_TYPE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MODEL_MINOR { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public bool ISVERIFY { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CREATE_DATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CREATE_USER { get; set; }

    }

    public class Business_ModifyVehicleModel: Business_ModifyVehicle
    {
        public string PLATE_NUMBER_M { get; set; }
        public string MODEL_MAJOR_M { get; set; }
        public string MODEL_MINOR_M { get; set; }
        public string MANAGEMENT_COMPANY_M { get; set; }
        public string BELONGTO_COMPANY_M { get; set; }
        public string ASSET_ID { get; set; }
        public string PERIOD { get; set; }
        public string DESCRIPTION { get; set; }
    }
}