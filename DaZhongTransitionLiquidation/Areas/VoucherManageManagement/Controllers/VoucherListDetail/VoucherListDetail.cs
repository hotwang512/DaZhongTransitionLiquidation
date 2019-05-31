using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail
{
    public class VoucherListDetail
    {
        public Guid VGUID { get; set; }
        /// <summary>
        /// Desc:账套Code
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string AccountModeCode { get; set; }
        /// <summary>
        /// Desc:账套名称
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string AccountModeName { get; set; }
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
        /// Desc:附件
        /// Default:借款单&/_theme/temp/img/2018113011943974182.jpg&花,借款单&/_theme/temp/img/20181130119433326410.jpg&水母
        /// Nullable:True
        /// </summary> 
        public string Attachment { get; set; }
        public string Status { get; set; }
        public List<Business_VoucherDetail> Detail { get; set; }
    }
}