using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class UserCompanySetDetail
    {
        public Guid Guids { get; set; }
        public string KeyDatas { get; set; }
        public string Code { get; set; }
        public string Descrption { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }

        public Guid VGUID { get; set; }
        public bool Isable { get; set; }
        public string PayBank { get; set; }
        public string PayAccount { get; set; }
        public string PayBankAccountName { get; set; }
        public string Borrow { get; set; }
        public string Loan { get; set; }
        public string KeyData { get; set; }
        public string OrderVGUID { get; set; }
    }
}