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
        public string EngineNumber { get; set; }
        public string ChassisNumber { get; set; }
        public string BelongToCompany { get; set; }
        public string StartVehicleDate { get; set; }
        public string UseDepartment { get; set; }
        public int AssetNum { get; set; }
        public string VehicleModel { set; get; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ChangeDate { get; set; }
        public string CreateUser { get; set; }
        public string ChangeUser { get; set; }
    }
    public class Business_AssetOrderBelongToShow
    {
        public Guid AssetsOrderVguid { get; set; }
        public string BelongToCompany { get; set; }
        public int? AssetNum { get; set; }
        public decimal? PurchasePrices { get; set; }
        public decimal? PurchaseCountPrices
        {
            get { return AssetNum * PurchasePrices; }
        }

    }
    public class BelongToCompanyModel
    {
        public string BelongToCompany { get; set; }
        
    }

    public class Vehicle_Model
    {
        public string VehicleModel { get; set; }
    }
}