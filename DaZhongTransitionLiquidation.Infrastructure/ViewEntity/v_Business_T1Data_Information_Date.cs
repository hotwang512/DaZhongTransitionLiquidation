using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class v_Business_T1Data_Information_Date
    {
        public DateTime? ChangeDate { get; set; }
        public string ChangeUser { get; set; }
        public string Channel_Id { get; set; }
        public string ChannelName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        /// <summary>
        /// 驾驶员欠款金额
        /// </summary>
        public decimal? Remitamount { get; set; }
        /// <summary>
        /// 驾驶员承担手续费=驾驶员实付金额-驾驶员欠款金额
        /// </summary>
        public decimal? DriverBearFees { get; set; }//驾驶员承担手续费=驾驶员实付金额-驾驶员欠款金额

        /// <summary>
        /// 驾驶员实付金额
        /// </summary>                                           
        public decimal? PaidAmount { get; set; }

        /// <summary>
        /// 道实收手续费
        /// </summary>
        public decimal? RevenueFee { get; set; }

        /// <summary>
        /// 公司承担手续费=渠道实收手续费-驾驶员承担手续费
        /// </summary>
        public decimal? CompanyBearsFees { get; set; }//公司承担手续费=渠道实收手续费-驾驶员承担手续费
        /// <summary>
        /// 渠道应付金额=驾驶员实付金额-渠道实收手续费
        /// </summary>
        public decimal? ChannelPayableAmount { get; set; }//渠道应付金额=驾驶员实付金额-渠道实收手续费

        public DateTime? Revenuetime { get; set; }
        public string RevenuetimeStr { get; set; }
        public string serialnumber { get; set; }
        public string SubjectId { get; set; }
        public string SubjectNmae { get; set; }
        public Guid Vguid { get; set; }
        public string WechatNo { get; set; }

    }
}
