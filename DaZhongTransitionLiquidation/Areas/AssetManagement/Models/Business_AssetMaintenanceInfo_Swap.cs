using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class Business_AssetMaintenanceInfo_Swap
    {
        public Guid VGUID { get; set; }
        public string ASSET_CATEGORY_MAJOR { set; get; }
        public string ASSET_CATEGORY_MINOR { set; get; }
        public string BOOK_TYPE_CODE { set; get; }
        public string TAG_NUMBER { set; get; }
        public string DESCRIPTION { set; get; }
        public int? QUANTITY { set; get; }
        public DateTime? ASSET_CREATION_DATE { set; get; }
        public double? ASSET_COST { set; get; }
        public string SALVAGE_TYPE { set; get; }
        public double? SALVAGE_PERCENT { set; get; }
        public double? SALVAGE_VALUE { set; get; }
        public double? YTD_DEPRECIATION { set; get; }
        public string ACCT_DEPRECIATION { set; get; }
        public string METHOD { set; get; }
        public int? LIFE_MONTHS { set; get; }
        public string AMORTIZATION_FLAG { set; get; }
        public double? EXP_ACCOUNT_SEGMENT1 { set; get; }
        public double? EXP_ACCOUNT_SEGMENT2 { set; get; }
        public double? EXP_ACCOUNT_SEGMENT3 { set; get; }
        public double? EXP_ACCOUNT_SEGMENT4 { set; get; }
        public double? EXP_ACCOUNT_SEGMENT5 { set; get; }
        public double? EXP_ACCOUNT_SEGMENT6 { set; get; }
        public double? EXP_ACCOUNT_SEGMENT7 { set; get; }
        public string FA_LOC_1 { set; get; }
        public string FA_LOC_2 { set; get; }
        public string FA_LOC_3 { set; get; }
        public string RETIRE_FLAG { set; get; }
        public int? RETIRE_QUANTITY { set; get; }
        public double? RETIRE_COST { set; get; }
        public DateTime? RETIRE_DATE { set; get; }
        public int? TRANSACTION_ID { set; get; }
        public DateTime? LAST_UPDATE_DATE { set; get; }
        public DateTime? CREATE_DATE { set; get; }
        public DateTime? CHANGE_DATE { set; get; }
        public string CREATE_USER { set; get; }
        public string CHANGE_USER { set; get; }
        public string STATUS { set; get; }
    }
}