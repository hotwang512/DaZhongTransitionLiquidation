using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement
{
    public class BankFlowTemplateController : BaseController
    {
        public BankFlowTemplateController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/BankFlowTemplate
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetBankFlowData(Business_BankFlowTemplate searchParams, GridParams para,string TransactionDateEnd)
        {
            var jsonResult = new JsonResultModel<Business_BankFlowTemplate>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                DateTime transactionDateEnd = Convert.ToDateTime(TransactionDateEnd + " 23:59:59");
                jsonResult.Rows = db.Queryable<Business_BankFlowTemplate>()
                .WhereIF(searchParams.TradingBank != null, i => i.TradingBank == searchParams.TradingBank)
                .WhereIF(searchParams.TransactionDate != null, i => i.TransactionDate >= searchParams.TransactionDate && i.TransactionDate <= transactionDateEnd)
                .WhereIF(searchParams.PaymentUnit != null, i => i.PaymentUnit == searchParams.PaymentUnit)
                .OrderBy(i => i.TransactionDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveBankFlow(Business_BankFlowTemplate sevenSection, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var guid = sevenSection.VGUID;
                    if (isEdit)
                    {
                        db.Updateable<Business_BankFlowTemplate>().UpdateColumns(it => new Business_BankFlowTemplate()
                        {
                            VoucherSubjectName = sevenSection.VoucherSubjectName,
                            VoucherSubject = sevenSection.VoucherSubject,
                            VoucherSummary = sevenSection.VoucherSummary,
                        }).Where(it => it.VGUID == guid).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult GetCompanySection(GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_SevenSection>();
            var response = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                //int pageCount = 0;               
                para.pagenum = para.pagenum + 1;
                response = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43" && x.Code != null && x.Status == "1")
                .OrderBy(i => i.Code, OrderByType.Asc).ToList();
                jsonResult.TotalRows = response.Count;
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SyncCurrentDayBankData()
        {
            ResultModel<string> resultModel = null;
            List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
            DbBusinessDataService.Command(db =>
            {
                resultModel = new ResultModel<string>() { IsSuccess = true, Status = "1" };
                bankFlowList = ShanghaiBankAPI.GetShangHaiBankTradingFlow();
                foreach (var item in bankFlowList)
                {
                    var isAny = db.Queryable<Business_BankFlowTemplate>().Any(x => x.Batch == item.Batch);
                    if (isAny)
                    {
                        continue;
                    }
                    item.CreateTime = DateTime.Now;
                    item.CreatePerson = "sysAdmin";
                    db.Insertable<Business_BankFlowTemplate>(item).ExecuteCommand();
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SyncYesterdayBankData()
        {
            ResultModel<string> resultModel = null;
            List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
            DbBusinessDataService.Command(db =>
            {
                resultModel = new ResultModel<string>() { IsSuccess = true, Status = "1" };
                bankFlowList = ShanghaiBankAPI.GetShangHaiBankYesterdayTradingFlow();
                foreach (var item in bankFlowList)
                {
                    var isAny = db.Queryable<Business_BankFlowTemplate>().Any(x => x.Batch == item.Batch);
                    if (isAny)
                    {
                        continue;
                    }
                    item.CreateTime = DateTime.Now;
                    item.CreatePerson = "sysAdmin";
                    db.Insertable<Business_BankFlowTemplate>(item).ExecuteCommand();
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}