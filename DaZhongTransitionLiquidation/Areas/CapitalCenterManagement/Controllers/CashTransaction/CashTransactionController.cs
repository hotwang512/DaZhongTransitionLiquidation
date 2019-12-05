using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherList;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Controllers;
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
                .Where(i => i.Status == searchParams.Status)
                .OrderBy(i => i.Batch, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdataCashTransaction(List<Guid> vguids, string status)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var userData = new List<Sys_User>();
            DbService.Command(_db =>
            {
                userData = _db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
            });
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
                        if (status == "3")
                        {
                            //现金报销审核成功进入现金交易流水表
                            SetCashTransactionFlow(db, item);
                            //根据报销单配置借贷生成凭证
                            GenerateVoucherList(db, item, userData);
                        }
                    }
                    resultModel.IsSuccess = saveChanges == 1;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                });
            });
            return Json(resultModel);
        }

        private void GenerateVoucherList(SqlSugarClient db, Guid vguid, List<Sys_User> userData)
        {
            var guid = Guid.NewGuid();
            var cashData = db.Queryable<Business_CashTransaction>().Where(x => x.VGUID == vguid).First();
            var cashDataDetail = db.Queryable<Business_CashBorrowLoan>().Where(x => x.PayVGUID == vguid).OrderBy("Borrow desc").ToList();
            //主信息
            Business_VoucherList voucher = new Business_VoucherList();
            voucher.CreditAmountTotal = cashDataDetail.Where(x=>x.Loan != null && x.Loan != "").Sum(x => x.Money);
            voucher.DebitAmountTotal = cashDataDetail.Where(x => x.Borrow != null && x.Borrow != "").Sum(x => x.Money);
            GetVoucherList(db, voucher, cashData, guid, userData);
            //借贷信息
            List<Business_VoucherDetail> BVDetailList = new List<Business_VoucherDetail>();
            if (cashDataDetail.Count > 0)
            {
                GetVoucherDetail(db, BVDetailList, cashData.AccountModeCode,cashData.CompanyCode, cashDataDetail, guid);
            }
            if(voucher != null && BVDetailList.Count > 0)
            {
                db.Insertable(voucher).ExecuteCommand();
                db.Insertable(BVDetailList).ExecuteCommand();
            }
        }
        public static void GetVoucherDetail(SqlSugarClient db, List<Business_VoucherDetail> BVDetailList, string accountModeCode, string companyCode, List<Business_CashBorrowLoan> cashDataDetail,Guid guid)
        {
            var i = 0;
            foreach (var item in cashDataDetail)
            {
                Business_VoucherDetail BVDetail = new Business_VoucherDetail();
                BVDetail.Abstract = item.Remark;
                BVDetail.JE_LINE_NUMBER = i++;
                BVDetail.VGUID = Guid.NewGuid();
                BVDetail.VoucherVGUID = guid;
                BVDetail.ModelVGUID = item.VGUID;
                string seven = null;
                if (item.Borrow != "" && item.Borrow != null)
                {
                    if (item.Borrow.Contains("\n"))
                    {
                        item.Borrow = item.Borrow.Substring(0, item.Borrow.Length - 1);
                    }
                    seven = item.Borrow;
                    BVDetail.BorrowMoney = item.Money;
                    BVDetail.BorrowMoneyCount = item.Money;
                }
                else
                {
                    if (item.Loan.Contains("\n"))
                    {
                        item.Loan = item.Loan.Substring(0, item.Loan.Length - 1);
                    }
                    seven = item.Loan;
                    BVDetail.LoanMoney = item.Money;
                    BVDetail.LoanMoneyCount = item.Money;
                }
                BVDetail.CompanySection = seven.Split(".")[0];
                BVDetail.SubjectSection = seven.Split(".")[1];
                BVDetail.AccountSection = seven.Split(".")[2];
                BVDetail.CostCenterSection = seven.Split(".")[3];
                BVDetail.SpareOneSection = seven.Split(".")[4];
                BVDetail.SpareTwoSection = seven.Split(".")[5];
                BVDetail.IntercourseSection = seven.Split(".")[6];
                BVDetail.SevenSubjectName = seven + "\n" + GetSevenSubjectName(seven, accountModeCode, companyCode);
                BVDetailList.Add(BVDetail);
            }
        }
        private void GetVoucherList(SqlSugarClient db, Business_VoucherList voucher, Business_CashTransaction cashData, Guid guid, List<Sys_User> userData)
        {
            voucher.AccountingPeriod = cashData.TransactionDate;
            voucher.AccountModeName = cashData.AccountModeName;
            voucher.CompanyCode = cashData.CompanyCode;
            voucher.CompanyName = cashData.CompanyName.ToDBC();
            var bank = "M" + cashData.AccountModeCode + cashData.CompanyCode + cashData.TransactionDate.Value.Year.ToString() + cashData.TransactionDate.Value.Month.ToString();
            //100201银行类2019090001
            var no = CreateNo.GetCreateNo(db, bank);
            voucher.VoucherNo = cashData.AccountModeCode + cashData.CompanyCode + "现金类" + no;
            voucher.BatchName = voucher.VoucherNo;
            voucher.DocumentMaker = "";
            voucher.Status = "1";
            voucher.VoucherDate = cashData.TransactionDate;
            voucher.VoucherType = "现金类";
            voucher.Automatic = "1";//自动
            voucher.TradingBank = "上海银行";
            voucher.ReceivingUnit = cashData.ReimbursementMan;
            voucher.TransactionDate = cashData.TransactionDate;
            voucher.Batch = cashData.Batch;
            voucher.CreateTime = DateTime.Now;
            voucher.VGUID = guid;
            foreach (var user in userData)
            {
                switch (user.Role)
                {
                    case "财务经理": voucher.FinanceDirector = user.LoginName; break;
                    case "财务主管": voucher.Bookkeeping = user.LoginName; break;
                    //case "审核岗": voucher.Auditor = user.LoginName; break;
                    case "出纳": voucher.Cashier = user.LoginName; break;
                    default: break;
                }
            }
        }
        private void SetCashTransactionFlow(SqlSugarClient db, Guid vguid)
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
        public static string GetSevenSubjectName(string subject, string acountModeCode, string companyCode)
        {
            SqlSugarClient db = DbBusinessDataConfig.GetInstance();
            var result = "";
            var seven = subject.Split(".");
            var data = db.Queryable<Business_SevenSection>().ToList();
            var i = 0;
            var sectionVguid = "";
            var value = "";
            foreach (var item in seven)
            {
                i++;
                switch (i)
                {
                    case 1: sectionVguid = "A63BD715-C27D-4C47-AB66-550309794D43"; break;//公司
                    case 2: sectionVguid = "B63BD715-C27D-4C47-AB66-550309794D43"; break;//科目
                    case 3: sectionVguid = "C63BD715-C27D-4C47-AB66-550309794D43"; break;//核算
                    case 4: sectionVguid = "D63BD715-C27D-4C47-AB66-550309794D43"; break;//成本中心
                    case 5: sectionVguid = "E63BD715-C27D-4C47-AB66-550309794D43"; break;//备用1
                    case 6: sectionVguid = "F63BD715-C27D-4C47-AB66-550309794D43"; break;//备用2
                    case 7: sectionVguid = "G63BD715-C27D-4C47-AB66-550309794D43"; break;//往来段
                    default:
                        break;
                }
                if (i == 1)
                {
                    value = data.Single(x => x.SectionVGUID == sectionVguid && x.AccountModeCode == acountModeCode
                                     && x.Code == item).Descrption;
                }
                else
                {
                    value = data.Single(x => x.SectionVGUID == sectionVguid && x.AccountModeCode == acountModeCode
                                    && x.CompanyCode == companyCode && x.Code == item).Descrption;
                }
                result += value + ".";
            }
            result = result.Substring(0, result.Length - 1);
            return result;
        }
    }
}