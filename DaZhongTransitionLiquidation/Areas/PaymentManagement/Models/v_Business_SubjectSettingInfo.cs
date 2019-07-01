using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Models
{
    public class v_Business_SubjectSettingInfo
    {
        public string BusinessCode { get; set; }
        public string Company { get; set; }
        public string CompanyCode { get; set; }
        public string AccountingCode { get; set; }
        public string Accounting { get; set; }
        public string CostCenterCode { get; set; }
        public string CostCenter { get; set; }
        public string SpareOneCode { get; set; }
        public string SpareOne { get; set; }
        public string SpareTwoCode { get; set; }
        public string SpareTwo { get; set; }
        public string IntercourseCode { get; set; }
        public string Intercourse { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectVGUID { get; set; }
        public string Checked { get; set; }
        public decimal? Balance { get; set; }
        public decimal? ENTERED_DR { get; set; }
        public decimal? ENTERED_CR { get; set; }
        public decimal? END_BALANCE { get; set; }
    }
}