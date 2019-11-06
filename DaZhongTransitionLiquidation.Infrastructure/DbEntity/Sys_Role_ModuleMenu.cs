using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.DbEntity
{
    public class Sys_Role_ModuleMenu
    {
        public Guid VGUID { get; set; }
        public Guid RoleVGUID { get; set; }
        public Guid ModuleMenuVGUD { get; set; }
        public int ModuleDataType { get; set; } = 1;
        public bool Look { get; set; }
        public bool New { get; set; }
        public bool Edit { get; set; }
        public bool StrikeOut { get; set; }
        public bool Obsolete { get; set; }
        public bool Submit { get; set; }
        public bool Review { get; set; }
        public bool GoBack { get; set; }
        public bool Import { get; set; }
        public bool Export { get; set; }
        public bool Generate { get; set; }
        public bool Calculation { get; set; }
        public string ComOrMan { get; set; }
        public bool Preview { get; set; }
        public bool Enable { get; set; }
    }
}
