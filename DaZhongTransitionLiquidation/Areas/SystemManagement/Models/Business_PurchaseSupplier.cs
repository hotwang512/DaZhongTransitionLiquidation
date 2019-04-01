using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    public class Business_PurchaseSupplier
    {
        public Guid VGUID { get; set; }
        public Guid? PurchaseOrderSettingVguid { get; set; }
        public string CustomerBankInfoCategory { get; set; }
        public Guid? CustomerBankInfoVguid { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateUser { get; set; }
    }
}