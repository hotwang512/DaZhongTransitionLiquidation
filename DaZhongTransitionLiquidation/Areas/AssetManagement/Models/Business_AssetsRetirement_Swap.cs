using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    ///<summary>
    ///资产报废中间表
    ///</summary>
    [SugarTable("AssetsRetirement_Swap")]
    public class AssetsRetirement_Swap
    {
        public AssetsRetirement_Swap()
        {


        }
        /// <summary>
        /// Desc:EBS资产账簿
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BOOK_TYPE_CODE { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的标签号字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TAG_NUMBER { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string DESCRIPTION { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? QUANTITY { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ASSET_CATEGORY_MAJOR { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ASSET_CATEGORY_MINOR { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的资产启用日期
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? ASSET_CREATION_DATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PERIOD { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ASSET_COST { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string SALVAGE_TYPE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? SALVAGE_PERCENT { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? SALVAGE_VALUE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string METHOD { get; set; }

        /// <summary>
        /// Desc:传入使用寿命月份数，对应资产账簿界面中的使用年限字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? LIFE_MONTHS { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? RETIRE_QUANTITY { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? RETIRE_COST { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? RETIRE_DATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string RETIRE_ACCT_DEPRECIATION { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? RETIRE_PL { get; set; }

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
        public DateTime? LAST_UPDATE_DATE { get; set; }

        public string ASSET_ID { get; set; }

        /// <summary>
        /// Desc:资产处置方式
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string DISPOSA_TYPE { get; set; }

        /// <summary>
        /// Desc:资产处置金额
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? DISPOSA_AMOUNT { get; set; }

        /// <summary>
        /// Desc:资产处置税金
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? DISPOSAL_TAX { get; set; }

        /// <summary>
        /// Desc:资产处置损益
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? DISPOSAL_PL { get; set; }

    }
}