using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class VoucherModelDetails
    {
        public Guid VGUID { get; set; }
        public string YearMonth { get; set; }
        public DateTime VoucherDate { get; set; }
        public string ModelName { get; set; }
        public string AccountModeCode { get; set; }
        public string CompanyCode { get; set; }
        public string VoucherStatus { get; set; }
        public List<Business_CashBorrowLoan> VoucherData { get; set; }
    }
}