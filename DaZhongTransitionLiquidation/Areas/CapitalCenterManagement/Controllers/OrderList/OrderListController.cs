﻿using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BusinessTypeSet;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList
{
    public class OrderListController : BaseController
    {
        public OrderListController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/OrderList
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("63f132c3-b342-4642-a3a1-04f9f254e869");
            return View();
        }
        public JsonResult GetOrderListDatas(Business_OrderList searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_OrderList>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.SqlQueryable<Business_OrderList>(@" select a.BusinessSubItem1,a.BusinessProject,a.VGUID,b.BusinessType,b.Founder,b.Status,b.CollectionCompany,b.CollectionCompanyName,b.Number,b.PaymentMethod,b.AttachmentNumber,
b.InvoiceNumber,b.CollectionAccount,b.CollectionBankAccount,b.CollectionBankAccountName,b.CollectionBank,b.CompanyCode,b.OrderDetailValue from v_Business_BusinessTypeSet as a
left join Business_OrderList as b on a.VGUID = b.OrderDetailValue")
                //.Where(i => i.Status == searchParams.Status)
                .WhereIF(searchParams.BusinessProject != null, i => i.BusinessProject.Contains(searchParams.BusinessProject))
                .WhereIF(searchParams.CollectionCompany != null, i => i.CollectionCompany == searchParams.CollectionCompany)
                .OrderBy("BusinessSubItem1 asc").ToList();
                //var data = db.Queryable<Business_CustomerBankInfo>().ToList();
                //foreach (var item in jsonResult.Rows)
                //{
                //    try
                //    {
                //        var vguid = new Guid(item.CollectionBankAccountName);
                //        if (vguid != Guid.Empty)
                //        {
                //            var bankAccountName = data.Single(x => x.VGUID == vguid).BankAccountName;
                //            item.CollectionBankAccountName = bankAccountName;
                //        }
                //    }
                //    catch (Exception)
                //    {
                //        continue;
                //    }
                //}
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteOrderListInfo(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_OrderList>(x => x.VGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == 1;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult UpdataOrderListInfo(List<Guid> vguids, string status)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    //更新主表信息
                    saveChanges = db.Updateable<Business_OrderList>().UpdateColumns(it => new Business_OrderList()
                    {
                        Status = status,
                    }).Where(it => it.VGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
    }
}