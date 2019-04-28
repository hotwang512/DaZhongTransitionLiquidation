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
using DaZhongTransitionLiquidation.Models;


namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.FixedAssetsOrder
{
    public class FixedAssetsOrderController : BaseController
    {
        // GET: AssetManagement/FixedAssetsOrder
        public FixedAssetsOrderController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetFixedAssetsOrderListDatas(Business_FixedAssetsOrder searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_FixedAssetsOrder>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_FixedAssetsOrder>()
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteFixedAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //删除主表信息
                saveChanges = db.Deleteable<Business_FixedAssetsOrder>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult SubmitFixedAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    foreach (var vguid in vguids)
                    {
                        var model = db.Queryable<Business_FixedAssetsOrder>().Where(c => c.VGUID == vguid).First();
                        if (model.SubmitStatus == FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt())
                        {
                            model.SubmitStatus = FixedAssetsSubmitStatusEnum.Submited.TryToInt();
                            model.SubmitDate = DateTime.Now;
                            model.SubmitUser = cache[PubGet.GetUserKey].UserName;
                            db.Updateable<Business_FixedAssetsOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                            //提交完后写入采购分配表
                            var purchaseAssignmodel =  new Business_PurchaseAssign();
                            purchaseAssignmodel.VGUID = Guid.NewGuid();
                            purchaseAssignmodel.CreateDate = DateTime.Now;
                            purchaseAssignmodel.CreateUser = cache[PubGet.GetUserKey].UserName;
                            purchaseAssignmodel.FixedAssetsOrderVguid = model.VGUID;
                            purchaseAssignmodel.PurchaseGoodsVguid = model.PurchaseGoodsVguid;
                            purchaseAssignmodel.PurchaseGoods = model.PurchaseGoods;
                            purchaseAssignmodel.OrderQuantity = model.OrderQuantity;
                            purchaseAssignmodel.PurchasePrices = model.PurchasePrices;
                            purchaseAssignmodel.ContractAmount = model.ContractAmount;
                            purchaseAssignmodel.AssetDescription = model.AssetDescription;
                            db.Insertable<Business_PurchaseAssign>(purchaseAssignmodel).ExecuteCommand();
                            //提交后写入[Business_OrderListDraft]表
                            var draft = new Business_OrderListDraft();
                            draft.VGUID = Guid.NewGuid();
                            draft.PaymentCompany = model.PaymentInformation;
                            draft.Money = model.ContractAmount;
                            draft.PaymentMethod = model.PayType;
                            draft.CreateTime = DateTime.Now;
                            draft.PaymentContents = model.AssetDescription;
                            draft.OrderBankAccouont = model.CompanyBankAccount;
                            draft.OrderBankAccouontName = model.CompanyBankAccountName;
                            draft.CollectBankAccountName = model.SupplierBankAccountName;
                            draft.CollectBankAccouont = model.SupplierBankAccount;
                            draft.OrderBankName = model.CompanyBankName;
                            draft.CollectBankName = model.SupplierBank;
                            draft.OrderCompany = model.CompanyBankAccountName;
                            draft.CollectBankNo = model.SupplierBankNo;
                            db.Insertable<Business_OrderListDraft>(draft).ExecuteCommand();
                        }
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess.ObjToBool() ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}