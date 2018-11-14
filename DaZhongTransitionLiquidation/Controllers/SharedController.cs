using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class SharedController : Controller
    {
        // GET: Shared
        public ActionResult Error503()
        {
            return View();
        }
        public ActionResult Error404()
        {
            return View();
        }
    }
}