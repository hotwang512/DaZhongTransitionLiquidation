using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetException
{
    public class AssetExceptionController : BaseController
    {
        // GET: AssetManagement/AssetException
        public AssetExceptionController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(
            dbService, dbBusinessDataService)
        {
        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("f0ed636a-5001-4393-9bbc-5c9dd195f64b");
            return View();
        }

        public JsonResult GetExceptionAssetListDatas(GridParams para)
        {
            var jsonResult = new JsonResultModel<AssetMaintenanceInfo_Swap>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<AssetMaintenanceInfo_Swap>()
                    .Where(x => x.STATUS == "E" && !x.CHECK_STATE)
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc)
                    .ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SubmitExceptionAsset(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var exceptionList = db.Queryable<AssetMaintenanceInfo_Swap>()
                    .Where(i => vguids.Contains(i.TRANSACTION_ID))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToList();
                try
                {
                    foreach (var exceptionItem in exceptionList)
                    {
                        var assetInfo = db.Queryable<Business_AssetMaintenanceInfo>()
                            .Where(x => x.ASSET_ID == exceptionItem.ASSET_ID).First();
                        //NEW_ASSET,PLATE_NUMBER,FA_LOC_1,FA_LOC_3
                        if (exceptionItem.PROCESS_TYPE == "NEW_ASSET")
                        {
                            
                        }
                        else if (exceptionItem.PROCESS_TYPE == "PLATE_NUMBER")
                        {
                            assetInfo.PLATE_NUMBER = exceptionItem.TAG_NUMBER.Split("-")[0].ToString();
                        }
                        else if (exceptionItem.PROCESS_TYPE == "FA_LOC_1")
                        {
                            assetInfo.BELONGTO_COMPANY = exceptionItem.FA_LOC_1;
                        }
                        else if (exceptionItem.PROCESS_TYPE == "FA_LOC_3")
                        {
                            assetInfo.ORGANIZATION_NUM = exceptionItem.FA_LOC_3;
                        }
                        else if (exceptionItem.PROCESS_TYPE == "RETIRE")
                        {
                            assetInfo.BACK_CAR_DATE = exceptionItem.RETIRE_DATE;
                        }
                        assetInfo.ASSET_CATEGORY_MAJOR = exceptionItem.ASSET_CATEGORY_MAJOR;
                        assetInfo.ASSET_CATEGORY_MINOR = exceptionItem.ASSET_CATEGORY_MINOR;
                        assetInfo.ASSET_COST = exceptionItem.ASSET_COST;
                        assetInfo.METHOD = exceptionItem.METHOD;
                        assetInfo.BELONGTO_COMPANY = exceptionItem.FA_LOC_1;
                        assetInfo.MANAGEMENT_COMPANY = exceptionItem.FA_LOC_2;
                        assetInfo.ORGANIZATION_NUM = exceptionItem.FA_LOC_3;
                        assetInfo.MODEL_MAJOR = exceptionItem.MODEL_MAJOR;
                        assetInfo.MODEL_MINOR = exceptionItem.MODEL_MINOR;
                        db.Updateable<Business_AssetMaintenanceInfo>().UpdateColumns(x => new
                        {
                            x.ASSET_CATEGORY_MAJOR,
                            x.ASSET_CATEGORY_MINOR,
                            x.ASSET_COST,
                            x.METHOD,
                            x.BELONGTO_COMPANY,
                            x.MANAGEMENT_COMPANY,
                            x.MODEL_MAJOR,
                            x.MODEL_MINOR,
                            x.ORGANIZATION_NUM
                        }).ExecuteCommand();
                        exceptionItem.TRANSACTION_ID = Guid.NewGuid();
                        exceptionItem.CREATE_DATE = DateTime.Now;
                        exceptionItem.LAST_UPDATE_DATE = DateTime.Now;
                        exceptionItem.STATUS = "N";
                        db.Insertable<AssetMaintenanceInfo_Swap>(exceptionItem).ExecuteCommand();
                    }
                    resultModel.IsSuccess = true;
                    resultModel.Status = "1";
                }
                catch (Exception ex)
                {
                    throw;
                }

            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateAssetSwap(AssetMaintenanceInfo_Swap swap)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                if (swap.PROCESS_TYPE == "NEW_ASSET")
                {
                    db.Updateable<AssetMaintenanceInfo_Swap>().UpdateColumns(x => new
                    {
                        x.ASSET_CATEGORY_MAJOR,
                        x.ASSET_CATEGORY_MINOR,
                        x.ASSET_COST,
                        x.METHOD,
                        x.FA_LOC_1,
                        x.FA_LOC_2,
                        x.FA_LOC_3,
                        x.DESCRIPTION
                    }).ExecuteCommand();
                }
                else if (swap.PROCESS_TYPE == "PLATE_NUMBER")
                {
                    db.Updateable<AssetMaintenanceInfo_Swap>().UpdateColumns(x => new
                    {
                        x.TAG_NUMBER
                    }).ExecuteCommand();
                }
                else if (swap.PROCESS_TYPE == "FA_LOC_1")
                {
                    db.Updateable<AssetMaintenanceInfo_Swap>().UpdateColumns(x => new
                    {
                        x.FA_LOC_1
                    }).ExecuteCommand();
                }
                else if (swap.PROCESS_TYPE == "FA_LOC_3")
                {
                    db.Updateable<AssetMaintenanceInfo_Swap>().UpdateColumns(x => new
                    {
                        x.FA_LOC_3
                    }).ExecuteCommand();
                }
                else if (swap.PROCESS_TYPE == "RETIRE")
                {
                    db.Updateable<AssetMaintenanceInfo_Swap>().UpdateColumns(x => new
                    {
                        x.RETIRE_DATE
                    }).ExecuteCommand();
                }
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}