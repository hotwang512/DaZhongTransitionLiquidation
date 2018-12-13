using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Models
{
    public class Business_OrderListAPI
    {
        public Guid VGUID { get; set; }
        /// <summary>
        /// Desc:业务类型
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BusinessType { get; set; }
        /// <summary>
        /// Desc:业务项目
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BusinessProject { get; set; }
        /// <summary>
        /// Desc:业务子项
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BusinessSubItem { get; set; }
        /// <summary>
        /// Desc:付款公司
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string PaymentCompany { get; set; }
        /// <summary>
        /// Desc:收款公司
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CollectionCompany { get; set; }
        /// <summary>
        /// Desc:业务单位
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BusinessUnit { get; set; }
        /// <summary>
        /// Desc:模式
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Mode { get; set; }
        /// <summary>
        /// Desc:车型
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VehicleType { get; set; }
        /// <summary>
        /// Desc:数量
        /// Default:
        /// Nullable:True
        /// </summary> 
        public int Number { get; set; }
        /// <summary>
        /// Desc:金额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public decimal? Money { get; set; }
        /// <summary>
        /// Desc:提交日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? SubmitDate { get; set; }
        /// <summary>
        /// Desc:支付方式
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string PaymentMethod { get; set; }
        /// <summary>
        /// Desc:附件张数
        /// Default:
        /// Nullable:True
        /// </summary> 
        public int AttachmentNumber { get; set; }
        /// <summary>
        /// Desc:发票张数
        /// Default:
        /// Nullable:True
        /// </summary> 
        public int InvoiceNumber { get; set; }
    }
}