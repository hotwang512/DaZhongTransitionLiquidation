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

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetsMaintenance
{
    public class AssetsMaintenanceController : BaseController
    {
        public AssetsMaintenanceController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetManagement/AssetsMaintenance
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetAssetMaintenanceInfoListDatas(Business_AssetMaintenanceInfo searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_AssetMaintenanceInfo>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_AssetMaintenanceInfo>()
                .WhereIF(searchParams.TAG_NUMBER != null, i => i.TAG_NUMBER == searchParams.TAG_NUMBER)
                .WhereIF(searchParams.ASSET_CATEGORY_MAJOR != null, i => i.ASSET_CATEGORY_MAJOR == searchParams.ASSET_CATEGORY_MAJOR)
                .WhereIF(searchParams.ASSET_CATEGORY_MINOR != null, i => i.ASSET_CATEGORY_MINOR == searchParams.ASSET_CATEGORY_MINOR)
                .WhereIF(searchParams.STATUS != null, i => i.STATUS == searchParams.STATUS)
                .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteAssetMaintenanceInfo(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //删除主表信息
                saveChanges = db.Deleteable<Business_AssetMaintenanceInfo>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}