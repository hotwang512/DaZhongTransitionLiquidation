using System;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class V_Sys_Role_Module
    {
        public int? Reads { get; set; }

        public int? Edit { get; set; }

        public int? Deletes { get; set; }

        public int? Adds { get; set; }

        public int? Enable { get; set; }

        public int? Disable { get; set; }

        public int? Import { get; set; }

        public int? Export { get; set; }

        public Guid Vguid { get; set; }

        public Guid? ModuleVGUID { get; set; }

        public Guid RoleVGUID { get; set; }

        public int? PageID { get; set; }

        public string ParentID { get; set; }

        public string PageName { get; set; }
    }
}