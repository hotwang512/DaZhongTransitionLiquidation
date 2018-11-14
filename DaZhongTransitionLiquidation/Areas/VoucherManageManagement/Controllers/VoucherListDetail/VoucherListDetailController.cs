using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail
{
    public class VoucherListDetailController : BaseController
    {
        public VoucherListDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: VoucherManageManagement/VoucherList
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
    }
}