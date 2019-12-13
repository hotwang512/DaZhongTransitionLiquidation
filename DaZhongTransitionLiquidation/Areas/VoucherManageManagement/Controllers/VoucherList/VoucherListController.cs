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
using System.Text.RegularExpressions;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashTransaction;
using System.Threading;

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
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("395599E8-F05B-4FAD-9347-0A17F0D4CEAA");
            return View();
        }
        public JsonResult GetVoucherListDatas(Business_VoucherList searchParams, DateTime? dateEnd, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_VoucherList>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                var starDate = "2019-09-01".TryToDate();
                DateTime? transactionDateE = DateTime.MaxValue;
                if (dateEnd != null)
                {
                    transactionDateE = (dateEnd.Value.ToString("yyyy-MM-dd") + " 23:59:59").TryToDate();
                }
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
                .WhereIF(searchParams.TransactionDate != null, i => i.TransactionDate >= searchParams.TransactionDate && i.TransactionDate <= transactionDateE)
                .Where(i => i.AccountModeName == UserInfo.AccountModeName && i.CompanyCode == UserInfo.CompanyCode)
                .OrderBy("CreateTime desc").ToPageList(para.pagenum, para.pagesize, ref pageCount);
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
                            if (status == "3")//审核
                            {
                                saveChanges = db.Updateable<Business_VoucherList>().UpdateColumns(it => new Business_VoucherList()
                                {
                                    Status = status,
                                    Auditor = UserInfo.LoginName
                                }).Where(it => it.VGUID == item).ExecuteCommand();
                            }
                            else
                            {
                                if (voucherOne.Status == "4")//退回
                                {
                                    saveChanges = db.Updateable<Business_VoucherList>().UpdateColumns(it => new Business_VoucherList()
                                    {
                                        Automatic = "1",
                                        Status = "2",
                                    }).Where(it => it.VGUID == voucherOne.VGUID).ExecuteCommand();
                                }
                                else
                                {
                                    //更新主表信息  提交
                                    saveChanges = db.Updateable<Business_VoucherList>().UpdateColumns(it => new Business_VoucherList()
                                    {
                                        Status = status,
                                        DocumentMaker = UserInfo.LoginName
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
            if (accountModeData != null)
            {
                accountModeCode = accountModeData.Code;
            }
            var type = "";
            switch (voucher.VoucherType)
            {
                case "现金类": type = "x.现金"; break;
                case "银行类": type = "y.银行"; break;
                case "转账类": type = "z.转帐"; break;
                default: break;
            }
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
                asset.JE_CATEGORY_NAME = type;//(x.现金、y.银行、z.转帐)
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
        //同步与Oracle差异的数据
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
                            if (item.JE_CATEGORY_NAME != "x.现金" && item.JE_CATEGORY_NAME != "y.银行")
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
        //校验Oracle状态为E数据
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
        //同步结算凭证
        public JsonResult CreateVoucher(string year, string month)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var userData = new List<Sys_User>();
            DbService.Command(_db =>
            {
                userData = _db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
            });
            DbBusinessDataService.Command(db =>
            {
                //查出当前登录的我方账套公司借贷配置数据
                var myData = db.Ado.SqlQuery<SettlementSubjectVoucher>(@"select a.*,b.BusinessType,c.BusinessType as BusinessTypeKey from Business_SettlementSubjectDetail as a 
                                        left join Business_SettlementSubject as b on a.SettlementVGUID=b.VGUID
                                        left join Business_SettlementSubject as c on b.ParentVGUID = c.VGUID
                                        where (a.Borrow is not null or a.Loan is not null) and  a.AccountModeCode=@AccountModeCode and a.CompanyCode=@CompanyCode order by c.Sort asc,b.Sort asc", new { AccountModeCode = UserInfo.AccountModeCode, CompanyCode = UserInfo.CompanyCode }).ToList();
                var month2 = month.TryToInt() < 10 ? "0" + month : month;
                var yearMonth = year + month2;
                var date = (year + "-" + month2).TryToDate();
                //查询出所选月份的结算金额
                var settlementCount = db.SqlQueryable<Business_SettlementCount>(@"select BusinessType,YearMonth,BELONGTO_COMPANY,SUM(Account)*(-1) as Account from Business_SettlementCount  
                                        group by BusinessType,YearMonth,BELONGTO_COMPANY").Where(x => x.YearMonth == yearMonth).ToList();
                if (myData.Count > 0 && settlementCount.Count > 0)
                {
                    if (UserInfo.AccountModeCode == "1002" && UserInfo.CompanyCode == "01")
                    {
                        //查出对方公司的数据,生成4张凭证
                        var companyInfo = db.Ado.SqlQuery<Business_SevenSection>(@" select * from Business_SevenSection where SectionVGUID='A63BD715-C27D-4C47-AB66-550309794D43' and Descrption != @CompanyName
                                        and OrgID in ('2','36','3','35','4') ", new { CompanyName = UserInfo.CompanyName }).ToList();
                        foreach (var item in companyInfo)
                        {
                            var isAnyVoucher = db.Queryable<Business_VoucherList>().Any(x => x.ReceivingUnit == item.Abbreviation && x.AccountingPeriod == date);
                            if (isAnyVoucher)
                            {
                                resultModel.IsSuccess = false;
                                resultModel.Status = "4";
                                return;
                            }
                            //查出我方借出到对方的数据
                            var myDataOther = myData.Where(x => x.AccountModeCodeOther == item.AccountModeCode && x.CompanyCodeOther == item.Code).ToList();
                            if (myDataOther.Count > 0)
                            {
                                //myDataOther[0].CompanyNameOther = myDataOther[0].CompanyNameOther + "（结算）";
                                //根据借贷配置数据生成凭证
                                GenerateVoucherList(db, myDataOther, userData, year, month, settlementCount);
                            }
                        }
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                    }
                    else
                    {
                        var isAnyVoucher = db.Queryable<Business_VoucherList>().Any(x => x.AccountModeName == UserInfo.AccountModeName && x.CompanyCode == UserInfo.CompanyCode && x.ReceivingUnit == "财务共享-大众出租" && x.AccountingPeriod == date);
                        if (isAnyVoucher)
                        {
                            resultModel.IsSuccess = false;
                            resultModel.Status = "4";
                        }
                        else
                        {
                            //根据所选账套,生成各自一张
                            var myDataOther = myData.Where(x => x.AccountModeCodeOther == "1002" && x.CompanyCodeOther == "01").ToList();
                            if (myDataOther.Count > 0)
                            {
                                //myDataOther[0].CompanyNameOther = myDataOther[0].CompanyNameOther + "（结算）";
                                //根据借贷配置数据生成凭证
                                GenerateVoucherList(db, myDataOther, userData, year, month, settlementCount);
                            }
                            resultModel.IsSuccess = true;
                            resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                        }
                    }
                }
                else
                {
                    if (myData.Count == 0)
                    {
                        resultModel.IsSuccess = false;
                        resultModel.Status = "2";
                    }
                    else if (settlementCount.Count == 0)
                    {
                        resultModel.IsSuccess = false;
                        resultModel.Status = "3";
                    }
                }
            });
            return Json(resultModel);
        }
        //获取模板VGUID
        public JsonResult GetVoucherModelList(string year, string month, GridParams para)//Guid[] vguids
        {
            var jsonResult = new JsonResultModel<VoucherModelClass>();
            DbBusinessDataService.Command(db =>
            {
                List<VoucherModelClass> vmcList = new List<VoucherModelClass>();
                month = month.TryToInt() < 10 ? "0" + month : month;
                var date = (year + "-" + month).TryToDate();
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                var data = db.Queryable<Business_VoucherModel>()
                .Where(x => x.Status == "1" || x.Status == null)//启用
                .Where(x => x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == UserInfo.CompanyCode)
                .OrderBy(i => i.ModelName, OrderByType.Asc).ToList();
                //.ToPageList(para.pagenum, para.pagesize, ref pageCount);
                var voucher = db.Queryable<Business_VoucherList>().Where(x => x.VoucherType == "转账类" && x.AccountModeName == UserInfo.AccountModeName && x.CompanyCode == UserInfo.CompanyCode
                                  && x.AccountingPeriod == date).ToList();
                foreach (var item in data)
                {
                    VoucherModelClass vmc = new VoucherModelClass();
                    vmc.VGUID = item.VGUID;
                    vmc.AccountModeCode = item.AccountModeCode;
                    vmc.CompanyCode = item.CompanyCode;
                    vmc.ModelName = item.ModelName;
                    vmc.Remark = item.Remark;
                    vmc.Status = item.Status;
                    vmc.Creater = item.Creater;
                    vmc.CreateTime = item.CreateTime;
                    var isAnyList = voucher.Where(x => x.ReceivingUnit == item.ModelName + "（模板）").ToList();
                    if (isAnyList.Count == 1)
                    {
                        if(isAnyList[0].Status == "1")
                        {
                            vmc.CreateStatus = "1";//当前月份已存在模板凭证,待审核
                        }
                        else
                        {
                            vmc.CreateStatus = "2";//当前月份已存在模板凭证,提交审核
                        }
                    }
                    else
                    {
                        vmc.CreateStatus = "0";//当前月份不存在模板凭证
                    }
                    vmcList.Add(vmc);
                }
                jsonResult.Rows = vmcList;
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        //保存当前凭证数据,并生成凭证
        public JsonResult SaveVoucherModel(string year, string month, Guid vguid)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var userData = new List<Sys_User>();
            DbService.Command(_db =>
            {
                userData = _db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
            });
            DbBusinessDataService.Command(db =>
            {
                //取出模板数据
                var months = month.TryToInt() < 10 ? "0" + month : month;
                var date = (year + "-" + months).TryToDate();
                var myData = db.Queryable<Business_VoucherModel>().Where(x => x.VGUID == vguid).ToList().FirstOrDefault();
                var cashDataLsit = db.Queryable<Business_CashBorrowLoan>().Where(x => x.PayVGUID == vguid).OrderBy(i => i.VCRTTIME, OrderByType.Asc).ToList();
                var isAnyModelList = db.Queryable<Business_VoucherList>().Where(x => x.VoucherType == "转账类" && x.AccountModeName == UserInfo.AccountModeName && x.CompanyCode == UserInfo.CompanyCode
                                   && x.ReceivingUnit == myData.ModelName + "（模板）" && x.AccountingPeriod == date).ToList();
                if (isAnyModelList.Count >= 1)
                {
                    if(isAnyModelList[0].Status == "1")
                    {
                        //在所选的会计期下模板已经生成凭证
                        var voucherVGUID = isAnyModelList[0].VGUID;
                        //var voucherDetail = db.Queryable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == voucherVGUID).ToList();
                        db.Deleteable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == voucherVGUID).ExecuteCommand();
                        List<Business_VoucherDetail> BVDetailList2 = new List<Business_VoucherDetail>();
                        CashTransactionController.GetVoucherDetail(db, BVDetailList2, UserInfo.AccountModeCode, UserInfo.CompanyCode, cashDataLsit, voucherVGUID);
                        db.Insertable(BVDetailList2).ExecuteCommand();
                    }
                    else
                    {
                        //已经存在且已提交审核,不进行操作
                    }
                }
                else
                {
                    var guid = Guid.NewGuid();
                    //主信息
                    Business_VoucherList voucher = new Business_VoucherList();
                    List<SettlementSubjectVoucher> myDataOther = new List<SettlementSubjectVoucher>();
                    SettlementSubjectVoucher myDataOne = new SettlementSubjectVoucher();
                    myDataOne.AccountModeCode = UserInfo.AccountModeCode;
                    myDataOne.CompanyCode = UserInfo.CompanyCode;
                    myDataOne.AccountModeName = UserInfo.AccountModeName;
                    myDataOne.CompanyName = UserInfo.CompanyName;
                    myDataOne.CompanyNameOther = myData.ModelName + "（模板）";
                    myDataOther.Add(myDataOne);
                    GetVoucherList(db, voucher, myDataOther, guid, userData, year, month);
                    //借贷信息
                    List<Business_VoucherDetail> BVDetailList = new List<Business_VoucherDetail>();
                    CashTransactionController.GetVoucherDetail(db, BVDetailList, UserInfo.AccountModeCode, UserInfo.CompanyCode, cashDataLsit, guid);
                    if (voucher != null && BVDetailList.Count > 0)
                    {
                        db.Insertable(voucher).ExecuteCommand();
                        db.Insertable(BVDetailList).ExecuteCommand();
                    }
                }
                resultModel.IsSuccess = true;
                resultModel.Status = "1";
            });
            return Json(resultModel);
        }

        private void DoSyncVoucherModel(object data)
        {
            VoucherModelClass vmc = (VoucherModelClass)data;
            
        }

        private void GenerateVoucherList(SqlSugarClient db, List<SettlementSubjectVoucher> myDataOther, List<Sys_User> userData, string year, string month, List<Business_SettlementCount> settlementCount)
        {
            db.Ado.UseTran(() =>
            {
                var guid = Guid.NewGuid();
                Business_VoucherList voucher = new Business_VoucherList();
                GetVoucherList(db, voucher, myDataOther, guid, userData, year, month);
                List<Business_VoucherDetail> BVDetailList = new List<Business_VoucherDetail>();
                GetVoucherDetail(db, BVDetailList, myDataOther, guid, settlementCount);
                if (voucher != null && BVDetailList.Count > 0)
                {
                    db.Insertable(voucher).ExecuteCommand();
                    db.Insertable(BVDetailList).ExecuteCommand();
                }
            });
        }
        private void GetVoucherList(SqlSugarClient db, Business_VoucherList voucher, List<SettlementSubjectVoucher> myDataOther, Guid guid, List<Sys_User> userData, string year, string month)
        {
            var months = month.TryToInt() < 10 ? "0" + month : month;
            var date = (year + "-" + months).TryToDate();
            var lastDay = LastDayOfMonth(date.TryToDate());
            voucher.AccountingPeriod = date;
            voucher.AccountModeName = myDataOther[0].AccountModeName;
            voucher.CompanyCode = myDataOther[0].CompanyCode;
            voucher.CompanyName = myDataOther[0].CompanyName.ToDBC();
            var bank = "Z" + myDataOther[0].AccountModeCode + myDataOther[0].CompanyCode + year + months;
            //100201转账类2019090001
            var no = CreateNo.GetCreateNo(db, bank);
            voucher.VoucherNo = myDataOther[0].AccountModeCode + myDataOther[0].CompanyCode + "转账类" + no;
            voucher.BatchName = voucher.VoucherNo;
            voucher.DocumentMaker = "";
            voucher.Status = "1";
            voucher.VoucherDate = lastDay;
            voucher.VoucherType = "转账类";
            voucher.Automatic = "1";//自动
            voucher.TradingBank = "";
            voucher.ReceivingUnit = myDataOther[0].CompanyNameOther;
            voucher.TransactionDate = DateTime.Now;
            voucher.Batch = "";
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
        private void GetVoucherDetail(SqlSugarClient db, List<Business_VoucherDetail> BVDetailList, List<SettlementSubjectVoucher> myDataOther, Guid guid, List<Business_SettlementCount> settlementCount)
        {
            var i = 0;
            foreach (var item in myDataOther)
            {
                Business_VoucherDetail BVDetail = new Business_VoucherDetail();
                BVDetail.Abstract = item.Remark;
                BVDetail.JE_LINE_NUMBER = i++;
                BVDetail.VGUID = Guid.NewGuid();
                BVDetail.ModelVGUID = item.VGUID;
                BVDetail.VoucherVGUID = guid;
                //var borrowMoney = settlementCount.Where(x => x.BELONGTO_COMPANY == item.CompanyName && x.BusinessType == (item.BusinessTypeKey + "-" + item.BusinessType)).FirstOrDefault().Account;
                decimal? Money = 0;
                if (item.BusinessTypeKey == null && item.BusinessType != null)
                {
                    if (item.CompanyNameOther == "财务共享-大众出租")
                    {
                        Money = settlementCount.Where(x => x.BELONGTO_COMPANY == item.CompanyName && x.BusinessType == item.BusinessType).FirstOrDefault().Account;
                    }
                    else
                    {
                        Money = settlementCount.Where(x => x.BELONGTO_COMPANY == item.CompanyNameOther && x.BusinessType == item.BusinessType).FirstOrDefault().Account;
                    }
                }
                if (item.BusinessTypeKey != null && item.BusinessType != null)
                {
                    if (item.CompanyNameOther == "财务共享-大众出租")
                    {
                        var settData = settlementCount.Where(x => x.BELONGTO_COMPANY == item.CompanyName && x.BusinessType == (item.BusinessTypeKey + "-" + item.BusinessType)).ToList();
                        if (settData.Count == 0)
                        {
                            continue;
                        }
                        Money = settlementCount.Where(x => x.BELONGTO_COMPANY == item.CompanyName && x.BusinessType == (item.BusinessTypeKey + "-" + item.BusinessType)).FirstOrDefault().Account;
                    }
                    else
                    {
                        var settData = settlementCount.Where(x => x.BELONGTO_COMPANY == item.CompanyNameOther && x.BusinessType == (item.BusinessTypeKey + "-" + item.BusinessType)).ToList();
                        if (settData.Count == 0)
                        {
                            continue;
                        }
                        Money = settlementCount.Where(x => x.BELONGTO_COMPANY == item.CompanyNameOther && x.BusinessType == (item.BusinessTypeKey + "-" + item.BusinessType)).FirstOrDefault().Account;
                    }
                }
                string seven = null;
                if (item.Borrow != "" && item.Borrow != null)
                {
                    if (item.Borrow.Contains("\n"))
                    {
                        item.Borrow = item.Borrow.Substring(0, item.Borrow.Length - 1);
                    }
                    seven = item.Borrow;
                    BVDetail.BorrowMoney = Money;
                    BVDetail.BorrowMoneyCount = null;
                }
                if (item.Loan != "" && item.Loan != null)
                {
                    if (item.Loan.Contains("\n"))
                    {
                        item.Loan = item.Loan.Substring(0, item.Loan.Length - 1);
                    }
                    seven = item.Loan;
                    BVDetail.LoanMoney = Money;
                    BVDetail.LoanMoneyCount = null;
                }
                if (seven != "" && seven != null)
                {
                    BVDetail.CompanySection = seven.Split(".")[0];
                    BVDetail.SubjectSection = seven.Split(".")[1];
                    BVDetail.AccountSection = seven.Split(".")[2];
                    BVDetail.CostCenterSection = seven.Split(".")[3];
                    BVDetail.SpareOneSection = seven.Split(".")[4];
                    BVDetail.SpareTwoSection = seven.Split(".")[5];
                    BVDetail.IntercourseSection = seven.Split(".")[6];
                    BVDetail.SevenSubjectName = seven + "\n" + GetSevenSubjectName(seven, myDataOther[0].AccountModeCode, myDataOther[0].CompanyCode);
                }
                BVDetailList.Add(BVDetail);
            }
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
        /// <summary>
        /// 取得某月的第一天
        /// </summary>
        /// <param name="datetime">要取得月份第一天的时间</param>
        /// <returns></returns>
        private DateTime FirstDayOfMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day);
        }
        //// <summary>
        /// 取得某月的最后一天
        /// </summary>
        /// <param name="datetime">要取得月份最后一天的时间</param>
        /// <returns></returns>
        private DateTime LastDayOfMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddMonths(1).AddDays(-1);
        }
    }
}