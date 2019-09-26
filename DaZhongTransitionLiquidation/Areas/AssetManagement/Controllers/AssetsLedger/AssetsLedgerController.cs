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
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("675f1c4c-5ad6-4930-b227-b31d19a98c05");
            return View();
        }
        public JsonResult GetAssetsLedgerListDatas(string PERIOD, string TagNumber, string CategoryMajor, string CategoryMinor, GridParams para)
        {
            var jsonResult = new JsonResultModel<AssetsLedger_Swap>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<AssetsLedger_Swap>()
                    .PartitionBy(it => new { it.ASSET_ID }).Take(1)
                    .WhereIF(PERIOD != null, i => i.PERIOD_CODE.Contains(PERIOD))
                    .WhereIF(TagNumber != null, i => i.TAG_NUMBER.Contains(TagNumber))
                    .WhereIF(CategoryMajor != null, i => i.ASSET_CATEGORY_MAJOR.Contains(CategoryMajor))
                    .WhereIF(CategoryMinor != null, i => i.ASSET_CATEGORY_MINOR.Contains(CategoryMinor))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public FileResult ExportExcel(string PERIOD, string TagNumber, string CategoryMajor, string CategoryMinor)
        {
            DataTable dt = new DataTable();
            DbBusinessDataService.Command(db =>
            {
                dt = db.Queryable<AssetsLedger_Swap>()
                    .PartitionBy(it => new { it.ASSET_ID }).Take(1)
                    .WhereIF(PERIOD != null, i => i.PERIOD_CODE.Contains(PERIOD))
                    .WhereIF(TagNumber != null, i => i.TAG_NUMBER.Contains(TagNumber))
                    .WhereIF(CategoryMajor != null, i => i.ASSET_CATEGORY_MAJOR.Contains(CategoryMajor))
                    .WhereIF(CategoryMinor != null, i => i.ASSET_CATEGORY_MINOR.Contains(CategoryMinor))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToDataTable();
            });
            dt.TableName = "AssetsLedger_Swap";
            var ms = ExcelHelper.OutModelFileToStream(dt, "/Template/AssetsLedger.xlsx", "资产台账");
            byte[] fileContents = ms.ToArray();
            return File(fileContents, "application/ms-excel", "资产台账" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        }
    }
}