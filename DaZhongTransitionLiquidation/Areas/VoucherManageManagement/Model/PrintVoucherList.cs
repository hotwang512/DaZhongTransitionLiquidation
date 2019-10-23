using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class PrintVoucherList
    {
        public string AccountingPeriod { get; set; }
        public string VoucherDate { get; set; }
        public string BatchName { get; set; }
        public string VoucherNo { get; set; }
        public string CompanyName { get; set; }
        public string FinanceDirector { get; set; }
        public string Bookkeeping { get; set; }
        public string Auditor { get; set; }
        public string DocumentMaker { get; set; }
        public string Cashier { get; set; }
    }
}