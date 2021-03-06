﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Common
{
    public class AssetMaintenanceInfoFlowResult
    {
        public bool success { get; set; }
        public string errmsg { get; set; }
        public List<AssetMaintenanceInfoFlowData> data
        {
            get
            {
                return list;
            }
        }

        public List<AssetMaintenanceInfoFlowData> list = new List<AssetMaintenanceInfoFlowData>();
    }
    public class AssetMaintenanceInfoFlowData
    {
        public string ASSET_CATEGORY_MAJOR { set; get; }
        public string ASSET_CATEGORY_MINOR { set; get; }
        public string GROUP_ID { set; get; }
        public string ORGANIZATION_NUM { set; get; }
        public string ENGINE_NUMBER { set; get; }
        public string CHASSIS_NUMBER { set; get; }
        public string BOOK_TYPE_CODE { set; get; }
        public string TAG_NUMBER { set; get; }
        public string DESCRIPTION { set; get; }
        public int QUANTITY { set; get; }
        public DateTime? ASSET_CREATION_DATE { set; get; }
        public double? ASSET_COST { set; get; }
        public string SALVAGE_TYPE { set; get; }
        public double? SALVAGE_PERCENT { set; get; }
        public double? SALVAGE_VALUE { set; get; }
        public double? YTD_DEPRECIATION { set; get; }
        public string ACCT_DEPRECIATION { set; get; }
        public string METHOD { set; get; }
        public int LIFE_MONTHS { set; get; }
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
        public int RETIRE_QUANTITY { set; get; }
        public double? RETIRE_COST { set; get; }
        public DateTime? RETIRE_DATE { set; get; }
        public int TRANSACTION_ID { set; get; }
        public DateTime? LAST_UPDATE_DATE { set; get; }
        public double? LISENSING_FEE { set; get; }
        public double? OUT_WAREHOUSE_FEE { set; get; }
        public double? DOME_LIGHT_FEE { set; get; }
        public double? ANTI_ROBBERY_FEE { set; get; }
        public double? LOADING_FEE { set; get; }
        public double? INNER_ROOF_FEE { set; get; }
        public double? TAXIMETER_FEE { set; get; }
        public double? OBD_FEE { set; get; }
        public string STATUS { set; get; }
    }
    public class AssetMinorResult
    {
        public bool success { get; set; }
        public string errmsg { get; set; }
        public string AssetMinor { get; set; }
    }
    public class AssetInfoData
    {
        public Guid VGUID { set; get; }
        public string ORGANATION_NUM { set; get; }
        public string ENGINE_NUMBER { set; get; }
        public string CHASSIS_NUMBER { set; get; }
        public string BOOK_TYPE_CODE { set; get; }
        public string TAG_NUMBER { set; get; }
        public string DESCRIPTION { set; get; }
        public int? QUANITIY { set; get; }

    }
}
