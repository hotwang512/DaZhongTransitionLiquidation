using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    ///<summary>
    ///车辆类资产费用标准
    ///</summary>
    [SugarTable("Business_VehicleExtrasFeeSetting")]
    public class Business_VehicleExtrasFeeSetting
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
        public string VehicleModelCode { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string VehicleModel { get; set; }

        public decimal Fee { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BusinessSubItem { get; set; }

        public string BusinessProject { get; set; }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? Status { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? ChangeDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreateUser { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ChangeUser { get; set; }

    }
}