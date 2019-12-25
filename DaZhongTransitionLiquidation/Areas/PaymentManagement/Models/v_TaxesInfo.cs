using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Models
{
    public class v_TaxesInfo
    {
        public string Code { get; set; }
        public string ParentCode { get; set; }
        public string Descrption { get; set; }
        public string TaxesType { get; set; }
        public string TaxRate { get; set; }
        public string AccountModeCode { get; set; }
        public string CompanyCode { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public Guid VGUID { get; set; }
        public Guid KeyVGUID { get; set; }
        public string Abbreviation { get; set; }
    }
}