using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class Business_CustomerBankSetting
    {
        public Guid VGUID { get; set; }
        public Guid PurchaseItemVGUID { get; set; }
        public string CustomerID { get; set; }
        public string OrderVGUID { get; set; }
        public bool Isable { get; set; }
    }
}