using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Models
{
    public class Business_TaxesInfo
    {
        public Guid VGUID { get; set; }
        public Guid SubjectVGUID { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string TaxesType { get; set; }
        public string TaxRate { get; set; }
        public string AccountModeCode { get; set; }
        public string CompanyCode { get; set; }
    }
}