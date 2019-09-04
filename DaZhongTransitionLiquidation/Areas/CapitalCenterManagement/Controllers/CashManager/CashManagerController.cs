using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashManager
{
    public class CashManagerController : BaseController
    {
        public CashManagerController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CashManager
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("aa345b36-259a-48f3-a5ec-5abc950b085d");
            return View();
        }
        public JsonResult GetCashManagerData(Business_CashManagerInfo searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_CashManagerInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_CashManagerInfo>()
                .WhereIF(searchParams.BankAccount != null, i => i.BankAccount == searchParams.BankAccount)
                .WhereIF(searchParams.ApplyDate != null, i => i.ApplyDate == searchParams.ApplyDate)
                .OrderBy(i => i.No, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}