using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class Business_LedgerSubject_Swap
    {
        /// <summary>
        /// Desc:账簿
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BOOK { get; set; }
        /// <summary>
        /// Desc:传Oracle中值集名
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VALUE_SET { get; set; }
        /// <summary>
        /// Desc:传入科目代码
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CODE { get; set; }
        /// <summary>
        /// Desc:传入科目说明
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string DESCRIPTION { get; set; }
        /// <summary>
        /// Desc:有效标识(有效Y，无效为N)
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ACTIVE_FLAG { get; set; }
        /// <summary>
        /// Desc:记录更新时间
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? LAST_UPDATE_DATE { get; set; }
        /// <summary>
        /// Desc:记录创建日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? CREATE_DATE { get; set; }
        public Guid VGUID { get; set; }
    }
}