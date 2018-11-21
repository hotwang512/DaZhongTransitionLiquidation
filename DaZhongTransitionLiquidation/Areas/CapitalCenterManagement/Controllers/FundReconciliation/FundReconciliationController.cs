using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
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
namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.FundReconciliation
{
    public class FundReconciliationController : BaseController
    {
        public FundReconciliationController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: CapitalCenterManagement/FundReconciliation
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetFundReconciliationData(Business_FundReconciliation searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_FundReconciliation>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_FundReconciliation>()
                .OrderBy(i => i.BalanceDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteFundReconciliation(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };

            DbBusinessDataService.Command(db =>
            {
                var data = db.Queryable<Business_FundReconciliation>();
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    var isAny = data.Any(x => x.VGUID == item);
                    if (isAny)
                    {
                        saveChanges = db.Deleteable<Business_FundReconciliation>(x => x.VGUID == item).ExecuteCommand();
                    }
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult SaveFundReconciliation(Business_FundReconciliation sevenSection,bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var any = "";
                var result = db.Ado.UseTran(() =>
                {
                    var datas = db.Queryable<Business_BankFlowTemplate>();
                    var balanceDate = sevenSection.BalanceDate;//余额日期
                    var initialBalanceData = db.Queryable<Business_FundReconciliation>().OrderBy("BalanceDate desc").First();//最新一条数据
                    var bankBalance = sevenSection.BankBalance;//银行余额 
                    var date = balanceDate.ToString().Split(" ")[0];
                    var initialBalance = datas.First().Balance;//初始余额
                    var balanceDateEnd = Convert.ToDateTime(date + " 23:59:59");
                    decimal number = 0;
                    if (initialBalanceData != null)
                    {
                        if (initialBalanceData.BalanceDate != null)
                        {
                            //对账,日期只能后延
                            if (balanceDate < initialBalanceData.BalanceDate)
                            {
                                any = "2";
                                return;
                            }
                        }
                        if (initialBalanceData.ReconciliantStatus == "对账失败" && initialBalanceData.BalanceDate != sevenSection.BalanceDate)
                        {
                            any = "3";
                            return;
                        }
                        number = db.Ado.SqlQuery<decimal>(@"select (SUM(TurnIn)-SUM(TurnOut)) as number from Business_BankFlowTemplate where  
                                 TransactionDate>='" + initialBalanceData.BalanceDate.Value.AddDays(1) + "' and TransactionDate<='" + balanceDateEnd + "' ").FirstOrDefault();
                        if (bankBalance == (initialBalanceData.BankBalance + number))
                        {
                            sevenSection.ReconciliantStatus = "对账成功";
                        }
                        else
                        {
                            sevenSection.ReconciliantStatus = "对账失败";
                        }
                    }
                    else
                    {
                        number = db.Ado.SqlQuery<decimal>(@"select (SUM(TurnIn)-SUM(TurnOut)) as number from Business_BankFlowTemplate where  
                                 TransactionDate<='" + balanceDateEnd + "' ").FirstOrDefault();
                        if (bankBalance == (initialBalance + number))
                        {
                            sevenSection.ReconciliantStatus = "对账成功";
                        }
                        else
                        {
                            sevenSection.ReconciliantStatus = "对账失败";
                        }
                    }
                    if (isEdit)
                    {
                        db.Updateable(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.VGUID = Guid.NewGuid();
                        db.Insertable(sevenSection).ExecuteCommand();
                    } 
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                if (any != "")
                {
                    resultModel.Status = any;
                }
            });
            return Json(resultModel);
        }
    }
}