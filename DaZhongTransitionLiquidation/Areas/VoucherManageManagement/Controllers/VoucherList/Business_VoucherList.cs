using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherList
{
    public class Business_VoucherList
    {
        public Guid VGUID { get; set; }
        /// <summary>
        /// Desc:营运公司Code
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CompanyCode { get; set; }
        /// <summary>
        /// Desc:营运公司Name
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CompanyName { get; set; }
        /// <summary>
        /// Desc:会计期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? AccountingPeriod { get; set; }
        /// <summary>
        /// Desc:币种
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Currency { get; set; }
        /// <summary>
        /// Desc:批名
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BatchName { get; set; }
        /// <summary>
        /// Desc:凭证号码
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VoucherNo { get; set; }
        /// <summary>
        /// Desc:凭证日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? VoucherDate { get; set; }
        /// <summary>
        /// Desc:凭证类型
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VoucherType { get; set; }
        /// <summary>
        /// Desc:借方金额合计
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? DebitAmountTotal { get; set; }
        /// <summary>
        /// Desc:贷方金额合计
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? CreditAmountTotal { get; set; }     
        /// <summary>
        /// Desc:财务主管
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string FinanceDirector { get; set; }
        /// <summary>
        /// Desc:记账
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Bookkeeping { get; set; }
        /// <summary>
        /// Desc:审核
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Auditor { get; set; }
        /// <summary>
        /// Desc:制单
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string DocumentMaker { get; set; }
        /// <summary>
        /// Desc:出纳
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Cashier { get; set; }
        /// <summary>
        /// Desc:附件1
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Attachment1 { get; set; }
        /// <summary>
        /// Desc:附件2
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Attachment2 { get; set; }
        /// <summary>
        /// Desc:附件3
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Attachment3 { get; set; }
        /// <summary>
        /// Desc:附件4
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Attachment4 { get; set; }
        /// <summary>
        /// Desc:附件5
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Attachment5 { get; set; }
        /// <summary>
        /// Desc:审核状态
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Status { get; set; }
    }
}