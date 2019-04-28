using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
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
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.PurchaseAssign
{
    public class PurchaseAssignController: BaseController
    {
        public PurchaseAssignController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetPurchase/PurchaseAssign
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetBusiness_PurchaseAssignListDatas(Business_PurchaseAssign searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_PurchaseAssign>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_PurchaseAssign>()
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAssetOrderDetails(string AssetType, Guid AssetsOrderVguid)
        {
            var listFixedAssetsOrder = new List<Business_AssetOrderDetails>();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                if (db.Queryable<Business_AssetOrderDetails>().Any(x => x.AssetsOrderVguid == AssetsOrderVguid))
                {
                    listFixedAssetsOrder = db.Queryable<Business_AssetOrderDetails>().Where(x => x.AssetsOrderVguid == AssetsOrderVguid).ToList();
                }
            });
            return Json(listFixedAssetsOrder, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult GetOrderBelong(Guid AssetsOrderVguid)
        {
            var listFixedAssetsOrder = new List<Business_AssetOrderBelongToShow>();
            DbBusinessDataService.Command(db =>
            {
                listFixedAssetsOrder = db.SqlQueryable<Business_AssetOrderBelongToShow>(
                        @"SELECT belongto.*,assetsorder.PurchasePrices FROM (
                        SELECT SUM(AssetNum) AS AssetNum,BelongToCompany,AssetsOrderVguid FROM Business_AssetOrderBelongTo
                        WHERE AssetsOrderVguid =  '" + AssetsOrderVguid + @"'
                         GROUP BY BelongToCompany,AssetsOrderVguid) belongto LEFT JOIN dbo.Business_FixedAssetsOrder assetsorder ON
                         belongto.AssetsOrderVguid = assetsorder.VGUID")
                    .ToList();
            });
            return Json(listFixedAssetsOrder, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPurchaseAssign(Business_AssetOrderBelongTo searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_AssetOrderBelongTo>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_AssetOrderBelongTo>().Where(x => x.AssetOrderDetailsVguid == searchParams.AssetOrderDetailsVguid)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBelongToCompany()
        {
            var list = new List<BelongToCompanyModel>();
            list.Add(new BelongToCompanyModel { BelongToCompany = "集团" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "虹口" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "奉贤" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "新亚" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "交运" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "万祥" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "营管部" });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveBelongToRow(Guid? vguid, Guid AssetOrderDetailsVguid,Guid AssetsOrderVguid,int AssetNum, string BelongToCompany)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (!vguid.IsNullOrEmpty())
                    {
                        var saveObj = db.Queryable<Business_AssetOrderBelongTo>().Where(c => c.VGUID == vguid).First();
                        saveObj.ChangeDate = DateTime.Now;
                        saveObj.ChangeUser = cache[PubGet.GetUserKey].UserName;
                        saveObj.AssetNum = AssetNum;
                        saveObj.BelongToCompany = BelongToCompany;
                        db.Updateable(saveObj).IgnoreColumns(it => new { it.CreateDate,it.CreateUser }).ExecuteCommand();
                    }
                    else
                    {
                        var saveObj = new Business_AssetOrderBelongTo();
                        saveObj.VGUID = Guid.NewGuid();
                        saveObj.CreateDate = DateTime.Now;
                        saveObj.CreateUser = cache[PubGet.GetUserKey].UserName;
                        saveObj.AssetNum = AssetNum;
                        saveObj.BelongToCompany = BelongToCompany;
                        saveObj.AssetOrderDetailsVguid = AssetOrderDetailsVguid;
                        saveObj.AssetsOrderVguid = AssetsOrderVguid;
                        var orderDetails = db.Queryable<Business_AssetOrderDetails>().Where(c => c.VGUID == AssetOrderDetailsVguid).First();
                        saveObj.AssetManagementCompany = orderDetails.AssetManagementCompany;
                        db.Insertable<Business_AssetOrderBelongTo>(saveObj).ExecuteCommand();
                        var assign = db.Queryable<Business_PurchaseAssign>().Where(c => c.FixedAssetsOrderVguid == AssetsOrderVguid).First();
                        if (assign.SubmitStatus == 0)
                        {
                            assign.SubmitStatus = 1;
                            db.Updateable<Business_PurchaseAssign>(assign).UpdateColumns(x => new { x.SubmitStatus}).ExecuteCommand();
                        }
                    }
                    
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult DeleteBelongToRow(Guid vguid)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //删除主表信息
                saveChanges = db.Deleteable<Business_AssetOrderBelongTo>(x => x.VGUID == vguid).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}