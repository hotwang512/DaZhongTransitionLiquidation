using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    ///<summary>
    ///资产台账接口
    ///</summary>
    [SugarTable("AssetsLedger_Swap")]
    public class AssetsLedger_Swap
    {
        /// <summary>
        /// Desc:EBS资产账簿
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BOOK_TYPE_CODE { get; set; }

        /// <summary>
        /// Desc:资产信息所属期间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PERIOD_CODE { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的标签号字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TAG_NUMBER { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的说明字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string DESCRIPTION { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的数量字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? QUANTITY { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的资产大类
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ASSET_CATEGORY_MAJOR { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的资产次类
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
        /// Desc:对应资产账簿界面中的资产原值
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ASSET_COST { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的残值类型字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string SALVAGE_TYPE { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的残值百分比字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? SALVAGE_PERCENT { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的残值字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? SALVAGE_VALUE { get; set; }

        /// <summary>
        /// Desc:对应资产本月折旧额
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? PTD_DEPRECIATION { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的YTD折旧字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? YTD_DEPRECIATION { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的累计折旧字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ACCT_DEPRECIATION { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的折旧方法字段
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string METHOD { get; set; }

        /// <summary>
        /// Desc:资产使用寿命月数
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? LIFE_MONTHS { get; set; }

        /// <summary>
        /// Desc:对应资产分配账户
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string EXP_ACCOUNT { get; set; }

        /// <summary>
        /// Desc:对应资产分配界面的＄?弹性域段1
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FA_LOC_1 { get; set; }

        /// <summary>
        /// Desc:对应资产分配界面的＄?弹性域段2
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FA_LOC_2 { get; set; }

        /// <summary>
        /// Desc:对应资产分配界面的＄?弹性域段3
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FA_LOC_3 { get; set; }

        /// <summary>
        /// Desc:记录创建日期（出租公司定义，接口表记录创建日期）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CREATE_DATE { get; set; }

        /// <summary>
        /// Desc:最后更新时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? LAST_UPDATE_DATE { get; set; }

    }
}