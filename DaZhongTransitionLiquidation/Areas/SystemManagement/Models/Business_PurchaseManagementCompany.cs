using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    public class Business_PurchaseManagementCompany
    {
        public Guid VGUID { get; set; }
        public Guid ManagementCompanyVguid { get; set; }
        public string ManagementCompany { get; set; }
        public string Descrption { get; set; }
        public Guid? PurchaseOrderSettingVguid { get; set; }
        public string AccountModeCode { get; set; }
        public string CompanyCode { get; set; }
        public string KeyData { get; set; }
        public bool IsCheck { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateUser { get; set; }
    }
}