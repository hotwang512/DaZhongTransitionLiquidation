using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Models
{
    public class Business_AssetOrderBelongTo
    {
        public Guid VGUID { get; set; }
        public Guid AssetsOrderVguid { get; set; }
        public Guid AssetOrderDetailsVguid { get; set; }
        public string AssetManagementCompany { get; set; }
        public string BelongToCompany { get; set; }
        public int? AssetNum { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ChangeDate { get; set; }
        public string CreateUser { get; set; }
        public string ChangeUser { get; set; }
    }
    public class BelongToCompanyModel
    {
        public string BelongToCompany { get; set; }
        
    }
}