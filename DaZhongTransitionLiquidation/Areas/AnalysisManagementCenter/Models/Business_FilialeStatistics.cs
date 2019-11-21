using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.AnalysisManagementCenter.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("Business_FilialeStatistics")]
    public partial class Business_FilialeStatistics
    {
        public Business_FilialeStatistics()
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
        public string DateOfYear { get; set; }
        public string YearMonth { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? BeginningPeriod { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? CurrentPeriodAdded { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? Clear { get; set; }
        public int? Zorder { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? Retirement { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? EndPeriod { get; set; }

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
        public string CreateUser { get; set; }

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
        public string ChangeUser { get; set; }

    }
}