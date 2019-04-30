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
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.PurchaseOrderSetting
{
    public class PurchaseOrderSettingController : BaseController
    {
        // GET: SystemManagement/PurchaseOrderSetting
        public PurchaseOrderSettingController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetManagement/AssetsLedger
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetPurchaseOrderSettingListDatas(Business_PurchaseOrderSetting searchModel, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_PurchaseOrderSetting>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_PurchaseOrderSetting>()
                    .WhereIF(!searchModel.PurchaseGoods.IsNullOrEmpty(), i => i.PurchaseGoods.Contains(searchModel.PurchaseGoods))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPurchaseSupplierListDatas(Guid Vguid, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_BankPurchaseSupplier>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<v_BankPurchaseSupplier>()
                    .WhereIF(Vguid != Guid.Empty, i => i.PurchaseOrderSettingVguid == Vguid)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
      
        
        public JsonResult GetCustomerBankInfo(Guid OrderSettingVguid, string BankAccount, string BankCategory, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_BankInfoSetting>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.SqlQueryable<v_BankInfoSetting>(@"SELECT DISTINCT bcbi.*,
                    bps.PurchaseOrderSettingVguid,
                    CASE
                        WHEN bps.VGUID IS NULL THEN
                    'NoCheck'
                    ELSE
                    'Checked'
                    END AS IsCheck
                        FROM Business_CustomerBankInfo bcbi
                    LEFT JOIN(SELECT * FROM Business_PurchaseSupplier WHERE PurchaseOrderSettingVguid = '"+ OrderSettingVguid + @"') bps
                        ON bps.CustomerBankInfoVguid = bcbi.VGUID " ).WhereIF(!string.IsNullOrEmpty(BankCategory) && BankCategory != "请选择", i => i.CompanyOrPerson.Contains(BankCategory))
                    .WhereIF(!string.IsNullOrEmpty(BankAccount), i => i.BankAccount.Contains(BankAccount))
                    .Where(i => i.PurchaseOrderSettingVguid == null || i.PurchaseOrderSettingVguid == OrderSettingVguid).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;

            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeletePurchaseOrderSetting(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //删除主表信息
                saveChanges = db.Deleteable<Business_PurchaseOrderSetting>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult SetPurchaseSupplier(List<Guid> selvguids, List<Guid> allvguids, string CustomerBankInfoCategory,Guid PurchaseOrderSettingVguid)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            allvguids.RemoveAll(c => selvguids.Contains(c.TryToGuid()));
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    foreach (var item in selvguids)
                    {
                        if (!db.Queryable<Business_PurchaseSupplier>().Any(c => c.CustomerBankInfoVguid == item && c.PurchaseOrderSettingVguid == PurchaseOrderSettingVguid))
                        {
                            var model = new Business_PurchaseSupplier();
                            model.VGUID = Guid.NewGuid();
                            model.PurchaseOrderSettingVguid = PurchaseOrderSettingVguid;
                            model.CustomerBankInfoCategory = CustomerBankInfoCategory;
                            model.CustomerBankInfoVguid = item;
                            model.CreateDate = DateTime.Now;
                            model.CreateUser = cache[PubGet.GetUserKey].UserName;
                            db.Insertable<Business_PurchaseSupplier>(model).ExecuteCommand();
                        }
                    }
                    if (allvguids.Count > 0)
                    {
                        foreach (var item in allvguids)
                        {
                            db.Deleteable<Business_PurchaseSupplier>().Where(c => c.PurchaseOrderSettingVguid == PurchaseOrderSettingVguid && c.CustomerBankInfoVguid == item).ExecuteCommand();
                        }
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult GetBankCategoryListDatas()
        {
            var list = new List<BankCategoryListData>();
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_CustomerBankInfo>().Select(c => new BankCategoryListData { CompanyOrPerson = c.CompanyOrPerson}).ToList();
            });
            var result = list.GroupBy(c => new { c.CompanyOrPerson}).Select(c => c.Key).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}