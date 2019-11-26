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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashManager
{
    public class CashManagerController : BaseController
    {
        public CashManagerController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CashManager
        public ActionResult Index()
        {
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("aa345b36-259a-48f3-a5ec-5abc950b085d");
            return View();
        }
        public JsonResult GetCashManagerData(Business_CashManagerInfo searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_CashManagerInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_CashManagerInfo>()
                .WhereIF(searchParams.BankAccount != null, i => i.BankAccount == searchParams.BankAccount)
                .WhereIF(searchParams.ApplyDate != null, i => i.ApplyDate == searchParams.ApplyDate)
                .Where(x=>x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == UserInfo.CompanyCode)
                .Where(i => i.Status == searchParams.Status)
                .OrderBy(i => i.No, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdataCashManager(List<Guid> vguids, string status)//Guid[] vguids
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
                        if (status == "3")
                        {
                            saveChanges = db.Updateable<Business_CashManagerInfo>().UpdateColumns(it => new Business_CashManagerInfo()
                            {
                                Status = status,
                                Auditor = UserInfo.LoginName,
                                CreateTime = DateTime.Now
                            }).Where(it => it.VGUID == item).ExecuteCommand();
                            //备用金审核成功进入现金交易流水表
                            SetCashTransactionFlow(db, item);
                            //根据备用金配置借贷生成凭证
                            GenerateVoucherList(db, item, userData);
                        }
                        else
                        {
                            saveChanges = db.Updateable<Business_CashManagerInfo>().UpdateColumns(it => new Business_CashManagerInfo()
                            {
                                Status = status,
                                Changer = UserInfo.LoginName,
                                CreateTime = DateTime.Now
                            }).Where(it => it.VGUID == item).ExecuteCommand();
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
            var cashData = db.Queryable<Business_CashManagerInfo>().Where(x => x.VGUID == vguid).First();
            var cashDataDetail = db.Queryable<Business_CashBorrowLoan>().Where(x => x.PayVGUID == vguid).OrderBy("Borrow desc").ToList();
            //主信息
            Business_VoucherList voucher = new Business_VoucherList();
            voucher.CreditAmountTotal = cashDataDetail.Where(x => x.Loan != null && x.Loan != "").Sum(x => x.Money);
            voucher.DebitAmountTotal = cashDataDetail.Where(x => x.Borrow != null && x.Borrow != "").Sum(x => x.Money);
            GetVoucherList(db, voucher, cashData, guid, userData);
            //借贷信息
            List<Business_VoucherDetail> BVDetailList = new List<Business_VoucherDetail>();
            if (cashDataDetail.Count > 0)
            {
                GetVoucherDetail(db, BVDetailList, cashData, cashDataDetail, guid);
            }
            if (voucher != null && BVDetailList.Count > 0)
            {
                db.Insertable(voucher).ExecuteCommand();
                db.Insertable(BVDetailList).ExecuteCommand();
            }
        }
        private void GetVoucherDetail(SqlSugarClient db, List<Business_VoucherDetail> BVDetailList, Business_CashManagerInfo cashData, List<Business_CashBorrowLoan> cashDataDetail, Guid guid)
        {
            var i = 0;
            foreach (var item in cashDataDetail)
            {
                Business_VoucherDetail BVDetail = new Business_VoucherDetail();
                BVDetail.Abstract = item.Remark;
                BVDetail.JE_LINE_NUMBER = i++;
                BVDetail.VGUID = Guid.NewGuid();
                BVDetail.VoucherVGUID = guid;
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
                BVDetail.SevenSubjectName = seven + "\n" + GetSevenSubjectName(seven, cashData.AccountModeCode, cashData.CompanyCode);
                BVDetailList.Add(BVDetail);
            }
        }
        private void GetVoucherList(SqlSugarClient db, Business_VoucherList voucher, Business_CashManagerInfo cashData, Guid guid, List<Sys_User> userData)
        {
            Regex rgx = new Regex(@"[\w|\W]{2,4}银行");
            var rgsBankName = rgx.Match(cashData.BankName).Value;
            voucher.AccountingPeriod = cashData.ApplyDate;
            voucher.AccountModeName = cashData.AccountModeName;
            voucher.CompanyCode = cashData.CompanyCode;
            voucher.CompanyName = cashData.CompanyName.ToDBC();
            var bank = "M" + cashData.AccountModeCode + cashData.CompanyCode + cashData.ApplyDate.Value.Year.ToString() + cashData.ApplyDate.Value.Month.ToString();
            //100201银行类2019090001
            var no = CreateNo.GetCreateNo(db, bank);
            voucher.VoucherNo = cashData.AccountModeCode + cashData.CompanyCode + "现金类" + no;
            voucher.BatchName = voucher.VoucherNo;
            voucher.DocumentMaker = "";
            voucher.Status = "1";
            voucher.VoucherDate = cashData.ApplyDate;
            voucher.VoucherType = "现金类";
            voucher.Automatic = "1";//自动
            voucher.TradingBank = rgsBankName;
            voucher.ReceivingUnit = cashData.Cashier;
            voucher.TransactionDate = cashData.ApplyDate;
            voucher.Batch = cashData.No;
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
            var cashData = db.Queryable<Business_CashManagerInfo>().Where(x => x.VGUID == vguid).First();
            //var bankInfo = db.Queryable<Business_CompanyBankInfo>().Where(x => x.BankStatus == true && x.AccountModeCode == cashData.AccountModeCode && x.CompanyCode == cashData.CompanyCode).First();
            var cashFlow = new Business_CashTransactionTemplate();
            Regex rgx = new Regex(@"[\w|\W]{2,4}银行");
            var rgsBankName = rgx.Match(cashData.BankName).Value;
            cashFlow.VGUID = Guid.NewGuid();
            cashFlow.AccountModeCode = cashData.AccountModeCode;
            cashFlow.AccountModeName = cashData.AccountModeName;
            cashFlow.CompanyCode = cashData.CompanyCode;
            cashFlow.CompanyName = cashData.CompanyName;
            cashFlow.TradingBank = rgsBankName;
            cashFlow.PayeeAccount = cashData.BankAccount;
            cashFlow.PaymentUnitInstitution = cashData.BankName;
            cashFlow.Batch = cashData.No;
            cashFlow.TransactionDate = cashData.CreateTime;
            cashFlow.TurnOut = 0;//转入(贷)
            cashFlow.TurnIn = cashData.Money;//转出(借)
            cashFlow.Balance = null;
            cashFlow.ReceivingUnit = cashData.Cashier;
            cashFlow.ReceivableAccount = "";
            cashFlow.ReceivingUnitInstitution = "";
            cashFlow.Purpose = cashData.Remark;
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