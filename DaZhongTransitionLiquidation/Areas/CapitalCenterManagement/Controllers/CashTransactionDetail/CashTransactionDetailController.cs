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
            ViewBag.GetGuid = Guid.NewGuid().TryToString();
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
                cashData = cashData.Where(x => x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == UserInfo.CompanyCode && x.Status == "3").ToList();
                if (data.Count > 0)
                {
                    var userBalance = data.First().UseBalance;//第一笔流水可用余额
                    decimal? money = 0;
                    if (cashData.Count > 0)
                    {
                        money = cashData.Sum(x => x.Money) - cashData.First().Money;//备用金提现总金额 - 第一笔备用金提现
                        //money = cashData.Sum(x => x.Money);
                    }
                    var turnOut = data.Sum(x => x.TurnOut).TryToDecimal();//现金流水支出总金额
                    result = userBalance + money - turnOut;
                }
                else
                {
                    //可用余额初始值 = 期初余额 + 备用金提现
                    var firstMoney = 0;
                    result = firstMoney + cashData.Sum(x => x.Money);
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
                        //2019110001--现金报销
                        var no = CreateNo.GetCreateCashNo(db, cash);
                        //sevenSection.VGUID = sevenSection.VGUID;
                        sevenSection.Batch = "XJBX" + no;
                        sevenSection.CreateTime = DateTime.Now;
                        sevenSection.CreatePerson = UserInfo.LoginName;
                        sevenSection.Status = "1";
                        db.Insertable(sevenSection).IgnoreColumns(it => it == "CheckTime").ExecuteCommand();
                    }
                    else
                    {
                        db.Updateable(sevenSection).IgnoreColumns(it => it == "CheckTime").ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult GetCashBorrowLoan(Guid PayVGUID, int sort, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_CashBorrowLoan>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                if (sort == 1)
                {
                    jsonResult.Rows = db.Queryable<Business_CashBorrowLoan>().Where(i => i.PayVGUID == PayVGUID).OrderBy(i => i.VCRTTIME, OrderByType.Asc)
                        .OrderBy(i => i.Sort, OrderByType.Asc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                }
                else
                {
                    jsonResult.Rows = db.Queryable<Business_CashBorrowLoan>().Where(i => i.PayVGUID == PayVGUID)
                        .OrderBy(i => i.VCRTTIME, OrderByType.Asc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                }

                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteCashBorrowLoan(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = db.Deleteable<Business_CashBorrowLoan>(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveCashBorrowLoan(Business_CashBorrowLoan bankChannel, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (!isEdit)
            {
                bankChannel.VCRTUSER = UserInfo.LoginName;
                bankChannel.VCRTTIME = DateTime.Now;
                bankChannel.VGUID = Guid.NewGuid();
                //新增时重新排序

            }
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (isEdit)
                    {
                        db.Updateable<Business_CashBorrowLoan>().UpdateColumns(it => new Business_CashBorrowLoan()
                        {
                            Borrow = bankChannel.Borrow,
                            Loan = bankChannel.Loan,
                            PayVGUID = bankChannel.PayVGUID,
                            Remark = bankChannel.Remark,
                            Money = bankChannel.Money,
                            Sort = bankChannel.Sort,
                            VMDFTIME = DateTime.Now,
                            VMDFUSER = UserInfo.LoginName,
                        }).Where(it => it.VGUID == bankChannel.VGUID).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(bankChannel).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}