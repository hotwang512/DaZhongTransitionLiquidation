using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    public class Business_PurchaseOrderSetting
    {
        public Guid VGUID { get; set; }
        public Guid PurchaseGoodsVguid { get; set; }
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
    public class Business_PurchaseDepartment
    {
        public Guid VGUID { get; set; }
        public string DepartmentName { get; set; }
        public Guid? DepartmentVguid { get; set; }
        public Guid? PurchaseOrderSettingVguid { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateUser { get; set; }
    }
    public class PurchaseOrderSettingModel: Business_PurchaseOrderSetting
    {
        public List<PurchaseDepartmentModel> DepartmentModelList { get; set; }
        public List<Business_PurchaseManagementCompany> ManagementCompanyList { get; set; }
    }
}