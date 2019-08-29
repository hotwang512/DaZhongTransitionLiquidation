using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class Excel_DisposeIncomeScrap
    {
        public string ImportPlateNumber { get; set; }
        public string VehicleModel { get; set; }
        public string CurbWeight { get; set; }
        public string DeductTonnage { get; set; }
        public string SalvageUnitPrice { get; set; }
        public string SalvageValue { get; set; }
        public string TransactionPrice { get; set; }
        public string Commission { get; set; }
        public string ProcedureFee { get; set; }
        public string RealSales { get; set; }
        public string ServiceFee { get; set; }
        public string TowageFee { get; set; }
        public string SettlementPrice { get; set; }
        public string CommissioningDate { get; set; }
        public string BackCarDate { get; set; }
        public string Remark { get; set; }
        public string BusinessModel { get; set; }
        public string UseDepartment { get; set; }
        public string VehicleOwner { get; set; }
        public string ServiceUnitFee { get; set; }
        public string VehicleType { get; set; }
        public string ActualTonnage { get; set; }
    }
}