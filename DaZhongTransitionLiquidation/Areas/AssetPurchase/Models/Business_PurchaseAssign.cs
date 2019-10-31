using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Models
{
    public class Business_PurchaseAssign
    {
        public Guid VGUID { get; set; }
        public Guid? FixedAssetsOrderVguid { set; get; }
        public Guid? PurchaseGoodsVguid { set; get; }
        public string PurchaseGoods { set; get; }
        public int? OrderQuantity { set; get; }
        public decimal? PurchasePrices { set; get; }
        public decimal? ContractAmount { set; get; }
        public string AssetDescription { set; get; }
        public int SubmitStatus { set; get; }
        public DateTime? CreateDate { set; get; }
        public DateTime? ChangeDate { set; get; }
        public string CreateUser { set; get; }
        public string ChangeUser { set; get; }
        public DateTime? SubmitDate { set; get; }
        public string SubmitUser { set; get; }
        public string OrderType { set; get; }
    }
}