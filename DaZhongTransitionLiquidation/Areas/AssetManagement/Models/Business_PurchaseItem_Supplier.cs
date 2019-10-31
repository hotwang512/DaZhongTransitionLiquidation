using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class Business_PurchaseItem_Supplier
    {
        public Guid VGUID { get; set; }
        public Guid? PurchaseGoodsVguid { get; set; }
        public Guid? BankInfoVguid { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ChangeDate { get; set; }
        public string CreateUser { get; set; }
        public string ChangeUser { get; set; }
    }
}