﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    ///<summary>
    ///处置净值
    ///</summary>
    [SugarTable("Business_DisposeNetValue")]
    public partial class Business_DisposeNetValue
    {
        public Business_DisposeNetValue()
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
        public string AssetID { get; set; }
        public string VehicleOwner { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string DepartmentVehiclePlateNumber { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string OraclePlateNumber { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? OriginalValue { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? AcctDepreciation { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? NetValue { get; set; }

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
        public string ManageCompany { get; set; }
        public string BelongToCompany { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ChangeUser { get; set; }

        public int SubmitStatus { get; set; }
    }
}