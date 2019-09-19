using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    ///<summary>
    ///车辆类资产退车审核表
    ///</summary>
    [SugarTable("Business_ScrapVehicle")]
    public class Business_ScrapVehicle
    {
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
        public string VEHICLE_STATE { get; set; }
        public string OPERATING_STATE { get; set; }
        public int VEHICLE_AGE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime BACK_CAR_DATE { get; set; }

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
        public string MODEL_MAJOR { get; set; }
        public string MODEL_MINOR { get; set; }

    }

    public class Business_ScrapVehicleModel: Business_ScrapVehicle
    {
        public string ASSET_ID { get; set; }
        public string BELONGTO_COMPANY { get; set; }
        public string MODEL_MAJOR { get; set; }
        public string MODEL_MINOR { get; set; }
        public string PERIOD { get; set; }
        public string DESCRIPTION { get; set; }
        public string BOOK_TYPE_CODE { get; set; }
        public decimal ASSET_COST { get; set; }
        public DateTime LISENSING_DATE { get; set; }
    }
    public class Business_ScrapVehicleShowModel : Business_ScrapVehicle
    {
        public string PLATE_NUMBER { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TAG_NUMBER { get; set; }
        public DateTime LISENSING_DATE { get; set; }

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
        public string ASSET_ID { get; set; }
    }
}