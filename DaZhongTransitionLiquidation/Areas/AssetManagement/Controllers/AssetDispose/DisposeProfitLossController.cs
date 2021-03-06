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
    public class DisposeProfitLossController : BaseController
    {
        // GET: AssetManagement/DisposeProfitLoss
        public DisposeProfitLossController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        // GET: AssetManagement/DisposeNetValue
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("fc52f562-f549-4bcd-b0a1-5eb2886e491d");
            return View();
        }
        public JsonResult GetAssetsDisposeProfitLossListDatas(string PlateNumber, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_DisposeProfitLoss>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_DisposeProfitLoss>()
                    .WhereIF(PlateNumber != null, i => i.DepartmentVehiclePlateNumber.Contains(PlateNumber) || i.OraclePlateNumber.Contains(PlateNumber))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ImportDisposeProfitLossFile(HttpPostedFileBase File)
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

        /// <summary>
        /// 计算损益
        /// </summary>
        /// <returns></returns>
        public JsonResult ComputeProfitLoss()
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    //计算损益
                    db.Ado.ExecuteCommand(@"update dbo.Business_DisposeProfitLoss set RealizedProfitLoss = Price + Taxes + DriverRentCarFee");
                    //同步主表
                    db.Ado.ExecuteCommand(@"update  info set info.DISPOSAL_AMOUNT = loss.Price, info.DISPOSAL_TAX = loss.Taxes, info.DISPOSAL_PROFIT_LOSS = loss.RealizedProfitLoss from Business_AssetMaintenanceInfo as info left join Business_DisposeProfitLoss loss on info.PLATE_NUMBER = loss.DepartmentVehiclePlateNumber");
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}