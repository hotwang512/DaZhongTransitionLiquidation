using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail
{
    public class Business_VoucherDetail
    {
        public Guid VGUID { get; set; }
        public Guid VoucherVGUID { get; set; }
        public Guid ModelVGUID { get; set; }
        public string Abstract { get; set; }
        public string CompanySection { get; set; }
        public string SubjectSection { get; set; }
        public string SubjectSectionName { get; set; }
        public string AccountSection { get; set; }
        public string CostCenterSection { get; set; }
        public string SpareOneSection { get; set; }
        public string SpareTwoSection { get; set; }
        public string IntercourseSection { get; set; }
        public decimal? BorrowMoney { get; set; }
        public decimal? LoanMoney { get; set; }
        public string SevenSubjectName { get; set; }
        public decimal? BorrowMoneyCount { get; set; }
        public decimal? LoanMoneyCount { get; set; }
        public int JE_LINE_NUMBER { get; set; }
        public string ReceivableAccount { get; set; }
    }
}