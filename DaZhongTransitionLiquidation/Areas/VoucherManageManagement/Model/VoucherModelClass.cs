using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model
{
    public class VoucherModelClass
    {
        public Guid VGUID { get; set; }
        public string ModelName { get; set; }
        public string AccountModeCode { get; set; }
        public string CompanyCode { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public string CreateStatus { get; set; }
        public string Creater { get; set; }
        public DateTime CreateTime { get; set; }
    }
}