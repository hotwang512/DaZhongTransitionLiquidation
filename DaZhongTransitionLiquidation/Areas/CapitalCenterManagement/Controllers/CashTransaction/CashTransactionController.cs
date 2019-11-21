using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashTransaction
{
    public class CashTransactionController : BaseController
    {
        public CashTransactionController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CashTransaction
        public ActionResult Index()
        {
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("3528ae93-d747-4feb-b64a-cf068b1c9e5c");
            return View();
        }
        public JsonResult GetCashTransaction(Business_CashTransaction searchParams, GridParams para, string TransactionDateEnd)
        {
            var jsonResult = new JsonResultModel<Business_CashTransaction>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                DateTime transactionDateEnd = Convert.ToDateTime(TransactionDateEnd + " 23:59:59");
                jsonResult.Rows = db.Queryable<Business_CashTransaction>()
                .WhereIF(searchParams.ReimbursementMan != null, i => i.ReimbursementMan == searchParams.ReimbursementMan)
                .WhereIF(searchParams.TransactionDate != null, i => i.TransactionDate >= searchParams.TransactionDate && i.TransactionDate <= transactionDateEnd)
                .WhereIF(searchParams.CompanyName != null, i => i.CompanyName.Contains(searchParams.CompanyName))
                .Where(i=>i.Status == searchParams.Status)
                .OrderBy(i => i.Batch, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdataCashTransaction(List<Guid> vguids, string status)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                db.Ado.UseTran(() =>
                {
                    int saveChanges = 1;
                    foreach (var item in vguids)
                    {
                        saveChanges = db.Updateable<Business_CashTransaction>().UpdateColumns(it => new Business_CashTransaction()
                        {
                            Status = status,
                            CreatePerson = UserInfo.LoginName,
                            CheckTime = DateTime.Now
                        }).Where(it => it.VGUID == item).ExecuteCommand();
                        if(status == "3")
                        {
                            //现金报销审核成功进入现金交易流水表
                            SetCashTransactionFlow(db,item);
                        }
                    }
                    resultModel.IsSuccess = saveChanges == 1;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                });
            });
            return Json(resultModel);
        }
        private void SetCashTransactionFlow(SqlSugarClient db,Guid vguid)
        {
            var cashData = db.Queryable<Business_CashTransaction>().Where(x => x.VGUID == vguid).First();
            var bankInfo = db.Queryable<Business_CompanyBankInfo>().Where(x => x.BankStatus == true && x.AccountModeCode == cashData.AccountModeCode && x.CompanyCode == cashData.CompanyCode).First();
            var cashFlow = new Business_CashTransactionTemplate();
            cashFlow.VGUID = Guid.NewGuid();
            cashFlow.AccountModeCode = cashData.AccountModeCode;
            cashFlow.AccountModeName = cashData.AccountModeName;
            cashFlow.CompanyCode = cashData.CompanyCode;
            cashFlow.CompanyName = cashData.CompanyName;
            cashFlow.TradingBank = "上海银行";
            cashFlow.PayeeAccount = bankInfo.BankAccount;
            cashFlow.PaymentUnitInstitution = bankInfo.BankName;
            cashFlow.Batch = cashData.Batch;
            cashFlow.TransactionDate = cashData.CheckTime;
            cashFlow.TurnOut = 0;//转入(贷)
            cashFlow.TurnIn = cashData.TurnOut;//转出(借)
            cashFlow.Balance = null;
            cashFlow.ReceivingUnit = cashData.ReimbursementMan;
            cashFlow.ReceivableAccount = "";
            cashFlow.ReceivingUnitInstitution = "";
            cashFlow.Purpose = cashData.Purpose;
            cashFlow.Remark = "";
            cashFlow.CreateTime = DateTime.Now;
            cashFlow.CreatePerson = UserInfo.LoginName;
            db.Insertable(cashFlow).ExecuteCommand();
        }
    }
}