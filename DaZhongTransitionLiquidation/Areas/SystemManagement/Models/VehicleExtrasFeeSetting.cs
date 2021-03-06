﻿using System;
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
        public Boolean Status { get; set; }

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
        public DateTime StartDate { get; set; }

    }

    public class SaveFeeSettingModel
    {
        public List<Business_VehicleExtrasFeeSetting> FeeSettingList { get; set; }
    }
    public class Business_VehicleExtrasFeeSettingShow: Business_VehicleExtrasFeeSetting
    {
        //public Business_VehicleExtrasFeeSetting FeeSetting { get; set; }

        public string strShow
        {
            get
            {
                var str = "";
                foreach (var item in SettingHistoryList)
                {
                    str = str + item.StartDate.ToString("yyyy-MM")+ "&nbsp;" + item.Fee + "&#13;";
                }
                return str;
            }
        }
        public List<Business_VehicleExtrasFeeSettingHistory> SettingHistoryList { get; set; }
    }
    public class ExtrasFeeSettingHistory
    {
        public string StartDate { get; set; }
        public string Fee { get; set; }
    }
}