using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Models
{
    public class Business_SevenSection
    {
        public Guid VGUID { get; set; }
        public string Code { get; set; }
        public string ParentCode { get; set; }
        public string Descrption { get; set; }
        public string SectionVGUID { get; set; }
        public string VCRTUSER { get; set; }
        public string VMDFUSER { get; set; }
        public DateTime? VMDFTIME { get; set; }
        public DateTime? VCRTTIME { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public bool IsAccountingCode { get; set; }
        public bool IsCostCenterCode { get; set; }
        public bool IsSpareOneCode { get; set; }
        public bool IsSpareTwoCode { get; set; }
        public bool IsIntercourseCode { get; set; }
        public bool IsCompanyCode { get; set; }
        public bool IsAccountModeCode { get; set; }
        public bool IsSubjectCode { get; set; }
        public bool IsSetAccount { get; set; }
        public bool IsCompanyBank { get; set; }
        public string AccountModeCode { get; set; }
        public string CompanyCode { get; set; }
    }
    public class SevenSectionDropdown
    {
        public Guid VGUID { get; set; }
        public string Descrption { get; set; }
    }
}