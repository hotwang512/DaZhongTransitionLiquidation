using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class Business_AssetsRetirement_Swap
    {
        public Guid VGUID { get; set; }
        public string BOOK_TYPE_CODE { get; set; }
        public string PERIOD_CODE { get; set; }
        public string TAG_NUMBER { get; set; }
        public string DESCRIPTION { get; set; }
        public int? QUANTITY { get; set; }
        public string ASSET_CATEGORY_MAJOR { get; set; }
        public string ASSET_CATEGORY_MINOR { get; set; }
        public DateTime? ASSET_CREATION_DATE { get; set; }
        public decimal? ASSET_COST { get; set; }
        public string SALVAGE_TYPE { get; set; }
        public decimal? SALVAGE_PERCENT { get; set; }
        public decimal? SALVAGE_VALUE { get; set; }
        public string METHOD { get; set; }
        public int? LIFE_MONTHS { get; set; }
        public int? RETIRE_QUANTITY { get; set; }
        public decimal? RETIRE_COST { get; set; }
        public DateTime? RETIRE_DATE { get; set; }
        public string RETIRE_ACCT_DEPRECIATION { get; set; }
        public decimal? RETIRE_PL { get; set; }
        public DateTime? LAST_UPDATE_DATE { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public DateTime? CHANGE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string CHANGE_USER { get; set; }
    }
}