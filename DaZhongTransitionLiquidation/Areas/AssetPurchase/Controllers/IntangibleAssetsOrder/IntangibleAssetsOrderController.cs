﻿using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
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

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.IntangibleAssetsOrder
{

    public class IntangibleAssetsOrderController : BaseController
    {
        public IntangibleAssetsOrderController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetManagement/IntangibleAssetsOrder
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetIntangibleAssetsOrderListDatas(Business_IntangibleAssetsOrder searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_IntangibleAssetsOrder>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_IntangibleAssetsOrder>()
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteIntangibleAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                var isAnySubmited = db.Queryable<Business_IntangibleAssetsOrder>().Any(c => vguids.Contains(c.VGUID) && (c.SubmitStatus == IntangibleAssetsSubmitStatusEnum.TailPaymentUnSubmit.TryToInt() || c.SubmitStatus == IntangibleAssetsSubmitStatusEnum.Submited.TryToInt()));
                if (isAnySubmited)
                {
                    resultModel.ResultInfo = "存在已提交的订单，订单提交后不允许删除";
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                }
                else
                {
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_IntangibleAssetsOrder>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult SubmitIntangibleAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    foreach (var vguid in vguids)
                    {
                        var model = db.Queryable<Business_IntangibleAssetsOrder>().Where(c => c.VGUID == vguid).First();
                        model.SubmitStatus = model.SubmitStatus == IntangibleAssetsSubmitStatusEnum.FirstPaymentUnSubmit.TryToInt() ? IntangibleAssetsSubmitStatusEnum.TailPaymentUnSubmit.TryToInt() : IntangibleAssetsSubmitStatusEnum.Submited.TryToInt();
                        model.SubmitDate = DateTime.Now;
                        model.SubmitUser = cache[PubGet.GetUserKey].UserName;
                        db.Updateable<Business_IntangibleAssetsOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                        //提交后写入[Business_OrderListDraft]表
                        var draft = new Business_OrderListDraft();
                        draft.VGUID = Guid.NewGuid();
                        draft.PaymentCompany = model.PaymentInformation;
                        draft.PaymentMethod = model.PayType;
                        draft.CreateTime = DateTime.Now;
                        draft.OrderBankAccouont = model.CompanyBankAccount;
                        draft.OrderBankAccouontName = model.CompanyBankAccountName;
                        draft.CollectBankAccountName = model.SupplierBankAccountName;
                        draft.CollectBankAccouont = model.SupplierBankAccount;
                        draft.OrderBankName = model.CompanyBankName;
                        draft.CollectBankName = model.SupplierBank;
                        draft.OrderCompany = model.CompanyBankAccountName;
                        draft.CollectBankNo = model.SupplierBankNo;
                        if (model.SubmitStatus == IntangibleAssetsSubmitStatusEnum.FirstPaymentUnSubmit.TryToInt())
                        {
                            draft.Money = model.FirstPayment;
                            draft.PaymentContents = "首付款";
                        }
                        else if(model.SubmitStatus == IntangibleAssetsSubmitStatusEnum.TailPaymentUnSubmit.TryToInt())
                        {
                            draft.Money = model.TailPayment;
                            draft.PaymentContents = "尾款";
                        }
                        db.Insertable<Business_OrderListDraft>(draft).ExecuteCommand();
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