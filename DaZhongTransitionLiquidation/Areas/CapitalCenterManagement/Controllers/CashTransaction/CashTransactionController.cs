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

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashTransaction
{
    public class CashTransactionController : BaseController
    {
        public CashTransactionController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CashTransaction
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("3528ae93-d747-4feb-b64a-cf068b1c9e5c");
            return View();
        }
        public JsonResult GetCashTransaction(Business_CashTransaction searchParams, GridParams para, string TransactionDateEnd)
        {
            var jsonResult = new JsonResultModel<Business_CashTransaction>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                DateTime transactionDateEnd = Convert.ToDateTime(TransactionDateEnd + " 23:59:59");
                jsonResult.Rows = db.Queryable<Business_CashTransaction>()
                .WhereIF(searchParams.ReimbursementMan != null, i => i.ReimbursementMan == searchParams.ReimbursementMan)
                .WhereIF(searchParams.TransactionDate != null, i => i.TransactionDate >= searchParams.TransactionDate && i.TransactionDate <= transactionDateEnd)
                .WhereIF(searchParams.CompanyName != null, i => i.CompanyName.Contains(searchParams.CompanyName))
                .OrderBy(i => i.Batch, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}