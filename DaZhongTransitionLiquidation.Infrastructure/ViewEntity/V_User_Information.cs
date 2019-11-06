using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class V_User_Information
    {
        public DateTime ChangeDate { get; set; }
        public string ChangeUser { get; set; }
        public string Company { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string Enable { get; set; }
        public string LoginName { get; set; }
        public string MobileNnumber { get; set; }
        public string Password { get; set; }
        public string Remark { get; set; }
        public string Role { get; set; }
        public string TranslationCompany { get; set; }
        public string TranslationDepartment { get; set; }
        public string TranslationEnable { get; set; }
        public string TranslationRole { get; set; }
        public string UserName { get; set; }
        public Guid Vguid { get; set; }
        public string WorkPhone { get; set; }
        public string Permission { get; set; }
    }
}
