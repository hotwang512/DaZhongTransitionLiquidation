using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList
{
    public class Business_OrderList
    {
        public Guid VGUID { get; set; }
        public string BusinessType { get; set; }
        public string BusinessProject { get; set; }
        public string BusinessSubItem { get; set; }
        public string Abstract { get; set; }
        public decimal? Money { get; set; }
        public string CompanySection { get; set; }
        public string CompanyName { get; set; }
        public string SubjectSection { get; set; }
        public string SubjectName { get; set; }
        public string AccountSection { get; set; }
        public string CostCenterSection { get; set; }
        public string SpareOneSection { get; set; }
        public string SpareTwoSection { get; set; }
        public string IntercourseSection { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Founder { get; set; }
        public string Status { get; set; }
    }
}