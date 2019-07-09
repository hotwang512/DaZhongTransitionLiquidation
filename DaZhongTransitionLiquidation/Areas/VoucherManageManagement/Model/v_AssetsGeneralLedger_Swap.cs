using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class v_AssetsGeneralLedger_Swap
    {
        public Guid VGUID { get; set; }
        public Guid SubjectVGUID { get; set; }
        /// <summary>
        /// Desc:账套名称 
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string LEDGER_NAME { get; set; }
        /// <summary>
        /// Desc:日记账批名
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string JE_BATCH_NAME { get; set; }
        /// <summary>
        /// Desc:日记账批说明
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string JE_BATCH_DESCRIPTION { get; set; }
        /// <summary>
        /// Desc:日记账头名
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string JE_HEADER_NAME { get; set; }
        /// <summary>
        /// Desc:日记账头说明
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string JE_HEADER_DESCRIPTION { get; set; }
        /// <summary>
        /// Desc:日记账来源
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string JE_SOURCE_NAME { get; set; }
        /// <summary>
        /// Desc:日记账类别
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string JE_CATEGORY_NAME { get; set; }
        /// <summary>
        /// Desc:记账日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? ACCOUNTING_DATE { get; set; }
        /// <summary>
        /// Desc:币种
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CURRENCY_CODE { get; set; }
        /// <summary>
        /// Desc:汇率类型
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CURRENCY_CONVERSION_TYPE { get; set; }
        /// <summary>
        /// Desc:汇率日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? CURRENCY_CONVERSION_DATE { get; set; }
        /// <summary>
        /// Desc:汇率
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? CURRENCY_CONVERSION_RATE { get; set; }
        /// <summary>
        /// Desc:日记账行号
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? JE_LINE_NUMBER { get; set; }
        /// <summary>
        /// Desc:科目组合SEGMENT1
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string SEGMENT1 { get; set; }
        /// <summary>
        /// Desc:科目组合SEGMENT2
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string SEGMENT2 { get; set; }
        /// <summary>
        /// Desc:科目组合SEGMENT3
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string SEGMENT3 { get; set; }
        /// <summary>
        /// Desc:科目组合SEGMENT4
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string SEGMENT4 { get; set; }
        /// <summary>
        /// Desc:科目组合SEGMENT5
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string SEGMENT5 { get; set; }
        /// <summary>
        /// Desc:科目组合SEGMENT6
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string SEGMENT6 { get; set; }
        /// <summary>
        /// Desc:科目组合SEGMENT7
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string SEGMENT7 { get; set; }
        /// <summary>
        /// Desc:输入借方金额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ENTERED_DR { get; set; }
        /// <summary>
        /// Desc:输入贷方金额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ENTERED_CR { get; set; }
        /// <summary>
        /// Desc:入账借方金额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ACCOUNTED_DR { get; set; }
        /// <summary>
        /// Desc:入账贷方金额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ACCOUNTED_CR { get; set; }
        /// <summary>
        /// Desc:更新时间
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? LAST_UPDATE_DATE { get; set; }
        /// <summary>
        /// Desc:创建日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? CREATE_DATE { get; set; }
        /// <summary>
        /// Desc:处理状态
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string STATUS { get; set; }
        /// <summary>
        /// Desc:自增长ID
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string TRASACTION_ID { get; set; }
        /// <summary>
        /// Desc:日记账行ID
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string LINE_ID { get; set; }
        public string JE_LINE_DESCRIPTION { get; set; }
        /// <summary>
        /// Desc:来源
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string SOURCE { get; set; }
        public string SubjectCount { get; set; }
        public string MESSAGE { get; set; }
        public string TurnOut { get; set; }
        public string TurnIn { get; set; }
    }
}