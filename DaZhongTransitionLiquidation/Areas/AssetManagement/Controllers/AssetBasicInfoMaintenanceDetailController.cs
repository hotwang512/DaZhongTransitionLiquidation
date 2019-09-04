using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SyntacticSugar;
using SqlSugar;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetsMaintenance
{
    public class AssetBasicInfoMaintenanceDetailController : BaseController
    {
        public AssetBasicInfoMaintenanceDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/AssetBasicInfoMaintenanceDetail
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }

        public JsonResult SaveAssetBasicInfo(Business_AssetsCategory sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (sevenSection.VGUID == Guid.Empty)
                    {
                        sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.CREATE_TIME = DateTime.Now;
                        sevenSection.CREATE_USER = cache[PubGet.GetUserKey].LoginName;
                        db.Insertable<Business_AssetsCategory>(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.CHANGE_TIME = DateTime.Now;
                        sevenSection.CHANGE_USER = cache[PubGet.GetUserKey].LoginName;
                        db.Updateable<Business_AssetsCategory>(sevenSection).IgnoreColumns(x => new { x.CREATE_TIME, x.CREATE_USER }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult GetAssetBasicInfoDetail(Guid vguid)
        {
            Business_AssetsCategory model = new Business_AssetsCategory();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                model = db.Queryable<Business_AssetsCategory>().Single(x => x.VGUID == vguid);
            });
            return Json(model, JsonRequestBehavior.AllowGet); ;
        }
    }
}