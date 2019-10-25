using DaZhongTransitionLiquidation.Infrastructure.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using SyntacticSugar;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using System.Text.RegularExpressions;
using DaZhongTransitionLiquidation.Controllers;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherList
{
    public class VoucherListController : BaseController
    {
        public VoucherListController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: VoucherManageManagement/VoucherList
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("395599E8-F05B-4FAD-9347-0A17F0D4CEAA");
            return View();
        }
        public JsonResult GetVoucherListDatas(Business_VoucherList searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_VoucherList>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                var starDate = "2019-09-01".TryToDate();
                if (searchParams.Automatic != "0")
                {
                    var voucherData = db.Queryable<Business_VoucherList>().Where(i => i.VoucherDate >= starDate)
                    .Where(i => i.Status == searchParams.Status)
                    .Where(i => i.Automatic == searchParams.Automatic)
                    .Where(i => i.AccountModeName == UserInfo.AccountModeName && i.CompanyCode == UserInfo.CompanyCode)
                    .WhereIF(searchParams.VoucherType != null, i => i.VoucherType == searchParams.VoucherType)
                    .Where(x => x.TradingBank == "" || x.TradingBank == null || x.VoucherNo == null || x.VoucherNo == "").ToList();
                    foreach (var item in voucherData)
                    {
                        if (item.VoucherNo == null || item.VoucherNo == "")
                        {
                            var bank = "Bank" + UserInfo.AccountModeCode + UserInfo.CompanyCode;
                            var no = CreateNo.GetCreateNo(db, bank);
                            item.VoucherNo = UserInfo.AccountModeCode + UserInfo.CompanyCode + item.VoucherType + no;
                            db.Updateable(item).ExecuteCommand();
                        }
                        if (item.TradingBank == "" || item.TradingBank == null)
                        {
                            var bankFlow1 = db.SqlQueryable<Business_BankFlowTemplate>(@"select * from Business_BankFlowTemplate where CONVERT(varchar(100), TransactionDate, 23)='" + item.VoucherDate.TryToDate().ToString("yyyy-MM-dd") + @"'").ToList();
                            var bankFlow = bankFlow1.Where(x => x.TurnIn == item.CreditAmountTotal || x.TurnOut == item.DebitAmountTotal)
                                            .Where(x => x.AccountModeName == item.AccountModeName && x.CompanyCode == item.CompanyCode).ToList();
                            if (bankFlow.Count == 1)
                            {
                                item.TradingBank = bankFlow[0].TradingBank;
                                item.ReceivingUnit = bankFlow[0].ReceivingUnit;
                                item.TransactionDate = bankFlow[0].TransactionDate;
                                item.Batch = bankFlow[0].Batch;
                            }
                            else
                            {
                                var voucherDetail = db.Queryable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == item.VGUID).ToList();
                                foreach (var it in voucherDetail)
                                {
                                    var bankFlow2 = bankFlow1
                                    .Where(x => x.Remark == it.Abstract)
                                    .Where(x => (x.TurnIn == it.LoanMoney || x.TurnOut == it.BorrowMoney))
                                    .Where(x => x.AccountModeName == item.AccountModeName && x.CompanyCode == item.CompanyCode).ToList();
                                    if (bankFlow2.Count == 1)
                                    {
                                        item.TradingBank = bankFlow2[0].TradingBank;
                                        item.ReceivingUnit = bankFlow2[0].ReceivingUnit;
                                        item.TransactionDate = bankFlow2[0].TransactionDate;
                                        item.Batch = bankFlow2[0].Batch;
                                    }
                                    else
                                    {
                                        var bankFlow3 = bankFlow1
                                   .Where(x => (x.TurnIn == it.LoanMoney || x.TurnOut == it.BorrowMoney))
                                   .Where(x => x.AccountModeName == item.AccountModeName && x.CompanyCode == item.CompanyCode).ToList();
                                        if (bankFlow3.Count == 1)
                                        {
                                            item.TradingBank = bankFlow3[0].TradingBank;
                                            item.ReceivingUnit = bankFlow3[0].ReceivingUnit;
                                            item.TransactionDate = bankFlow3[0].TransactionDate;
                                            item.Batch = bankFlow3[0].Batch;
                                        }
                                    }
                                }
                                //var bankFlow2 = bankFlow1.
                            }
                            db.Updateable(item).ExecuteCommand();
                        }
                    }
                }
                
                
                //DateTime? firstDay = null;
                //DateTime? lastDay = null;
                //if (searchParams.AccountingPeriod != null)
                //{
                //    firstDay = searchParams.AccountingPeriod.Value.AddDays(1 - searchParams.AccountingPeriod.Value.Day);
                //    lastDay = searchParams.AccountingPeriod.Value.AddDays(1 - searchParams.AccountingPeriod.Value.Day).AddMonths(1).AddDays(-1);
                //}
                var transactionDate = (searchParams.TransactionDate.TryToDate().ToString("yyyy-MM-dd") + " 23:59:59").TryToDate();
                var tradingBank = "";
                if (searchParams.TradingBank != null)
                {
                    Regex rgx = new Regex(@"[\w|\W]{2,2}银行");
                    tradingBank = rgx.Match(searchParams.TradingBank).Value;
                }
                jsonResult.Rows = db.Queryable<Business_VoucherList>()
                .Where(i => i.Status == searchParams.Status)
                .Where(i => i.Automatic == searchParams.Automatic)
                .Where(i => i.VoucherDate >= starDate)
                .WhereIF(searchParams.VoucherType != null, i => i.VoucherType == searchParams.VoucherType)
                .WhereIF(searchParams.ReceivingUnit != null, i => i.ReceivingUnit.Contains(searchParams.ReceivingUnit))
                .WhereIF(searchParams.TradingBank != null, i => i.TradingBank == tradingBank)
                .WhereIF(searchParams.TransactionDate != null, i => i.TransactionDate >= searchParams.TransactionDate && i.TransactionDate <= transactionDate)
                .Where(i => i.AccountModeName == UserInfo.AccountModeName && i.CompanyCode == UserInfo.CompanyCode)
                .OrderBy("VoucherNo desc").ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteVoucherListInfo(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                foreach (var item in vguids)
                {
                    int saveChanges = 1;
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_VoucherList>(x => x.VGUID == item).ExecuteCommand();
                    //删除副表信息
                    db.Deleteable<Business_VoucherDetail>(x => x.VoucherVGUID == item).ExecuteCommand();
                    //删除中间表信息
                    //db.Deleteable<AssetsGeneralLedger_Swap>(x => x.SubjectVGUID == item).ExecuteCommand(); 
                    //删除附件信息
                    db.Deleteable<Business_VoucherAttachmentList>(x => x.VoucherVGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == 1;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult UpdataVoucherListInfo(List<Guid> vguids, string status, string index)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                db.Ado.UseTran(() =>
                {
                    var i = 1;
                    int saveChanges = 1;
                    foreach (var item in vguids)
                    {
                        var voucherOne = db.Queryable<Business_VoucherList>().Where(x => x.VGUID == item).First();
                        var voucher = db.Queryable<Business_VoucherDetail>().Where(it => it.VoucherVGUID == item).ToList();
                        var loanMoney = voucher == null ? null : voucher.Sum(x => x.LoanMoney);//贷方总金额
                        var borrowMoney = voucher == null ? null : voucher.Sum(x => x.BorrowMoney);//借方总金额
                        if (loanMoney == borrowMoney)
                        {
                            if (status == "3")
                            {
                                saveChanges = db.Updateable<Business_VoucherList>().UpdateColumns(it => new Business_VoucherList()
                                {
                                    Status = status,
                                    Auditor = UserInfo.LoginName
                                }).Where(it => it.VGUID == item).ExecuteCommand();
                            }
                            else
                            {
                                if (voucherOne.Status == "4")
                                {
                                    saveChanges = db.Updateable<Business_VoucherList>().UpdateColumns(it => new Business_VoucherList()
                                    {
                                        Automatic = "1",
                                        Status = "2",
                                    }).Where(it => it.VGUID == voucherOne.VGUID).ExecuteCommand();
                                }
                                else
                                {
                                    //更新主表信息
                                    saveChanges = db.Updateable<Business_VoucherList>().UpdateColumns(it => new Business_VoucherList()
                                    {
                                        Status = status,
                                    }).Where(it => it.VGUID == item).ExecuteCommand();
                                }
                            }
                            //审核成功写入中间表
                            if (status == "3")
                            {
                                if (index != "2")
                                {
                                    var result = new List<Sys_User>();
                                    DbService.Command(_db =>
                                    {
                                        result = _db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role,a.Email from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
                                    });
                                    InsertAssetsGeneralLedger(item, db, result);
                                }
                            }
                        }
                        else
                        {
                            var j = i++;
                            resultModel.Status = "2";
                            resultModel.ResultInfo = j.ToString();
                            continue;
                        }
                    }
                    if (resultModel.Status != "2")
                    {
                        resultModel.IsSuccess = saveChanges == 1;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                    }
                });
            });
            return Json(resultModel);
        }
        private void InsertAssetsGeneralLedger(Guid item, SqlSugarClient db, List<Sys_User> result)
        {
            //删除现有中间表数据
            //db.Deleteable<AssetsGeneralLedger_Swap>().Where(x => x.LINE_ID == item.TryToString()).ExecuteCommand();
            //凭证中间表
            var accountModeCode = "";
            var assetsData = db.Queryable<AssetsGeneralLedger_Swap>().ToList();
            var voucher = db.Queryable<Business_VoucherList>().Where(x => x.VGUID == item).First();
            var voucherDetail = db.Queryable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == item).ToList();
            var accountModeData = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Descrption == voucher.AccountModeName).First();
            if(accountModeData != null)
            {
                accountModeCode = accountModeData.Code;
            }
            var type = "";            
            var documentMakerData = result.Where(x => x.LoginName == voucher.DocumentMaker).FirstOrDefault();//Oracle用户名
            var documentMaker = "";
            if (documentMakerData != null)
            {
                documentMaker = documentMakerData.Email;
            }
            //asset.VGUID = Guid.NewGuid();
            foreach (var items in voucherDetail)
            {
                AssetsGeneralLedger_Swap asset = new AssetsGeneralLedger_Swap();
                asset.CREATE_DATE = DateTime.Now;
                asset.CREATE_USER = documentMaker;
                //asset.SubjectVGUID = guid;
                asset.LINE_ID = item.TryToString();
                asset.LEDGER_NAME = voucher.AccountModeName;
                asset.JE_BATCH_NAME = voucher.BatchName;
                asset.JE_BATCH_DESCRIPTION = "";
                asset.JE_HEADER_NAME = voucher.VoucherNo;
                asset.JE_HEADER_DESCRIPTION = "";
                asset.JE_SOURCE_NAME = "大众出租财务共享平台";
                asset.JE_CATEGORY_NAME = type;//(x.现金、y.银行、z.转账)
                asset.ACCOUNTING_DATE = voucher.VoucherDate;
                asset.CURRENCY_CODE = "RMB";//币种
                asset.CURRENCY_CONVERSION_TYPE = "";//币种是RMB时为空
                asset.CURRENCY_CONVERSION_DATE = null;
                asset.CURRENCY_CONVERSION_RATE = null;//币种是RMB时为空
                asset.STATUS = "N";
                asset.TRASACTION_ID = Guid.NewGuid().TryToString();
                asset.JE_LINE_NUMBER = items.JE_LINE_NUMBER;
                asset.JE_LINE_DESCRIPTION = items.Abstract;
                asset.SEGMENT1 = items.CompanySection;
                asset.SEGMENT2 = items.SubjectSection;
                asset.SEGMENT3 = items.AccountSection;
                asset.SEGMENT4 = items.CostCenterSection;
                asset.SEGMENT5 = items.SpareOneSection;
                asset.SEGMENT6 = items.SpareTwoSection;
                asset.SEGMENT7 = items.IntercourseSection;
                asset.ENTERED_CR = items.LoanMoney.TryToString();
                asset.ENTERED_DR = items.BorrowMoney.TryToString();
                asset.ACCOUNTED_DR = items.BorrowMoney.TryToString();
                asset.ACCOUNTED_CR = items.LoanMoney.TryToString();
                //asset.SubjectCount = items.CompanySection + "." + items.SubjectSection + "." + items.AccountSection + "." + items.CostCenterSection + "." + items.SpareOneSection + "." + items.SpareTwoSection + "." + items.IntercourseSection;
                //同步至中间表
                db.Insertable(asset).ExecuteCommand();
            }
        }
        public JsonResult SyncAssetsData()
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var data = new List<AssetsGeneralLedger_Swap>();
                var voucherData = db.Queryable<Business_VoucherList>().Where(x => x.Automatic == "3" && x.Status == "2").OrderBy("VoucherDate desc").ToList();
                DateTime? voucherDate = null;
                if (voucherData.Count > 0)
                {
                    voucherDate = voucherData[0].VoucherDate;
                }
                //从已审核凭证中提取数据
                var AssetsData = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select (b.CompanySection+'.'+b.SubjectSection+'.'+b.AccountSection+'.'+b.CostCenterSection+'.'+b.SpareOneSection+'.'+b.SpareTwoSection+'.'+b.IntercourseSection) as SubjectCount,a.AccountModeName as LEDGER_NAME,a.BatchName as JE_BATCH_NAME,
a.VoucherNo as JE_HEADER_NAME,a.VoucherDate as ACCOUNTING_DATE,b.BorrowMoney as ENTERED_DR,b.LoanMoney as ENTERED_CR from Business_VoucherList as a left join Business_VoucherDetail as b on a.VGUID = b.VoucherVGUID
 where a.Status='3' ").ToList();
                //从总账明细中提取数据
                var AssetsDetailData = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select COMBINATION as SubjectCount,LEDGER_NAME,JE_BATCH_NAME,JE_HEADER_NAME,JE_CATEGORY_NAME,ACCOUNTING_DATE,ENTERED_DR,ENTERED_CR
from AssetsGeneralLedgerDetail_Swap where ACCOUNTING_DATE > @VoucherData", new { VoucherData = voucherDate }).ToList();
                foreach (var item in AssetsDetailData)
                {
                    //item.ENTERED_CR = item.ENTERED_CR == null ? "0.00" : item.ENTERED_CR;
                    //item.ENTERED_DR = item.ENTERED_DR == null ? "0.00" : item.ENTERED_DR;
                    var isAny = AssetsData.Any(x => x.LEDGER_NAME == item.LEDGER_NAME && x.SubjectCount == item.SubjectCount && x.ACCOUNTING_DATE == item.ACCOUNTING_DATE && item.JE_HEADER_NAME.Contains(x.JE_HEADER_NAME) && x.ENTERED_DR == item.ENTERED_DR && x.ENTERED_CR == item.ENTERED_CR);
                    if (!isAny)
                    {
                        data.Add(item);
                    }
                }
                if (data.Count > 0)
                {
                    #region 构造与Oracle差异的数据类
                    var voucherList = new List<Business_VoucherList>();
                    var voucherDetail = new List<Business_VoucherDetail>();
                    var sevenData1 = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                    var sevenData2 = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43").ToList();
                    var sevenData3 = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ToList();
                    var assetsData = db.Queryable<AssetsGeneralLedgerDetail_Swap>().Where(x => x.ACCOUNTING_DATE > voucherDate).ToList();
                    //var tableData = jsonData.JsonToModel<List<AssetsGeneralLedger_Swap>>();
                    foreach (var item in data)
                    {
                        try
                        {
                            //var isAny = voucherList.Any(x => x.VoucherNo == item.JE_HEADER_NAME.Split(" ")[0]);
                            //var isAny2 = voucherData.Any(x => x.VoucherNo == item.JE_HEADER_NAME.Split(" ")[0]);
                            var isAny = voucherList.Any(x => x.VoucherNo == item.JE_HEADER_NAME && x.BatchName == item.JE_BATCH_NAME && x.VoucherDate == item.ACCOUNTING_DATE);
                            var isAny2 = voucherData.Any(x => x.VoucherNo == item.JE_HEADER_NAME && x.BatchName == item.JE_BATCH_NAME && x.VoucherDate == item.ACCOUNTING_DATE);
                            if (isAny || isAny2)
                            {
                                continue;
                            }
                            var account = sevenData2.SingleOrDefault(x => x.Descrption == item.LEDGER_NAME).Code;
                            var company = sevenData1.SingleOrDefault(x => x.AccountModeCode == account && x.Code == item.SubjectCount.Split(".")[0]).Descrption;
                            var credit = item.ENTERED_CR == null ? item.ENTERED_DR : item.ENTERED_CR;
                            var debit = item.ENTERED_DR == null ? item.ENTERED_CR : item.ENTERED_DR;
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
                            voucher.Status = "2";
                            voucher.VoucherDate = item.ACCOUNTING_DATE;
                            if (item.JE_CATEGORY_NAME != "x.现金" && item.JE_CATEGORY_NAME != "y.银行" && item.JE_CATEGORY_NAME != "z.转账")
                            {
                                voucher.VoucherType = "转账类";
                            }
                            else
                            {
                                voucher.VoucherType = item.JE_CATEGORY_NAME.Split(".")[1] + "类";
                            }
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
                        catch (Exception ex)
                        {
                            var info = item;
                            throw ex;
                        }
                    }
                    #endregion
                    if (voucherList.Count > 0 && voucherDetail.Count > 0)
                    {
                        db.Insertable(voucherList).ExecuteCommand();
                        db.Insertable(voucherDetail).ExecuteCommand();
                        resultModel.IsSuccess = true;
                    }
                    else
                    {
                        resultModel.Status = "1";
                    }
                }
                else
                {
                    resultModel.Status = "2";
                }
            });
            return Json(resultModel);
        }
        public JsonResult CheckOracleData()//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var saveChanges = 0;
                var vguidList = db.SqlQueryable<AssetsGeneralLedger_Swap>(@"select distinct(LINE_ID) from AssetsGeneralLedger_Swap where STATUS = 'E' and (CheckStatus != '1' or CheckStatus is null) ").ToList();
                var voucherData = db.Queryable<Business_VoucherList>().Where(x => x.Status == "3").ToList();
                var oracleDataList = db.Queryable<AssetsGeneralLedger_Swap>().Where(x => x.STATUS == "E" && (x.CheckStatus != "1" || x.CheckStatus == null)).ToList();
                foreach (var item in vguidList)
                {
                    var oracleRemark = "";
                    var oracle = oracleDataList.Where(x => x.LINE_ID == item.LINE_ID).ToList();
                    if (oracle.Count > 0)
                    {
                        foreach (var it in oracle)
                        {
                            oracleRemark += it.MESSAGE + ",";
                        }
                        var vguid = item.LINE_ID.TryToGuid();
                        var oracleData = voucherData.Where(x => x.VGUID == vguid).FirstOrDefault();
                        oracleData.Status = "4";
                        oracleData.Automatic = "4";
                        oracleData.OracleMessage = oracleRemark;
                        db.Updateable(oracleData).ExecuteCommand();
                        db.Updateable<AssetsGeneralLedger_Swap>().UpdateColumns(it => new AssetsGeneralLedger_Swap() { CheckStatus = "1" })
                                        .Where(it => it.LINE_ID == item.LINE_ID).ExecuteCommand();
                        saveChanges = 1;
                    }
                }
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}