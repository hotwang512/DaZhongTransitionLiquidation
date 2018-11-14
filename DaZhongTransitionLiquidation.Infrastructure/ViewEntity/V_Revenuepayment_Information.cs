using System;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class V_Revenuepayment_Information
    {
       
        public string Beneficiary { get; set; }
        public DateTime? ChangeDate { get; set; }
        public string ChangeUser { get; set; }
        public string Channel_Id { get; set; }
        public string Channel_IdDESC { get; set; }
        public decimal? CompanyAccount { get; set; }
        /// <summary>
        /// 驾驶员欠款金额
        /// </summary>
        public decimal? PaymentAmount { get; set; }//驾驶员欠款金额
        /// <summary>
        /// 驾驶员承担手续费=驾驶员实付金额-驾驶员欠款金额
        /// </summary>
        public decimal? DriverBearFees { get; set; }//驾驶员承担手续费=驾驶员实付金额-驾驶员欠款金额
        /// <summary>
        /// 驾驶员实付金额
        /// </summary>
        public decimal? ActualAmount { get; set; }//驾驶员实付金额
        /// <summary>
        /// 道实收手续费
        /// </summary>
        public decimal? copeFee { get; set; }//渠道实收手续费
        /// <summary>
        /// 公司承担手续费=渠道实收手续费-驾驶员承担手续费
        /// </summary>
        public decimal? CompanyBearsFees { get; set; }//公司承担手续费=渠道实收手续费-驾驶员承担手续费
        /// <summary>
        /// 渠道应付金额=驾驶员实付金额-渠道实收手续费
        /// </summary>
        public decimal? ChannelPayableAmount { get; set; }//渠道应付金额=驾驶员实付金额-渠道实收手续费


        public DateTime? CreateDate { get; set; }
        public string CreateUser { get; set; }
        public string Department { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string JobNumber { get; set; }
        public string Name { get; set; }
        public DateTime? PayDate { get; set; }
     
        public string PaymentBrokers { get; set; }
        public Guid? PaymentPersonnel { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentType { get; set; }
        public string PhoneNumber { get; set; }
        public string ReceiptAccount { get; set; }
        public string Remarks { get; set; }
        public int? RevenueStatus { get; set; }
        public int? RevenueType { get; set; }
        public string ServiceNumber { get; set; }
        public string Subject_IdDESC { get; set; }
        public string SubjectId { get; set; }
        public string TransactionID { get; set; }
        public string UserID { get; set; }
        public Guid VGUID { get; set; }


         


    }
}