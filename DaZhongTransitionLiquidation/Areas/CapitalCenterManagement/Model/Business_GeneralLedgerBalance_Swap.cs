using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class GeneralLedgerBalance_Swap
    {
        /// <summary>
        /// Desc:总账账簿
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string LEDGER_NAME { get; set; }
        /// <summary>
        /// Desc:科目组合ID
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CCID { get; set; }
        /// <summary>
        /// Desc:科目组合（代码）
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string COMBINATION { get; set; }
        /// <summary>
        /// Desc:余额对应期间
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? PERIOD { get; set; }
        /// <summary>
        /// Desc:期初余额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? BEGIN_BALANCE { get; set; }
        /// <summary>
        /// Desc:期间借方发生额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? PTD_DR { get; set; }
        /// <summary>
        /// Desc:期间贷方发生额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? PTD_CR { get; set; }
        /// <summary>
        /// Desc:期末余额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? END_BALANCE { get; set; }
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