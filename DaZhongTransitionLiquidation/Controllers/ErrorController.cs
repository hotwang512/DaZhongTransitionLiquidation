using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using SqlSugar;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class ErrorController : BaseController
    {
        public ErrorController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: Error
        public ActionResult Index()
        {
            return View();
        }
    }
}