using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
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
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("03066773-A699-4E7C-92D7-05AF0A211FF3");
            ViewBag.CompanyCode = GetCompanyCode();
            return View();
        }
        public JsonResult GetFundReconciliationData(Business_FundReconciliation searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_FundReconciliation>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_FundReconciliation>().Where(x=>x.CompanyName == UserInfo.CompanyName)
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
                int saveChanges = 0;
                foreach (var item in vguids)
                { 
                    var isAny = data.Any(x => x.VGUID == item);
                    if (isAny)
                    {
                        saveChanges = db.Deleteable<Business_FundReconciliation>(x => x.VGUID == item).ExecuteCommand();
                    }
                }
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
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
                    var companyCode = sevenSection.CompanyCode;//公司
                    var bankAccount = sevenSection.BankAccount;//银行账号
                    var initialBalance = db.Queryable<Business_CompanyBankInfo>().Where(x=>x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == companyCode && x.BankAccount == bankAccount).First().InitialBalance;//初始余额

                    var bankBalance = sevenSection.BankBalance;//银行余额 
                    var balanceDate = sevenSection.BalanceDate;//余额日期
                    var date = balanceDate.ToString().Split(" ")[0];
                    var balanceDateEnd = Convert.ToDateTime(date + " 23:59:59");//结束日期
                    var initialBalanceData = db.Queryable<Business_FundReconciliation>().Where(x=>x.ReconciliantStatus == "对账成功" && x.CompanyCode == companyCode 
                                              && x.BankAccount == bankAccount).OrderBy("BalanceDate desc").First();//当前公司银行账号下最新一条数据
                    decimal number = 0;
                    decimal sysBalance = 0;
                    var balanceDateStar = initialBalanceData.BalanceDate.Value.AddDays(1);//开始日期
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
                        number = db.Ado.SqlQuery<decimal>(@"select (SUM(TurnOut)-SUM(TurnIn)) as number from Business_BankFlowTemplate where BankAccount = '" + bankAccount + @"'  
                                 and TransactionDate >= '" + balanceDateStar + "' and TransactionDate<='" + balanceDateEnd + "' ").FirstOrDefault();
                        sysBalance = initialBalanceData.BankBalance.TryToDecimal() + number;
                        if (bankBalance == sysBalance)
                        {
                            sevenSection.BalanceDate = (sevenSection.BalanceDate.Value.ToString("yyyy-MM-dd") + " " + DateTime.Now.ToString("HH:mm:ss")).TryToDate();
                            sevenSection.ReconciliantStatus = "对账成功";
                        }
                        else
                        {
                            sevenSection.ReconciliantStatus = "对账失败";
                        }
                    }
                    else
                    {
                        number = db.Ado.SqlQuery<decimal>(@"select (SUM(TurnOut)-SUM(TurnIn)) as number from Business_BankFlowTemplate where BankAccount ='" + bankAccount + @"'  
                                 and TransactionDate<='" + balanceDateEnd + "' ").FirstOrDefault();
                        sysBalance = initialBalance.TryToDecimal() + number;
                        if (bankBalance == sysBalance)
                        {
                            sevenSection.BalanceDate = (sevenSection.BalanceDate.Value.ToString("yyyy-MM-dd") + " " + DateTime.Now.ToString("HH:mm:ss")).TryToDate();
                            sevenSection.ReconciliantStatus = "对账成功";
                        }
                        else
                        {
                            sevenSection.ReconciliantStatus = "对账失败"; 
                        }
                    }
                    if (isEdit)
                    {
                        sevenSection.SysBalance = sysBalance;
                        db.Updateable(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.SysBalance = sysBalance;
                        sevenSection.ReconciliantDate = DateTime.Now;
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
        public List<Business_SevenSection> GetCompanyCode()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public JsonResult GetCompanyCodes()
        {
            var jsonResult = new List<Business_UserCompanySet>();
            DbBusinessDataService.Command(db =>
            {
                var jsonResults = db.Queryable<Business_UserCompanySet>().Where(x => x.Block == "1" && x.IsCheck == true 
                                         && x.UserVGUID == UserInfo.Vguid.TryToString() && x.Code == UserInfo.AccountModeCode).OrderBy("CompanyCode asc").ToList();
                var sevenData = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == UserInfo.AccountModeCode).ToList();
                foreach (var item in jsonResults)
                {
                    var unable = sevenData.Any(x => x.Code == item.CompanyCode && x.Status == "0");
                    if (!unable)
                    {
                        jsonResult.Add(item);
                    }
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        
    }
}