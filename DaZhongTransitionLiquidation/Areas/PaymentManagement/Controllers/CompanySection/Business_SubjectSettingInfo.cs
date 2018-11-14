using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection
{
    public class Business_SubjectSettingInfo
    {
        public Guid VGUID { get; set; }
        public string SubjectCode { get; set; }
        public string AccountingCode { get; set; }
        public string CostCenterCode { get; set; }
        public string SpareOneCode { get; set; }
        public string SpareTwoCode { get; set; }
        public string IntercourseCode { get; set; }
        public Guid SubjectVGUID { get; set; }
        public bool Checked { get; set; }
        public string CompanyCode { get; set; }
        public string AccountModeCode { get; set; }
    }
}
