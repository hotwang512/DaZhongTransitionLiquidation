using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class Business_IntangibleAssetsOrder
    {
        public Guid VGUID { get; set; }
        public string OrderType { set; get; }
        public Guid? PaymentInformationVguid { set; get; }
        public string PaymentInformation { set; get; }
        public decimal? SumPayment { set; get; }
        public decimal? FirstPayment { set; get; }
        public decimal? TailPayment { set; get; }
        public string SupplierInformation { set; get; }
        public string ContractName { set; get; }
        public string ContractFilePath { set; get; }
        public int? SubmitStatus { set; get; }
        public DateTime? CreateDate { set; get; }
        public DateTime? ChangeDate { set; get; }
        public string CreateUser { set; get; }
        public string ChangeUser { set; get; }
        public DateTime? SubmitDate { set; get; }
        public string SubmitUser { set; get; }
    }
}