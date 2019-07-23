using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Models
{
    public class Business_IntangibleAssetsOrder
    {
        public Guid VGUID { get; set; }
        public Guid? PurchaseGoodsVguid { set; get; }
        public string OrderNumber { set; get; }
        public string PurchaseDepartmentIDs { set; get; }
        public string PurchaseGoods { set; get; }
        public string AssetDescription { set; get; }
        public Guid? PaymentInformationVguid { set; get; }
        public string PaymentInformation { set; get; }
        public decimal? SumPayment { set; get; }
        public decimal? FirstPayment { set; get; }
        public decimal? TailPayment { set; get; }
        public string ContractName { set; get; }
        public string ContractFilePath { set; get; }
        public string SupplierBankAccountName { set; get; }
        public string SupplierBankAccount { set; get; }
        public string SupplierBank { set; get; }
        public string SupplierBankNo { set; get; }
        public string PayType { set; get; }
        public Guid PayCompanyVguid { set; get; }
        public string PayCompany { set; get; }
        public string CompanyBankName { set; get; }
        public string CompanyBankAccount { set; get; }
        public string CompanyBankAccountName { set; get; }
        public string AccountType { set; get; }
        public int? SubmitStatus { set; get; }
        public DateTime? CreateDate { set; get; }
        public DateTime? ChangeDate { set; get; }
        public string CreateUser { set; get; }
        public string ChangeUser { set; get; }
        public DateTime? SubmitDate { set; get; }
        public string SubmitUser { set; get; }
        public Guid? PaymentVoucherVguid { set; get; }
        public string PaymentVoucherUrl { set; get; }
        public string OSNO { get; set; }
    }
}