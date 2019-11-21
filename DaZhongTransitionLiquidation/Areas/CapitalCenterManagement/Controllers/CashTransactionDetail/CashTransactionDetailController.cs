using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashTransactionDetail
{
    public class CashTransactionDetailController : BaseController
    {
        public CashTransactionDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CashTransactionDetail
        public ActionResult Index()
        {
            ViewBag.GetAccountMode = GetAccountModes();
            ViewBag.GetUseBalance = GetUseBalance();
            return View();
        }

        public decimal? GetUseBalance()
        {
            decimal? result = 0;
            DbBusinessDataService.Command(db =>
            {
                //现金交易流水
                var data = db.Queryable<Business_CashTransaction>().OrderBy("Batch asc").ToList();
                //备用金提现后用支票号匹配银行流水
                var cashData = db.SqlQueryable<Business_CashManagerInfo>(@"select * from Business_CashManagerInfo where CheckNo in (select VoucherSubject from Business_BankFlowTemplate 
                                    where TradingBank='交通银行' and ReceivingUnit='现金')")
                                    .OrderBy("No asc").ToList();
                if (data.Count > 0)
                {
                    var userBalance = data.First().UseBalance;//第一笔流水可用余额
                    decimal? money = 0;
                    if (cashData.Count > 0)
                    {
                        money = cashData.Sum(x => x.Money) - cashData.First().Money;//备用金提现总金额 - 第一笔备用金提现
                    }
                    var turnOut = data.Sum(x => x.TurnOut);//现金流水支出总金额
                    result = userBalance + money - turnOut;
                }
                else
                {
                    //可用余额初始值 = 期初余额 + 备用金提现
                }
            });
            return result;
        }

        public List<Business_SevenSection> GetAccountModes()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public JsonResult GetCashTransactionInfo(Guid vguid)
        {
            Business_CashTransaction orderList = new Business_CashTransaction();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_CashTransaction>().Single(x => x.VGUID == vguid);
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult SaveCashTransactionDetail(Business_CashTransaction sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var isAny = db.Queryable<Business_CashTransaction>().Any(x => x.VGUID == sevenSection.VGUID);
                    if (!isAny)
                    {
                        var cash = "CashTran" + UserInfo.AccountModeCode + UserInfo.CompanyCode;
                        //2019110001
                        var no = CreateNo.GetCreateCashNo(db, cash);
                        sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.Batch = GetVoucherName(no);
                        sevenSection.CreateTime = DateTime.Now;
                        sevenSection.CreatePerson = UserInfo.LoginName;
                        sevenSection.Status = "1";
                        db.Insertable(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        db.Updateable(sevenSection).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        private string GetVoucherName(string voucherNo)
        {
            var batchNo = 0;
            if (voucherNo.IsValuable() && voucherNo.Length > 4)
            {
                batchNo = voucherNo.Substring(voucherNo.Length - 4, 4).TryToInt();
            }
            return DateTime.Now.ToString("yyyyMMdd") + (batchNo + 1).TryToString().PadLeft(4, '0');
        }
    }
}