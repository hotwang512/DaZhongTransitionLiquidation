using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    public class Business_UserCompanySet
    {
        public Guid VGUID { get; set; }
        public string KeyData { get; set; }
        public bool IsCheck { get; set; }
        public string Block { get; set; }
        public string UserVGUID { get; set; }
        public string Code { get; set; }
        public string Descrption { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string BusinessCode { get; set; }
        public string BusinessName { get; set; }
    }
}