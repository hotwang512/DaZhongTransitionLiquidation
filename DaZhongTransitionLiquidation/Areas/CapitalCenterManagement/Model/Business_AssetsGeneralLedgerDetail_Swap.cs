using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class AssetsGeneralLedgerDetail_Swap
    {
        public string LEDGER_NAME { get; set; }
        public string JE_BATCH_NAME { get; set; }
        public string JE_BATCH_DESCRIPTION { get; set; }
        public string JE_HEADER_NAME { get; set; }
        public string JE_HEADER_DESCRIPTION { get; set; }
        public string JE_SOURCE_NAME { get; set; }
        public string JE_CATEGORY_NAME { get; set; }
        public DateTime? ACCOUNTING_DATE { get; set; }
        public string CURRENCY_CODE { get; set; }
        public string CURRENCY_CONVERSION_TYPE { get; set; }
        public DateTime? CURRENCY_CONVERSION_DATE { get; set; }
        public string CURRENCY_CONVERSION_RATE { get; set; }
        public string JE_LINE_NUMBER { get; set; }
        public string COMBINATION { get; set; }
        public string COMBINATION_DESCRIPTION { get; set; }
        public string JE_LINE_DESCRIPTION { get; set; }
        public decimal? ENTERED_DR { get; set; }
        public decimal? ENTERED_CR { get; set; }
        public decimal? ACCOUNTED_DR { get; set; }
        public decimal? ACCOUNTED_CR { get; set; }
        public DateTime? LAST_UPDATE_DATE { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string LINE_ID { get; set; }
        public string JE_LINE_ID { get; set; }
        public string SOURCE { get; set; }
    }
}