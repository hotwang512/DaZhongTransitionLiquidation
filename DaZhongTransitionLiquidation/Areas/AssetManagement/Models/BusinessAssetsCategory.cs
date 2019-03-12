using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class Business_AssetsCategory
    {
        public Guid VGUID { get; set; }
        public string ASSET_CATEGORY_MAJOR { set; get; }
        public string ASSET_CATEGORY_MINOR { set; get; }
        public int? LIFE_YEARS { set; get; }
        public int? LIFE_MONTHS { set; get; }
        public double SALVAGE_PERCENT { set; get; }
        public string METHOD { set; get; }
        public string BOOK_TYPE_CODE { set; get; }
        public string ASSET_COST_ACCOUNT { set; get; }
        public string ASSET_SETTLEMENT_ACCOUNT { set; get; }
        public string DEPRECIATION_EXPENSE_SEGMENT { set; get; }
        public string ACCT_DEPRECIATION_ACCOUNT { set; get; }
        public DateTime? CREATE_TIME { set; get; }
        public string CREATE_USER { set; get; }
        public DateTime? CHANGE_TIME { set; get; }
        public string CHANGE_USER { set; get; }
    }
    public class MajorListData
    {
        public string AssetMajor { get; set; }
    }
}
