using DaZhongTransitionLiquidation.Infrastructure.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.HomePage.Controllers
{
    public class HomePageController : BaseController
    {
        public HomePageController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: HomePage/HomePage
        public ActionResult Index()
        {
            return View();
        }
    }
}