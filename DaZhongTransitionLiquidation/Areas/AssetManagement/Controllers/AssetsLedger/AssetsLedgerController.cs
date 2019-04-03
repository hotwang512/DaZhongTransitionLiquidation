﻿using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
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
        public FileResult ExportExcel(DateTime? StartDate, DateTime? EndDate)
        {
            DataTable dt = new DataTable();
                DbBusinessDataService.Command(db =>
                {
                    dt = db.Queryable<Business_AssetsLedger_Swap>()
                        .WhereIF(StartDate != null, i => i.LAST_UPDATE_DATE >= StartDate)
                        .WhereIF(EndDate != null, i => i.LAST_UPDATE_DATE <= EndDate)
                        .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToDataTable();
                });
                dt.TableName = "Business_AssetsLedger_Swap";
                var ms = ExcelHelper.OutModelFileToStream(dt, "/Template/AssetsLedger.xlsx", "资产台账");
                byte[] fileContents = ms.ToArray();
                return File(fileContents, "application/ms-excel", "资产台账" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        }
    }
}