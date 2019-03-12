using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class SharedController : BaseController
    {
        public SharedController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: Shared
        public ActionResult Error503()
        {
            return View();
        }
        public ActionResult Error404()
        {
            return View();
        }
        public ActionResult _Layout()
        {
            ViewBag.SysUser = GetSys_User();
            return View();
        }
        public Sys_User GetSys_User()
        {
            var result = new Sys_User();
            result = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            return result;
        }
    }
}