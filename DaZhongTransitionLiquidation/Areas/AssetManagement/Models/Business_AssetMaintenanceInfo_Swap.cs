using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    ///<summary>
    ///资产更新报废接口
    ///</summary>
    [SugarTable("AssetMaintenanceInfo_Swap")]
    public class AssetMaintenanceInfo_Swap
    {
        public AssetMaintenanceInfo_Swap()
        {


        }
        /// <summary>
        /// Desc:EBS资产账簿（Oracle中中文名称）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BOOK_TYPE_CODE { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的标签号字段，作资产唯一标识（出租公司定义，唯一）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TAG_NUMBER { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的说明字段（出租公司定义）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string DESCRIPTION { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的数量字段（默认为1，出租公司定义）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? QUANTITY { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的资产大类（出租公司定义，与Oracle同步设置）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ASSET_CATEGORY_MAJOR { get; set; }

        /// <summary>
        /// Desc:对应资产信息界面中的资产次类（出租公司定义，与Oracle同步设置）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ASSET_CATEGORY_MINOR { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的资产启用日期（出租公司定义，YYYY-MM-DD）
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
        /// Desc:对应资产账簿界面中的资产原值（出租公司定义）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ASSET_COST { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的残值类型字段，传入P（百分比）或A（金额）（出租公司定义，建议给A）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string SALVAGE_TYPE { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的残值百分比字段，若残值类型为P，则不可为空（出租公司定义，提供数字，无需%）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? SALVAGE_PERCENT { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的残值字段（百分比时留空，金额时不可为空）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? SALVAGE_VALUE { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的YTD折旧字段（出租公司定义，当年累计折旧）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? YTD_DEPRECIATION { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的累计折旧字段（出租公司定义，累计折旧）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ACCT_DEPRECIATION { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的折旧方法字段（出租公司定义，与Oracle设置相同，固定STL）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string METHOD { get; set; }

        /// <summary>
        /// Desc:传入使用寿命月份数，对应资产账簿界面中的使用年限字段（出租公司定义）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? LIFE_MONTHS { get; set; }

        /// <summary>
        /// Desc:对应资产账簿界面中的摊销调整字段（出租公司定义，Y-平均，N-当期处理）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string AMORTIZATION_FLAG { get; set; }

        /// <summary>
        /// Desc:对应资产分配账户科目组合段1（出租公司定义，代码）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string EXP_ACCOUNT_SEGMENT1 { get; set; }

        /// <summary>
        /// Desc:对应资产分配账户科目组合段2（出租公司定义，代码）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string EXP_ACCOUNT_SEGMENT2 { get; set; }

        /// <summary>
        /// Desc:对应资产分配账户科目组合段3（出租公司定义，代码）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string EXP_ACCOUNT_SEGMENT3 { get; set; }

        /// <summary>
        /// Desc:对应资产分配账户科目组合段4（出租公司定义，代码）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string EXP_ACCOUNT_SEGMENT4 { get; set; }

        /// <summary>
        /// Desc:对应资产分配账户科目组合段5（出租公司定义，代码）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string EXP_ACCOUNT_SEGMENT5 { get; set; }

        /// <summary>
        /// Desc:对应资产分配账户科目组合段6（出租公司定义，代码）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string EXP_ACCOUNT_SEGMENT6 { get; set; }

        /// <summary>
        /// Desc:对应资产分配账户科目组合段7（出租公司定义，代码）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string EXP_ACCOUNT_SEGMENT7 { get; set; }

        /// <summary>
        /// Desc:对应资产分配界面的地点弹性域段1（出租公司定义，与Oracle同步设置）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FA_LOC_1 { get; set; }

        /// <summary>
        /// Desc:对应资产分配界面的地点弹性域段2（出租公司定义，与Oracle同步设置）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FA_LOC_2 { get; set; }

        /// <summary>
        /// Desc:对应资产分配界面的地点弹性域段3（出租公司定义，与Oracle同步设置）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FA_LOC_3 { get; set; }

        /// <summary>
        /// Desc:若为报废，则传入Y，否则传N
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string RETIRE_FLAG { get; set; }

        /// <summary>
        /// Desc:对应资产报废界面的报废数量字段（出租公司定义）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? RETIRE_QUANTITY { get; set; }

        /// <summary>
        /// Desc:对应资产报废界面的报废成本字段，若报废标识为Y，则不可为空（出租公司定义）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? RETIRE_COST { get; set; }

        /// <summary>
        /// Desc:对应资产报废界面的报废日期字段，若报废标识为Y，则不可为空（出租公司定义）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? RETIRE_DATE { get; set; }

        /// <summary>
        /// Desc:资产事务处理记录最后更新时间（出租公司定义等于创建日期，oracle更新状态后更新时间）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? LAST_UPDATE_DATE { get; set; }

        /// <summary>
        /// Desc:记录创建日期（出租公司定义，接口表记录创建日期）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CREATE_DATE { get; set; }

        /// <summary>
        /// Desc:业务系统中资产标识（出租公司定义，资产唯一ID）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ASSET_ID { get; set; }

        /// <summary>
        /// Desc:EBS处理后回写处理状态（出租公司插入时为N-新建，Oracle更新， S-成功E-错误）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string STATUS { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MESSAGE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ACCOUNTMODE_COMPANYCODE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string VEHICLE_TYPE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MODEL_MAJOR { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MODEL_MINOR { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public Guid TRANSACTION_ID { get; set; }

        public string PROCESS_TYPE { get; set; }
        public bool CHECK_STATE { get; set; }
    }
}