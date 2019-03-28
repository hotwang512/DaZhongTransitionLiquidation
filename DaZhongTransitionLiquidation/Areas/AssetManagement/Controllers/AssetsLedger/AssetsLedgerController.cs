using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetsLedger
{
    public class AssetsLedgerController : BaseController
    {
        public AssetsLedgerController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetManagement/AssetsLedger
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetAssetsLedgerListDatas(DateTime? StartDate, DateTime? EndDate, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_AssetsLedger_Swap>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_AssetsLedger_Swap>()
                    .WhereIF(StartDate != null, i => i.LAST_UPDATE_DATE >= StartDate)
                    .WhereIF(EndDate != null, i => i.LAST_UPDATE_DATE <= EndDate)
                .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}