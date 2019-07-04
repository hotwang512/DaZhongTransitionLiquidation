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
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;

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
        public JsonResult GetSubjectBalance(string companyCode, string accountModeCode, string accountModeName, string month, string check, GridParams para)
        {
            var jsonResult = new JsonResultModel<v_Business_SubjectSettingInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                if (check == "T")
                {
                    var SubjectBalanceList = CheckSubjectBalanceList(db, companyCode, accountModeCode, accountModeName, month);
                    jsonResult.Rows = SubjectBalanceList;
                    jsonResult.TotalRows = pageCount;
                    return;
                }
                //查询期初7个段组合编码,期初余额
                var SubjectBalance = db.Ado.SqlQuery<v_Business_SubjectSettingInfo>("exec usp_SubjectSettingInfo @AccountModeCode,@CompanyCode", new { AccountModeCode = accountModeCode, CompanyCode = companyCode }).ToList();
                //查询账期下的借贷额
                var Assets = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select (SEGMENT1+'.'+SEGMENT2+'.'+SEGMENT3+'.'+SEGMENT4+'.'+SEGMENT5+'.'+SEGMENT6+'.'+SEGMENT7) as SubjectCount ,Sum(cast(CASE ENTERED_DR WHEN '' THEN '0' else ENTERED_DR end as decimal(20,2))) as ENTERED_DR
,Sum(cast(CASE ENTERED_CR WHEN '' THEN '0' else ENTERED_CR end as decimal(20,2))) as ENTERED_CR from AssetsGeneralLedger_Swap where LEDGER_NAME=@AccountModeName and SEGMENT1=@CompanyCode and MONTH(ACCOUNTING_DATE)=@Month
Group By (SEGMENT1+'.'+SEGMENT2+'.'+SEGMENT3+'.'+SEGMENT4+'.'+SEGMENT5+'.'+SEGMENT6+'.'+SEGMENT7)", new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month }).ToList();
                foreach (var item in SubjectBalance)
                {
                    item.Company = item.Company + "." + item.Accounting + "." + item.CostCenter + "." + item.SpareOne + "." + item.SpareTwo + "." + item.Intercourse;
                    var drcr = Assets.Where(x => x.SubjectCount == item.BusinessCode).FirstOrDefault();
                    if (drcr != null)
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
        public List<v_Business_SubjectSettingInfo> CheckSubjectBalanceList(SqlSugarClient db, string companyCode, string accountModeCode, string accountModeName, string month)
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
                var data = new List<AssetsGeneralLedger_Swap>();
                var AssetsData = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select (SEGMENT1+'.'+SEGMENT2+'.'+SEGMENT3+'.'+SEGMENT4+'.'+SEGMENT5+'.'+SEGMENT6+'.'+SEGMENT7) as SubjectCount,LEDGER_NAME,JE_BATCH_NAME,
JE_HEADER_NAME,ACCOUNTING_DATE,cast(CASE ENTERED_DR WHEN '' THEN '0' else ENTERED_DR end as decimal(20,2)) as ENTERED_DR,cast(CASE ENTERED_CR WHEN '' THEN '0' else ENTERED_CR end as decimal(20,2)) as ENTERED_CR,STATUS,MESSAGE from AssetsGeneralLedger_Swap where MONTH(ACCOUNTING_DATE)=@Month and LEDGER_NAME=@AccountModeName 
and SEGMENT1=@CompanyCode ", new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month }).ToList();
                AssetsData = AssetsData.Where(x => x.SubjectCount == businessCode).ToList();
                var AssetsDetailData = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select COMBINATION as SubjectCount,LEDGER_NAME,JE_BATCH_NAME,JE_HEADER_NAME,JE_CATEGORY_NAME,ACCOUNTING_DATE,ENTERED_DR,ENTERED_CR
from AssetsGeneralLedgerDetail_Swap where LEDGER_NAME=@AccountModeName and substring(COMBINATION,0,3)=@CompanyCode and MONTH(ACCOUNTING_DATE)=@Month and COMBINATION=@SubjectCount",
                new { AccountModeName = accountModeName, CompanyCode = companyCode, Month = month, SubjectCount = businessCode }).ToList();
                foreach (var item in AssetsDetailData)
                {
                    item.ENTERED_CR = item.ENTERED_CR == null ? "0.00" : item.ENTERED_CR;
                    item.ENTERED_DR = item.ENTERED_DR == null ? "0.00" : item.ENTERED_DR;
                    var isAny = AssetsData.Any(x => x.SubjectCount == item.SubjectCount && x.LEDGER_NAME == item.LEDGER_NAME && x.JE_HEADER_NAME == item.JE_HEADER_NAME && x.ENTERED_DR == item.ENTERED_DR && x.ENTERED_CR == item.ENTERED_CR);
                    if (!isAny)
                    {
                        data.Add(item);
                    }
                }
                jsonResult.Rows = data;
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SyncAssetsData(string jsonData)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var voucherList = new List<Business_VoucherList>();
                var voucherDetail = new List<Business_VoucherDetail>();
                var sevenData1 = db.Queryable<Business_SevenSection>().Where(x=>x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                var sevenData2 = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43").ToList();
                var sevenData3 = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ToList();
                var voucherData = db.Queryable<Business_VoucherList>().Where(x => x.Automatic == "3").ToList();
                var assetsData = db.Queryable<AssetsGeneralLedgerDetail_Swap>().ToList();
                var tableData = jsonData.JsonToModel<List<AssetsGeneralLedger_Swap>>();
                foreach (var item in tableData)
                {
                    var isAny = voucherData.Any(x => x.VoucherNo == item.JE_HEADER_NAME.Split(" ")[0]);
                    if (isAny)
                    {
                        continue;
                    }
                    var account = sevenData2.SingleOrDefault(x => x.Descrption == item.LEDGER_NAME).Code;
                    var company = sevenData1.SingleOrDefault(x => x.AccountModeCode == account && x.Code == item.SubjectCount.Split(".")[0]).Descrption;
                    var credit = item.ENTERED_CR == "0"? item.ENTERED_DR: item.ENTERED_CR;
                    var debit = item.ENTERED_DR == "0" ? item.ENTERED_CR : item.ENTERED_DR;
                    Business_VoucherList voucher = new Business_VoucherList();
                    voucher.AccountingPeriod = item.ACCOUNTING_DATE;
                    voucher.AccountModeName = item.LEDGER_NAME;
                    voucher.Auditor = "";
                    voucher.Bookkeeping = "";
                    voucher.Cashier = "";
                    voucher.CompanyCode = item.SubjectCount.Split(".")[0];
                    voucher.CompanyName = company;
                    voucher.Currency = "";
                    voucher.DocumentMaker = "";
                    voucher.FinanceDirector = "";
                    voucher.Status = "3";
                    voucher.VoucherDate = item.ACCOUNTING_DATE;
                    voucher.VoucherType = item.JE_CATEGORY_NAME.Split(".")[1] + "类";
                    voucher.CreditAmountTotal = credit.TryToDecimal();
                    voucher.DebitAmountTotal = debit.TryToDecimal();
                    voucher.CreateTime = DateTime.Now;
                    var guid = Guid.NewGuid();
                    voucher.BatchName = item.JE_BATCH_NAME.Split(" ")[0];
                    voucher.VoucherNo = item.JE_HEADER_NAME.Split(" ")[0];
                    voucher.VGUID = guid;
                    voucher.Automatic = "3";//Oracle同步
                    voucherList.Add(voucher);
                    //凭证明细表
                    var assetsDataList = assetsData.Where(x => x.LEDGER_NAME == item.LEDGER_NAME && x.JE_HEADER_NAME == item.JE_HEADER_NAME && x.ACCOUNTING_DATE == item.ACCOUNTING_DATE).ToList();
                    if (assetsDataList.Count > 0)
                    {
                        foreach (var ass in assetsDataList)
                        {
                            var subject = sevenData3.Where(x => x.Code == ass.COMBINATION.Split(".")[1]).FirstOrDefault().Descrption;
                            Business_VoucherDetail BVDetail = new Business_VoucherDetail();
                            BVDetail.Abstract = "Oracle同步数据";
                            BVDetail.CompanySection = ass.COMBINATION.Split(".")[0];
                            BVDetail.SubjectSection = ass.COMBINATION.Split(".")[1];
                            BVDetail.SubjectSectionName = subject;
                            BVDetail.AccountSection = ass.COMBINATION.Split(".")[2];
                            BVDetail.CostCenterSection = ass.COMBINATION.Split(".")[3];
                            BVDetail.SpareOneSection = ass.COMBINATION.Split(".")[4];
                            BVDetail.SpareTwoSection = ass.COMBINATION.Split(".")[5];
                            BVDetail.IntercourseSection = ass.COMBINATION.Split(".")[6];           
                            BVDetail.SevenSubjectName = ass.COMBINATION + ass.COMBINATION_DESCRIPTION;
                            BVDetail.BorrowMoney = ass.ENTERED_DR;
                            BVDetail.LoanMoney = ass.ENTERED_CR;
                            BVDetail.BorrowMoneyCount = ass.ENTERED_DR;
                            BVDetail.LoanMoneyCount = ass.ENTERED_CR;
                            BVDetail.JE_LINE_NUMBER = 0;
                            BVDetail.VGUID = Guid.NewGuid();
                            BVDetail.VoucherVGUID = guid;
                            voucherDetail.Add(BVDetail);
                        }
                    }
                }
                if(voucherList.Count > 0 && voucherDetail.Count > 0)
                {
                    db.Insertable(voucherList).ExecuteCommand();
                    db.Insertable(voucherDetail).ExecuteCommand();
                    resultModel.IsSuccess = true;
                }else
                {
                    resultModel.Status = "1";
                }
            });
            return Json(resultModel);
        }
    }
}