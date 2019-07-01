using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    public class Business_PurchaseFaxOrderSetting
    {
        public Guid VGUID { get; set; }
        public string PurchaseGoods { get; set; }
        public string AssetCategoryMajor { get; set; }
        public string AssetCategoryMinor { get; set; }
        public int OrderCategory { get; set; }
        public string BusinessSubItem { get; set; }
        public Guid? AssetCategoryMinorVguid { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ChangeDate { get; set; }
        public string CreateUser { get; set; }
        public string ChangeUser { get; set; }
    }
    public class PurchaseFaxOrderSettingModel : Business_PurchaseFaxOrderSetting
    {
        public List<PurchaseDepartmentModel> DepartmentModelList { get; set; }
        public List<Business_PurchaseManagementCompany> ManagementCompanyList { get; set; }
    }
}