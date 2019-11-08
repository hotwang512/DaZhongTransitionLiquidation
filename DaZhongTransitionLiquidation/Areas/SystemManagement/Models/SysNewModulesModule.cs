using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Models
{
    public class SysNewModulesModule: Sys_Role_ModuleMenu
    {
        public string Name { get; set; }
        public Guid KeyVGUID { get; set; }
        public Guid Parent { get; set; }
        public bool IsLook { get; set; }
        public bool IsNew { get; set; }
        public bool IsEdit { get; set; }
        public bool IsStrikeOut { get; set; }
        public bool IsObsolete { get; set; }
        public bool IsSubmit { get; set; }
        public bool IsReview { get; set; }
        public bool IsGoBack { get; set; }
        public bool IsImport { get; set; }
        public bool IsExport { get; set; }
        public bool IsGenerate { get; set; }
        public bool IsCalculation { get; set; }
        public bool IsPreview { get; set; }
        public bool IsEnable { get; set; }
    }
}