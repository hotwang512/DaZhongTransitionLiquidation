using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    public class Business_TransferSettingDetail
    {
        public string TransferCompany { get; set; }
        public string TransferType { get; set; }
        public string Month { get; set; }
        public string Channel { get; set; }
        public string ChannelName { get; set; }
        public Guid VGUID { get; set; }
        public string Borrow { get; set; }
        public string PayVGUID { get; set; }
        public string Loan { get; set; }
        public string CompanyCode { get; set; }
        public string AccountModeCode { get; set; }
        public bool IsUnable { get; set; }
        public DateTime? VMDFTIME { get; set; }
        public string VMDFUSER { get; set; }
        public DateTime? VCRTTIME { get; set; }
        public string VCRTUSER { get; set; }
        public decimal? Money { get; set; }
    }
}