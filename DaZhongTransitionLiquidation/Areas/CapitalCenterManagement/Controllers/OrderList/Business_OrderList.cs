using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList
{
    public class Business_OrderList
    {
        public Guid VGUID { get; set; }
        public string BusinessType { get; set; }
        public string BusinessProject { get; set; }
        public string BusinessSubItem1 { get; set; }
        public string BusinessSubItem2 { get; set; }
        public string BusinessSubItem3 { get; set; }
        public string Abstract { get; set; }
        public string CompanySection { get; set; }
        public string CompanyName { get; set; }
        public string SubjectSection { get; set; }
        public string SubjectName { get; set; }
        public string AccountSection { get; set; }
        public string CostCenterSection { get; set; }
        public string SpareOneSection { get; set; }
        public string SpareTwoSection { get; set; }
        public string IntercourseSection { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Founder { get; set; }
        public string Status { get; set; }
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