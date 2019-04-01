using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SyntacticSugar;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.PurchaseOrderSetting
{
    public class PurchaseOrderSettingDetailController : BaseController
    {
        // GET: SystemManagement/PurchaseOrderSettingDetail
        public PurchaseOrderSettingDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }

        public JsonResult SavePurchaseOrderSetting(Business_PurchaseOrderSetting sevenSection)
        {
            var resultModel = new ResultModel<string>() {IsSuccess = false, Status = "0"};
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (sevenSection.VGUID == Guid.Empty)
                    {
                        sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.CreateDate = DateTime.Now;
                        sevenSection.CreateUser = cache[PubGet.GetUserKey].UserName;
                        db.Insertable<Business_PurchaseOrderSetting>(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.ChangeDate = DateTime.Now;
                        sevenSection.ChangeUser = cache[PubGet.GetUserKey].UserName;
                        db.Updateable<Business_PurchaseOrderSetting>(sevenSection)
                            .IgnoreColumns(x => new {x.CreateDate, x.CreateUser }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        public JsonResult GetPurchaseOrderSettingDetail(Guid vguid)
        {
            Business_PurchaseOrderSetting model = new Business_PurchaseOrderSetting();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                model = db.Queryable<Business_PurchaseOrderSetting>().Single(x => x.VGUID == vguid);
            });
            return Json(model, JsonRequestBehavior.AllowGet);
            ;
        }
        public JsonResult GetMinorListDatas(string MAJOR)
        {
            var list = new List<MinorListData>();
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_AssetsCategory>().WhereIF(MAJOR != null, i => i.ASSET_CATEGORY_MAJOR == MAJOR).Select(c => new MinorListData { AssetMinor = c.ASSET_CATEGORY_MINOR,AssetMinorVguid = c.VGUID}).ToList();
            });
            var result = list.GroupBy(c => new { c.AssetMinor,c.AssetMinorVguid }).Select(c => c.Key).ToList();
            return Json(result,JsonRequestBehavior.AllowGet);
        }
        
    }
}