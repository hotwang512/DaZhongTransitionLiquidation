using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.DbEntity
{
    public class Sys_ModuleMenu
    {
        public Guid VGUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public Guid? Parent { get; set; }
        public int Type { get; set; } = 0;
        public string Url { get; set; }
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
        public int Zorder { get; set; } = 0;
        public List<Sys_ModuleMenu> children { get; set; } = new List<Sys_ModuleMenu>();

    }
}
