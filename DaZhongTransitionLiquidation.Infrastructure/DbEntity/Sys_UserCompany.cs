using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.DbEntity
{
    public class Sys_UserCompany
    {
        public Guid VGUID { get; set; }
        public string UserName { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyCodeName { get; set; }
        public string Status { get; set; }
    }
}
