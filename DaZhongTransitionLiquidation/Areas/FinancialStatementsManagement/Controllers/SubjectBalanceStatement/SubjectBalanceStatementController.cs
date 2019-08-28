using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqlSugar;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherList;

namespace DaZhongTransitionLiquidation.Areas.FinancialStatementsManagement.Controllers
{
    public class SubjectBalanceStatementController : BaseController
    {
        public SubjectBalanceStatementController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: FinancialStatementsManagement/SubjectBalanceStatement
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            ViewBag.CompanyCode = GetCompanyCode();
            ViewBag.AccountMode = GetAccountMode();
            return View();
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
        public JsonResult GetSubjectBalance(string companyCode, string accountModeCode, string accountModeName, string month, string year, string check, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_Business_SubjectSettingInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                if (check == "T")
                {
                    var SubjectBalanceList = CheckSubjectBalanceList(db, companyCode, accountModeCode, accountModeName, month, year);
                    jsonResult.Rows = SubjectBalanceList;
                    jsonResult.TotalRows = pageCount;
                    return;
                }
                //查询期初7个段组合编码,期初余额
                var SubjectBalance = db.Ado.SqlQuery<v_Business_SubjectSettingInfo>("exec usp_SubjectSettingInfo @AccountModeCode,@CompanyCode,@Year,@Month",
                    new { AccountModeCode = accountModeCode, CompanyCode = companyCode, Year = year, Month = month }).ToList();
                //查询账期下的借贷额
                //                var Assets = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select (SEGMENT1+'.'+SEGMENT2+'.'+SEGMENT3+'.'+SEGMENT4+'.'+SEGMENT5+'.'+SEGMENT6+'.'+SEGMENT7) as SubjectCount ,Sum(cast(CASE ENTERED_DR WHEN '' THEN '0' else ENTERED_DR end as decimal(20,2))) as ENTERED_DR
                //,Sum(cast(CASE ENTERED_CR WHEN '' THEN '0' else ENTERED_CR end as decimal(20,2))) as ENTERED_CR from AssetsGeneralLedger_Swap where LEDGER_NAME=@AccountModeName and SEGMENT1=@CompanyCode and Year(ACCOUNTING_DATE)=@Year and MONTH(ACCOUNTING_DATE)=@Month
                //Group By (SEGMENT1+'.'+SEGMENT2+'.'+SEGMENT3+'.'+SEGMENT4+'.'+SEGMENT5+'.'+SEGMENT6+'.'+SEGMENT7)", new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month,Year = year }).ToList();
                var Assets = db.Ado.SqlQuery<v_AssetsGeneralLedger_Swap>(@"select (b.CompanySection+'.'+b.SubjectSection+'.'+b.AccountSection+'.'+b.CostCenterSection+'.'+b.SpareOneSection+'.'+b.SpareTwoSection+'.'+b.IntercourseSection) as SubjectCount,a.AccountModeName as LEDGER_NAME,a.BatchName as JE_BATCH_NAME,
a.VoucherNo as JE_HEADER_NAME,a.VoucherDate as ACCOUNTING_DATE,b.BorrowMoney as TurnOut,b.LoanMoney as TurnIn from Business_VoucherList as a left join Business_VoucherDetail as b on a.VGUID = b.VoucherVGUID
 where a.Status='3' and a.AccountModeName=@AccountModeName and a.CompanyCode=@CompanyCode and MONTH(a.VoucherDate)=@Month and Year(a.VoucherDate)=@Year",
 new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month, Year = year }).ToList();
                foreach (var item in SubjectBalance)
                {
                    item.Company = item.Company + "." + item.Accounting + "." + item.CostCenter + "." + item.SpareOne + "." + item.SpareTwo + "." + item.Intercourse;
                    var drcr = Assets.Where(x => x.SubjectCount == item.BusinessCode).FirstOrDefault();
                    if (drcr != null)
                    {
                        try
                        {
                            item.Balance = item.Balance == null ? 0 : item.Balance;//期初余额
                            item.ENTERED_DR = Convert.ToDecimal(drcr.TurnOut);//本期借方
                            item.ENTERED_CR = Convert.ToDecimal(drcr.TurnIn);//本期贷方
                            item.END_BALANCE = item.Balance + item.ENTERED_DR - item.ENTERED_CR;//期末余额
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
                string uniqueKey = PubGet.GetUserKey + "SubjectBalance";
                CacheManager<List<v_Business_SubjectSettingInfo>>.GetInstance().Add(uniqueKey, SubjectBalance, 8 * 60 * 60);
                jsonResult.Rows = SubjectBalance;
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public List<v_Business_SubjectSettingInfo> CheckSubjectBalanceList(SqlSugarClient db, string companyCode, string accountModeCode, string accountModeName, string month, string year)
        {
            //从缓存中获取科目余额
            var SubjectBalance = CacheManager<List<v_Business_SubjectSettingInfo>>.GetInstance()[PubGet.GetUserKey + "SubjectBalance"];
            //var checkSubjectBalance = SubjectBalance.Where(x => x.Balance != null || x.ENTERED_DR != null || x.ENTERED_CR != null).ToList();
            //从科目余额中间表获取数据
            var General = db.Ado.SqlQuery<GeneralLedgerBalance_Swap>(@"select COMBINATION,Sum(BEGIN_BALANCE) as BEGIN_BALANCE,Sum(cast(PTD_DR as decimal(20,2))) as PTD_DR
,Sum(cast(PTD_CR as decimal(20,2))) as PTD_CR,Sum(END_BALANCE) as END_BALANCE from GeneralLedgerBalance_Swap where LEDGER_NAME=@AccountModeName and substring(COMBINATION,0,3)=@CompanyCode and substring(PERIOD,7,7)=@Month
Group By COMBINATION", new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month }).ToList();
            foreach (var item in SubjectBalance)
            {
                var isAny = General.Any(x => x.COMBINATION == item.BusinessCode);
                if (isAny)
                {
                    var isCheck = General.Any(x => x.BEGIN_BALANCE == item.Balance && x.PTD_DR == item.ENTERED_DR && x.PTD_CR == item.ENTERED_CR && x.END_BALANCE == x.END_BALANCE);
                    if (isCheck)
                    {
                        item.Checked = "Y";
                    }
                    else
                    {
                        item.Checked = "N";
                    }
                }
                else
                {
                    item.Checked = "";
                }
            }
            return SubjectBalance;
        }
        public JsonResult CheckSubjectBalance(string businessCode, string companyCode, string accountModeCode, string accountModeName, string month,string year, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_AssetsGeneralLedger_Swap>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                var data = new List<v_AssetsGeneralLedger_Swap>();
                //从已审核凭证中提取数据
                var AssetsData = db.Ado.SqlQuery<v_AssetsGeneralLedger_Swap>(@"select (b.CompanySection+'.'+b.SubjectSection+'.'+b.AccountSection+'.'+b.CostCenterSection+'.'+b.SpareOneSection+'.'+b.SpareTwoSection+'.'+b.IntercourseSection) as SubjectCount,a.AccountModeName as LEDGER_NAME,a.BatchName as JE_BATCH_NAME,
a.VoucherNo as JE_HEADER_NAME,a.VoucherDate as ACCOUNTING_DATE,b.BorrowMoney as TurnOut,b.LoanMoney as TurnIn from Business_VoucherList as a left join Business_VoucherDetail as b on a.VGUID = b.VoucherVGUID
 where a.Status='3' and a.AccountModeName=@AccountModeName and a.CompanyCode=@CompanyCode and MONTH(a.VoucherDate)=@Month and Year(a.VoucherDate)=@Year", 
 new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month, Year = year }).ToList();
                AssetsData = AssetsData.Where(x => x.SubjectCount == businessCode).ToList();
                data = AssetsData;
                //从总账明细中提取数据
                var AssetsDetailData = db.Ado.SqlQuery<v_AssetsGeneralLedger_Swap>(@"select COMBINATION as SubjectCount,LEDGER_NAME,JE_BATCH_NAME,JE_HEADER_NAME,JE_CATEGORY_NAME,ACCOUNTING_DATE,ENTERED_DR,ENTERED_CR
from AssetsGeneralLedgerDetail_Swap where LEDGER_NAME=@AccountModeName and substring(COMBINATION,0,3)=@CompanyCode and Year(ACCOUNTING_DATE)=@Year and MONTH(ACCOUNTING_DATE)=@Month and COMBINATION=@SubjectCount",
                new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month, SubjectCount = businessCode, Year = year }).ToList();
                foreach (var item in AssetsDetailData)
                {
                    var ENTERED_CR = item.ENTERED_CR;
                    var ENTERED_DR = item.ENTERED_DR;
                    //var isA = item.JE_HEADER_NAME.Contains("201906210225");
                    var headerName = item.JE_HEADER_NAME.Split(" ")[0];
                    var isAny = AssetsData.Any(x => x.SubjectCount == item.SubjectCount && x.LEDGER_NAME == item.LEDGER_NAME && item.JE_HEADER_NAME.Contains(x.JE_HEADER_NAME) && x.TurnOut == ENTERED_DR && x.TurnIn == ENTERED_CR);
                    if (!isAny)
                    {
                        data.Add(item);
                    }
                    else
                    {
                        var removeData = AssetsData.Where(x => x.JE_HEADER_NAME == headerName && x.TurnOut == ENTERED_DR && x.TurnIn == ENTERED_CR).ToList();
                        foreach (var it in removeData)
                        {
                            data.Remove(it);
                        }
                    }
                }
                jsonResult.Rows = data;
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckVoucher(string companyCode, string accountModeCode, string accountModeName, string month,string year)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var isAny = db.Queryable<Business_VoucherList>().Any(x => x.AccountModeName == accountModeName && x.CompanyCode == companyCode
                                && x.VoucherDate.Value.Year == year.TryToInt() && x.VoucherDate.Value.Month == month.TryToInt() && x.Status == "2");
                if (isAny)
                {
                    resultModel.Status = "1";
                }
            });
            return Json(resultModel);
        }
    }
}