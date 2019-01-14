using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Models
{
    public class Business_OrderListDraft
    {
        public Guid VGUID { get; set; }
        /// <summary>
        /// Desc:填单日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public DateTime? FillingDate { get; set; }
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
        /// Desc:业务子项1
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BusinessSubItem1 { get; set; }
        /// <summary>
        /// Desc:业务子项2
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BusinessSubItem2 { get; set; }
        /// <summary>
        /// Desc:业务子项3
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BusinessSubItem3 { get; set; }
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
        /// <summary>
        /// Desc:订单日期
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string OrderDate { get; set; }
        /// <summary>
        /// Desc:订单时间
        /// Default:中午或晚上
        /// Nullable:True
        /// </summary> 
        public string OrderTime { get; set; }
        /// <summary>
        /// Desc:来客人数
        /// Default:
        /// Nullable:True
        /// </summary> 
        public int VisitorsNumber { get; set; }
        /// <summary>
        /// Desc:陪同人数
        /// Default:
        /// Nullable:True
        /// </summary> 
        public int EscortNumber { get; set; }
        /// <summary>
        /// Desc:人数合计
        /// Default:
        /// Nullable:True
        /// </summary> 
        public int NumberCount { get; set; }
        /// <summary>
        /// Desc:付款内容
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string PaymentContents { get; set; }
        /// <summary>
        /// Desc:金额（大写）
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CapitalizationMoney { get; set; }
        /// <summary>
        /// Desc:企业负责人
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string EnterpriseLeader { get; set; }
        /// <summary>
        /// Desc:分管负责人
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ResponsibleLeader { get; set; }
        /// <summary>
        /// Desc:计财部审核
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string JiCaiBuExamine { get; set; }
        /// <summary>
        /// Desc:部门主管
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string DepartmentHead { get; set; }
        /// <summary>
        /// Desc:出纳
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Cashier { get; set; }
        /// <summary>
        /// Desc:受款人经办人
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string Payee { get; set; }
        public string Status { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Founder { get; set; }
        public string Attachment { get; set; }
        public string OrderCompany { get; set; }
        /// <summary>
        /// Desc:付款账号ACON
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string OrderBankAccouont { get; set; }
        /// <summary>
        /// Desc:付款账号户名
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string OrderBankAccouontName { get; set; }
        /// <summary>
        /// Desc:付款账号开户行
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string OrderBankName { get; set; }
        /// <summary>
        /// Desc:收款账号OPAC
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CollectBankAccouont { get; set; }
        /// <summary>
        /// Desc:收款账号户名
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CollectBankAccountName { get; set; }
        /// <summary>
        /// Desc:收款账号开户行
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CollectBankName { get; set; }
        /// <summary>
        /// Desc:交易序列号
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string OSNO { get; set; }
        /// <summary>
        /// Desc:交易状态
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BankStatus { get; set; }
        /// <summary>
        /// Desc:交易最终结果描述
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BankStatusName { get; set; }
    }
}