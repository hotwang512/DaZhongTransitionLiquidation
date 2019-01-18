using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.ModuleManagement
{
    public class ModuleManagementController : BaseController
    {
        public ModuleManagementController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        // GET: SystemManagement/Module
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.AuthorityManagement);
            return View();
        }

        public ActionResult GetModules()
        {
            var jsonResult = new JsonResultModel<Sys_Module>();
            List<Sys_Module> sys_Modules = new List<Sys_Module>();
            DbService.Command(db =>
            {
                sys_Modules = db.Queryable<Sys_Module>().OrderBy(c => c.Zorder).ToList();
            });
            jsonResult.Rows = sys_Modules;
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}