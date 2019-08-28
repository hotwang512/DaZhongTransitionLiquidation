using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Aspose.Cells;
using DaZhongTransitionLiquidation.Common;

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
        public JsonResult GetAssetsDisposeIncomeListDatas(string PERIOD, string TagNumber, string CategoryMajor, string CategoryMinor, GridParams para)
        {
            var jsonResult = new JsonResultModel<AssetsLedger_Swap>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<AssetsLedger_Swap>()
                    .WhereIF(PERIOD != null, i => i.PERIOD_CODE.Contains(PERIOD))
                    .WhereIF(TagNumber != null, i => i.TAG_NUMBER.Contains(TagNumber))
                    .WhereIF(CategoryMajor != null, i => i.ASSET_CATEGORY_MAJOR.Contains(CategoryMajor))
                    .WhereIF(CategoryMinor != null, i => i.ASSET_CATEGORY_MINOR.Contains(CategoryMinor))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}