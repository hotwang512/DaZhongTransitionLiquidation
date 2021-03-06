﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetDispose
{
    public class DisposeTaxController : BaseController
    {
        public DisposeTaxController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/DisposeTax
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("9946bcc7-59b3-4374-af71-f2c7048c3c9b");
            return View();
        }
        public JsonResult GetAssetsDisposeTaxListDatas(string PlateNumber, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_DisposeTax>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_DisposeTax>()
                    .WhereIF(PlateNumber != null, i => i.DepartmentVehiclePlateNumber.Contains(PlateNumber) || i.OraclePlateNumber.Contains(PlateNumber))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ImportDisposeTaxFile(HttpPostedFileBase File)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
            if (File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "DisposeIncome\\";
                var filePath = AppDomain.CurrentDomain.BaseDirectory + uploadPath + newFileName;
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + uploadPath))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + uploadPath);
                }
                try
                {
                    File.SaveAs(filePath);
                    DbBusinessDataService.Command(db =>
                    {
                        var result = db.Ado.UseTran(() =>
                        {
                            
                        });
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", filePath, ex.ToString()));
                }
            }
            return Json(resultModel);
        }
    }
}