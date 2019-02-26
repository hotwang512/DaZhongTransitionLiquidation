using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class v_Channel_Subject_Desc
    {
        public string SubjectId { get; set; }
        public string SubjectNmae { get; set; }

        public decimal? Rate { get; set; }
        public DateTime? ContractStartTime { get; set; }
        public DateTime? ContractEndTime { get; set; }
        public Guid Vguid { get; set; }
        public DateTime VCRTTIME { get; set; }
        public string VCRTUSER { get; set; }
        public DateTime? VMDFTIME { get; set; }
        public string VMDFUSER { get; set; }
        /// <summary>
        /// 公司、部门
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// 终端号
        /// </summary>
        public string TerminalNo { get; set; }
        /// <summary>
        /// 门店号
        /// </summary>
        public string StoreNo { get; set; }
        /// <summary>
        /// 商户号
        /// </summary>
        public string ShopNo { get; set; }



        public Guid ChannelVguid { get; set; }

        public string OrganizationName { get; set; }

        public string ChannelName { get; set; }

        private bool _deposit = false;
        public bool Deposit { get { return _deposit; } set { _deposit = value; } }
    }
}
