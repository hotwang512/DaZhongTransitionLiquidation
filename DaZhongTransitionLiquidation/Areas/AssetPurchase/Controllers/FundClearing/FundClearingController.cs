using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;
using Business_AssetOrderDetails = DaZhongTransitionLiquidation.Areas.AssetPurchase.Models.Business_AssetOrderDetails;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.FundClearing
{
    public class FundClearingController : BaseController
    {
        // GET: AssetPurchase/FundClearing
        public FundClearingController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetPurchase/PurchaseAssign
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetListDatas(Business_FundClearing searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_FundClearing>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.SqlQueryable<Business_FundClearing>("SELECT pa.* FROM Business_FundClearing pa INNER JOIN (select * from Business_FixedAssetsOrder where SubmitStatus = 2) fao ON pa.FixedAssetsOrderVguid = fao.VGUID")//WHERE fao.SubmitStatus = 1
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCompanyData()
        {
            var list = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_SevenSection>()
                    .Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAssignCompany(Guid Vguid)
        {
            var list = new List<Business_LiquidationDistribution>();
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_LiquidationDistribution>().Where(x => x.FundClearingVguid == Vguid).ToList();
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteAssignCompany(Guid Vguid)
        {
            var resultModel = new ResultModel<string>() {IsSuccess = false,Status = "0"};
            DbBusinessDataService.Command(db =>
            {
                var saveChanges = db.Deleteable<Business_LiquidationDistribution>(x => x.VGUID == Vguid).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddAssign(Guid FundClearingVguid,Guid CompanyVguid,string Company,int AssetNum)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var FundClearingModel = db.Queryable<Business_FundClearing>().Where(x => x.VGUID == FundClearingVguid).First();
                var liquidationDistribution = new Business_LiquidationDistribution();
                liquidationDistribution.VGUID = Guid.NewGuid();
                liquidationDistribution.FundClearingVguid = FundClearingModel.VGUID;
                liquidationDistribution.AssetsOrderVguid = FundClearingModel.FixedAssetsOrderVguid;
                liquidationDistribution.CompanyVguid = CompanyVguid;
                liquidationDistribution.Company = Company;
                liquidationDistribution.PurchasePrices = FundClearingModel.PurchasePrices;
                liquidationDistribution.AssetNum = AssetNum;
                liquidationDistribution.ContractAmount = FundClearingModel.PurchasePrices * AssetNum;
                liquidationDistribution.CreateDate = DateTime.Now;
                liquidationDistribution.CreateUser = cache[PubGet.GetUserKey].UserName;
                db.Insertable<Business_LiquidationDistribution>(liquidationDistribution).ExecuteCommand();
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
            });
            return Json(resultModel);
        }
        public JsonResult SubmitAssign(Guid FundClearingVguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var fundClearingModel = db.Queryable<Business_FundClearing>().Where(x => x.VGUID == FundClearingVguid).First();
                fundClearingModel.SubmitStatus = 1;
                fundClearingModel.SubmitDate = DateTime.Now;
                fundClearingModel.SubmitUser = cache[PubGet.GetUserKey].UserName;
                db.Updateable<Business_FundClearing>(fundClearingModel)
                    .UpdateColumns(x => new {x.SubmitDate, x.SubmitStatus, x.SubmitUser}).ExecuteCommand();
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
            });
            return Json(resultModel);
        }
    }
}