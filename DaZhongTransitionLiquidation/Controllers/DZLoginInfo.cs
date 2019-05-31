using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class DZLoginInfo
    {
        public string code { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
        public DZLoginResultData data { get; set; }
    }
    public class DZLoginResultData
    {
        public UserInfo user { get; set; }
        public RoleInfo role { get; set; }
        public string token { get; set; }
    }

    public class UserInfo
    {
        public string subCompanyId { get; set; }
        public string companyId { get; set; }
        public string id { get; set; }
        public string loginName { get; set; }
        public string name { get; set; }
        public string company { get; set; }
    }

    public class RoleInfo
    {
        public string roleCode { get; set; }
        public string roleName { get; set; }
    }
}