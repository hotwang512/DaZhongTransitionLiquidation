using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class PrintVoucherDetail
    {
        public string Abstract { get; set; }
        public string SevenSubjectName { get; set; }
        public string BorrowMoney { get; set; }
        public string LoanMoney { get; set; }
        public string BorrowMoneyCount { get; set; }
        public string LoanMoneyCount { get; set; }
        public int JE_LINE_NUMBER { get; set; }
    }
}