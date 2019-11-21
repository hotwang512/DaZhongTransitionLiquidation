using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashTransactionFlow
{
    public class CashTransactionFlowController : BaseController
    {
        public CashTransactionFlowController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CashTransactionFlow
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetCashTransactionFlow(Business_CashTransactionTemplate searchParams, GridParams para, string TransactionDateEnd)
        {
            var jsonResult = new JsonResultModel<Business_CashTransactionTemplate>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                DateTime transactionDateEnd = Convert.ToDateTime(TransactionDateEnd + " 23:59:59");
                jsonResult.Rows = db.Queryable<Business_CashTransactionTemplate>()
                .WhereIF(searchParams.TradingBank != null, i => i.BankAccount == searchParams.TradingBank)
                .WhereIF(searchParams.TransactionDate != null, i => i.TransactionDate >= searchParams.TransactionDate && i.TransactionDate <= transactionDateEnd)
                .WhereIF(searchParams.ReceivingUnit != null, i => i.ReceivingUnit.Contains(searchParams.ReceivingUnit))
                .WhereIF(UserInfo.LoginName != "admin", x => x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == UserInfo.CompanyCode)
                .Where(x => x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == UserInfo.CompanyCode)
                .OrderBy(i => i.TransactionDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}