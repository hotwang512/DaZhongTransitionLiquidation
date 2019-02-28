using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using SqlSugar;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetsMaintenance
{
    public class AssetBasicInfoMaintenanceController : BaseController
    {
        public AssetBasicInfoMaintenanceController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/AssetBasicInfoMaintenance
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetAssetBasicInfoListDatas(Business_AssetsCategory searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_AssetsCategory>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_AssetsCategory>()
                .WhereIF(searchParams.ASSET_CATEGORY_MAJOR != null, i => i.ASSET_CATEGORY_MAJOR == searchParams.ASSET_CATEGORY_MAJOR)
                .WhereIF(searchParams.ASSET_CATEGORY_MINOR != null, i => i.ASSET_CATEGORY_MINOR == searchParams.ASSET_CATEGORY_MINOR)
                .OrderBy(i => i.CREATE_TIME, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteAssetBasicInfo(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {

                int saveChanges = 1;
                //删除主表信息
                saveChanges = db.Deleteable<Business_AssetsCategory>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult GetMajorListDatas()
        {
            var list = new List<MajorListData>();
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_AssetsCategory>().Select(c => new MajorListData { AssetMajor = c.ASSET_CATEGORY_MAJOR }).ToList();
            });
            var result = list.GroupBy(c => new { c.AssetMajor }).Select(c => c.Key).ToList();
            return Json(result);
        }
    }
}