﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class Business_FixedAssetsOrder
    {
        public Guid VGUID { get; set; }
        public string OrderType { set; get; }
        public Guid? PaymentInformationVguid { set; get; }
        public string PaymentInformation { set; get; }
        public int? OrderQuantity { set; get; }
        public decimal? PurchasePrices { set; get; }
        public decimal? ContractAmount { set; get; }
        public string AssetDescription { set; get; }
        public Guid? UseDepartmentVguid { get; set; }
        public string UseDepartment { set; get; }
        public string SupplierInformation { set; get; }
        public DateTime? AcceptanceDate { set; get; }
        public DateTime? PaymentDate { set; get; }
        public string ContractName { set; get; }
        public string ContractFilePath { set; get; }
        public string SubmitStatus { set; get; }
        public DateTime? CreateDate { set; get; }
        public DateTime? ChangeDate { set; get; }
        public string CreateUser { set; get; }
        public string ChangeUser { set; get; }
    }
}