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
        public JsonResult GetSubjectBalance(string companyCode, string accountModeCode,string accountModeName,string month,string check, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_Business_SubjectSettingInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                if (check == "T")
                {
                    var SubjectBalanceList = CheckSubjectBalanceList(db,companyCode, accountModeCode, accountModeName, month);
                    jsonResult.Rows = SubjectBalanceList;
                    jsonResult.TotalRows = pageCount;
                    return;
                }
                //查询期初7个段组合编码,期初余额
                var SubjectBalance = db.Ado.SqlQuery<v_Business_SubjectSettingInfo>("exec usp_SubjectSettingInfo @AccountModeCode,@CompanyCode", new { AccountModeCode = accountModeCode, CompanyCode = companyCode }).ToList();
                //查询账期下的借贷额
                var Assets = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select (SEGMENT1+'.'+SEGMENT2+'.'+SEGMENT3+'.'+SEGMENT4+'.'+SEGMENT5+'.'+SEGMENT6+'.'+SEGMENT7) as SubjectCount ,Sum(cast(ENTERED_DR as decimal(20,2))) as ENTERED_DR
,Sum(cast(ENTERED_CR as decimal(20,2))) as ENTERED_CR from AssetsGeneralLedger_Swap where LEDGER_NAME=@AccountModeName and SEGMENT1=@CompanyCode and MONTH(ACCOUNTING_DATE)=@Month
Group By (SEGMENT1+'.'+SEGMENT2+'.'+SEGMENT3+'.'+SEGMENT4+'.'+SEGMENT5+'.'+SEGMENT6+'.'+SEGMENT7)", new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month }).ToList();
                foreach (var item in SubjectBalance)
                {
                    var drcr = Assets.Where(x => x.SubjectCount == item.BusinessCode).FirstOrDefault();
                    if(drcr != null)
                    {
                        try
                        {
                            item.Balance = item.Balance == null ? 0 : item.Balance;//期初余额
                            item.ENTERED_DR = Convert.ToDecimal(drcr.ENTERED_DR);//本期借方
                            item.ENTERED_CR = Convert.ToDecimal(drcr.ENTERED_CR);//本期贷方
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
        public List<v_Business_SubjectSettingInfo> CheckSubjectBalanceList(SqlSugarClient db,string companyCode, string accountModeCode, string accountModeName, string month)
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
        public JsonResult CheckSubjectBalance(string businessCode, string companyCode, string accountModeCode, string accountModeName, string month, GridParams para)
        {
            var jsonResult = new JsonResultModel<AssetsGeneralLedger_Swap>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                var data = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select (SEGMENT1+'.'+SEGMENT2+'.'+SEGMENT3+'.'+SEGMENT4+'.'+SEGMENT5+'.'+SEGMENT6+'.'+SEGMENT7) as SubjectCount,LEDGER_NAME,JE_BATCH_NAME,
JE_HEADER_NAME,ACCOUNTING_DATE,ENTERED_DR,ENTERED_CR,STATUS,MESSAGE from AssetsGeneralLedger_Swap where MONTH(ACCOUNTING_DATE)=@Month and LEDGER_NAME=@AccountModeName 
and SEGMENT1=@CompanyCode ", new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month }).ToList();
                jsonResult.Rows = data.Where(x => x.SubjectCount == businessCode).ToList();
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}