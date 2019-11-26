using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model
{
    public class Business_CashBorrowLoan
    {
        public Guid VGUID { get; set; }
        public string Borrow { get; set; }
        public Guid PayVGUID { get; set; }
        public string Loan { get; set; }
        public DateTime? VMDFTIME { get; set; }
        public string VMDFUSER { get; set; }
        public DateTime? VCRTTIME { get; set; }
        public string VCRTUSER { get; set; }
        public string Remark { get; set; }
        public decimal Money { get; set; }
    }
}