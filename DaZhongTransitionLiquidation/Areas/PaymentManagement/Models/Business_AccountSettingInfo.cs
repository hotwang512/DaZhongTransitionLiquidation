using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Models
{
    public class Business_AccountSettingInfo
    {
        public Guid VGUID { get; set; }
        public string AccountCode { get; set; }
        public Guid BankVGUID { get; set; }
        public bool IsChecked { get; set; }
    }
}