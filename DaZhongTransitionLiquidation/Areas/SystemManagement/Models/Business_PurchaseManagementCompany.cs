using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    public class Business_PurchaseManagementCompany
    {
        public Guid VGUID { get; set; }
        public string ManagementCompany { get; set; }
        public Guid? PurchaseOrderSettingVguid { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateUser { get; set; }
    }
}