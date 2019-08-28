using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Models
{
    public class Business_SubjectBalance
    {
        public Guid VGUID { get; set; }
        public decimal? Balance { get; set; }
        public string Code { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string AccountModeCode { get; set; }
        public string CompanyCode { get; set; }
    }
}