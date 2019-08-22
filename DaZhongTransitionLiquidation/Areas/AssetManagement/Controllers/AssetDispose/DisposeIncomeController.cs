using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetDisposeIncome
{
    public class DisposeIncomeController : BaseController
    {
        // GET: AssetManagement/DisposeIncome
        public DisposeIncomeController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/AssetBasicInfoMaintenance
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
    }
}