using Aspose.Cells;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.BankData;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherList;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Models;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement
{
    public class BankFlowTemplateController : BaseController
    {
        public BankFlowTemplateController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/BankFlowTemplate
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("2665ED30-972B-4A72-8049-1FD479502088");
            return View();
        }
        public ActionResult ImportDataCBC(string fileName)
        {
            ResultModel<string> data = new ResultModel<string>
            {
                IsSuccess = true,
                Status = "0"
            };
            //string[] fileLines = System.IO.File.ReadAllLines(Path.Combine(Server.MapPath(Global.Temp), fileName), Encoding.Default);
            Workbook workbook = new Workbook(Path.Combine(Server.MapPath(Global.Temp), fileName));
            Worksheet worksheet = workbook.Worksheets[0];
            Cells cells = worksheet.Cells;
            DataTable datatable = cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, true);//这里用到Aspose.Cells的ExportDataTableAsString方法来读取excel数据
            string str = string.Empty;
            List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
            DbBusinessDataService.Command(db =>
            {
                if (worksheet.Cells.MaxDataRow > 0)
                {
                    for (int i = 0; i < worksheet.Cells.MaxDataRow; i++)
                    {
                        var bankAccount = datatable.Rows[i]["账号"].ToString();
                        var isAny = db.Queryable<Business_BankFlowTemplate>().Any(x => x.Batch == datatable.Rows[i]["账户明细编号-交易流水号"].ToString() && x.TradingBank == "建设银行" && x.BankAccount == bankAccount);
                        if (isAny)
                        {
                            continue;
                        }
                        var companyBankData = db.Queryable<Business_CompanyBankInfo>().Single(x => x.BankAccount == bankAccount);
                        var accountMode = db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == companyBankData.AccountModeCode);
                        Business_BankFlowTemplate bankFlow = new Business_BankFlowTemplate();
                        bankFlow.TradingBank = "建设银行";
                        bankFlow.AccountModeCode = accountMode.Code;
                        bankFlow.CompanyCode = "01";
                        bankFlow.AccountModeName = accountMode.Descrption;
                        bankFlow.TurnIn = datatable.Rows[i]["借方发生额（支取）"].ObjToDecimal();
                        bankFlow.TurnOut = datatable.Rows[i]["贷方发生额（收入）"].ObjToDecimal();
                        bankFlow.BankAccount = bankAccount;
                        bankFlow.PaymentUnit = datatable.Rows[i]["账户名称"].ToString();
                        bankFlow.PayeeAccount = datatable.Rows[i]["账号"].ToString();
                        bankFlow.ReceivableAccount = datatable.Rows[i]["对方账号"].ToString();
                        bankFlow.ReceivingUnit = datatable.Rows[i]["对方户名"].ToString();
                        bankFlow.ReceivingUnitInstitution = datatable.Rows[i]["对方开户机构"].ToString();
                        bankFlow.TransactionDate = datatable.Rows[i]["交易时间"].ToString().Insert(4, "/").Insert(7, "/").ObjToDate();
                        bankFlow.Balance = datatable.Rows[i]["余额"].ObjToDecimal();
                        bankFlow.Currency = datatable.Rows[i]["币种"].ToString();
                        bankFlow.Purpose = datatable.Rows[i]["摘要"].ToString();
                        bankFlow.Remark = datatable.Rows[i]["备注"].ToString();
                        bankFlow.Batch = datatable.Rows[i]["账户明细编号-交易流水号"].ToString();
                        bankFlow.VoucherSubject = datatable.Rows[i]["凭证号"].ToString();
                        bankFlow.VoucherSubjectName = datatable.Rows[i]["凭证种类"].ToString();
                        bankFlow.CreateTime = DateTime.Now;
                        bankFlow.CreatePerson = UserInfo.LoginName;
                        bankFlow.VGUID = Guid.NewGuid();
                        bankFlowList.Add(bankFlow);
                    }
                    //按交易日期排序取最小值
                    bankFlowList = bankFlowList.OrderBy(c => c.TransactionDate).ToList();
                    if (bankFlowList.Count > 0)
                    {
                        db.Insertable(bankFlowList).ExecuteCommand();
                        //根据流水自动生成凭证
                        var userData = new List<Sys_User>();
                        DbService.Command(_db =>
                        {
                            userData = _db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
                        });
                        GenerateVoucherList(db, bankFlowList, UserInfo.LoginName, userData);
                        //同步银行流水到银行数据表
                        BankDataPack.SyncBackFlow(bankFlowList[0].TransactionDate.GetValueOrDefault().AddDays(-1));
                    }
                    data.IsSuccess = true;
                }
                else
                {
                    data.IsSuccess = false;
                    data.ResultInfo = "导入文件数据不正确！";
                }
            });
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ImportDataBCM(string fileName)
        {
            ResultModel<string> data = new ResultModel<string>
            {
                IsSuccess = true,
                Status = "0"
            };
            //string[] fileLines = System.IO.File.ReadAllLines(Path.Combine(Server.MapPath(Global.Temp), fileName), Encoding.Default);
            Workbook workbook = new Workbook(Path.Combine(Server.MapPath(Global.Temp), fileName));
            Worksheet worksheet = workbook.Worksheets[0];
            Cells cells = worksheet.Cells;
            var bankAccount = cells.GetCell(0, 1).Value;//账号
            var bankAccountName = cells.GetCell(0, 3).Value;//户名
            DataTable datatable = cells.ExportDataTableAsString(1, 0, cells.MaxDataRow, cells.MaxDataColumn + 1, true);//这里用到Aspose.Cells的ExportDataTableAsString方法来读取excel数据
            string str = string.Empty;
            List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
            DbBusinessDataService.Command(db =>
            {
                var bankData = db.Queryable<Business_CompanyBankInfo>().ToList();
                var bankFlowData = db.Queryable<Business_BankFlowTemplate>().ToList();
                var sevenData = db.Queryable<Business_SevenSection>().ToList();
                if (worksheet.Cells.MaxDataRow > 0)
                {
                    for (int i = 0; i < worksheet.Cells.MaxDataRow - 2; i++)
                    {
                        var isAny = bankFlowData.Where(x => x.Batch == datatable.Rows[i]["核心流水号"].ToString() && x.TradingBank == "交通银行" && x.BankAccount == bankAccount.ObjToString()).ToList();
                        if (isAny.Count == 1)
                        {
                            isAny[0].PaymentUnitInstitution = bankData.Where(x => x.BankAccount == bankAccount.ToString()).FirstOrDefault().BankName;
                            db.Updateable(isAny[0]).IgnoreColumns(it => it == "CreateTime").ExecuteCommand();
                            continue;
                        }
                        var companyBankData = bankData.Single(x => x.BankAccount == bankAccount.ObjToString());
                        var accountMode = sevenData.Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == companyBankData.AccountModeCode);
                        Business_BankFlowTemplate bankFlow = new Business_BankFlowTemplate();
                        bankFlow.TradingBank = "交通银行";
                        bankFlow.AccountModeCode = accountMode.Code;
                        bankFlow.CompanyCode = "01";
                        bankFlow.AccountModeName = accountMode.Descrption;
                        bankFlow.BankAccount = bankAccount.ToString();
                        bankFlow.PaymentUnitInstitution = bankData.Where(x => x.BankAccount == bankAccount.ToString()).FirstOrDefault().BankName;
                        bankFlow.ReceivableAccount = datatable.Rows[i]["对方账号"].ToString();
                        bankFlow.ReceivingUnit = datatable.Rows[i]["对方户名"].ToString();
                        bankFlow.ReceivingUnitInstitution = datatable.Rows[i]["对方行名"].ToString();
                        bankFlow.PaymentUnit = bankAccountName.ToString();
                        bankFlow.PayeeAccount = bankAccount.ToString();
                        var type = datatable.Rows[i]["借贷标志"].ToString();
                        if (type == "借")
                        {
                            bankFlow.TurnOut = 0;
                            bankFlow.TurnIn = datatable.Rows[i]["发生额"].ObjToDecimal();
                        }
                        else
                        {
                            bankFlow.TurnOut = datatable.Rows[i]["发生额"].ObjToDecimal();
                            bankFlow.TurnIn = 0;
                        }
                        bankFlow.TransactionDate = datatable.Rows[i]["交易时间"].ObjToDate();
                        bankFlow.Balance = datatable.Rows[i]["余额"].ObjToDecimal();
                        bankFlow.Currency = datatable.Rows[i]["币种"].ToString();
                        bankFlow.Purpose = datatable.Rows[i]["摘要"].ToString();
                        //bankFlow.Remark = datatable.Rows[i]["备注"].ToString();
                        bankFlow.Batch = datatable.Rows[i]["核心流水号"].ToString();
                        bankFlow.VoucherSubject = datatable.Rows[i]["凭证号码"].ToString();
                        bankFlow.VoucherSubjectName = datatable.Rows[i]["凭证种类"].ToString();
                        bankFlow.CreateTime = DateTime.Now;
                        bankFlow.CreatePerson = UserInfo.LoginName;
                        bankFlow.VGUID = Guid.NewGuid();
                        bankFlowList.Add(bankFlow);
                    }
                    //按交易日期排序取最小值
                    bankFlowList = bankFlowList.OrderBy(c => c.TransactionDate).ToList();
                    if (bankFlowList.Count > 0)
                    {
                        db.Insertable(bankFlowList).ExecuteCommand();
                        //根据流水自动生成凭证
                        var userData = new List<Sys_User>();
                        //DbService.Command(_db =>
                        //{
                        //    userData = _db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
                        //});
                        GenerateVoucherList(db, bankFlowList, UserInfo.LoginName, userData);
                        //同步银行流水到银行数据表
                        BankDataPack.SyncBackFlow(bankFlowList[0].TransactionDate.GetValueOrDefault().AddDays(-1));
                    }
                    data.IsSuccess = true;
                }
                else
                {
                    data.IsSuccess = false;
                    data.ResultInfo = "导入文件数据不正确！";
                }
            });
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBankFlowData(Business_BankFlowTemplate searchParams, GridParams para, string TransactionDateEnd)
        {
            var jsonResult = new JsonResultModel<Business_BankFlowTemplate>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                DateTime transactionDateEnd = Convert.ToDateTime(TransactionDateEnd + " 23:59:59");
                jsonResult.Rows = db.Queryable<Business_BankFlowTemplate>()
                .WhereIF(searchParams.TradingBank != null, i => i.BankAccount == searchParams.TradingBank)
                .WhereIF(searchParams.TransactionDate != null, i => i.TransactionDate >= searchParams.TransactionDate && i.TransactionDate <= transactionDateEnd)
                .WhereIF(searchParams.ReceivingUnit != null, i => i.ReceivingUnit.Contains(searchParams.ReceivingUnit))
                .WhereIF(UserInfo.LoginName != "admin", x => x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == UserInfo.CompanyCode)
                .Where(x=>x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == UserInfo.CompanyCode)
                .OrderBy(i => i.TransactionDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveBankFlow(Business_BankFlowTemplate sevenSection, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var guid = sevenSection.VGUID;
                    if (isEdit)
                    {
                        db.Updateable<Business_BankFlowTemplate>().UpdateColumns(it => new Business_BankFlowTemplate()
                        {
                            VoucherSubjectName = sevenSection.VoucherSubjectName,
                            VoucherSubject = sevenSection.VoucherSubject,
                            VoucherSummary = sevenSection.VoucherSummary,
                        }).Where(it => it.VGUID == guid).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult GetCompanySection(GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_SevenSection>();
            var response = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                //int pageCount = 0;               
                para.pagenum = para.pagenum + 1;
                response = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43" && x.Code != null && x.Status == "1")
                .OrderBy(i => i.Code, OrderByType.Asc).ToList();
                jsonResult.TotalRows = response.Count;
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SyncCurrentDayBankData()
        {
            ResultModel<string> resultModel = null;
            DbBusinessDataService.Command(db =>
            {
                resultModel = new ResultModel<string>() { IsSuccess = true, Status = "1" };
                var companyBankData = db.Queryable<Business_CompanyBankInfo>().Where(x => x.OpeningDirectBank == true).ToList();
                var bankData = db.Queryable<Business_CompanyBankInfo>().ToList();
                foreach (var item in companyBankData)
                {
                    List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
                    List<Business_BankFlowTemplate> newBankFlowList = new List<Business_BankFlowTemplate>();
                    if (item.BankName.Contains("上海银行"))
                    {
                        bankFlowList = ShanghaiBankAPI.GetShangHaiBankTradingFlow(item.BankAccount);
                    }
                    foreach (var items in bankFlowList)
                    {
                        items.BankAccount = item.BankAccount;
                        var paymentUnitInstitution = bankData.Where(x => x.BankAccount == items.BankAccount).First().BankName;
                        items.PaymentUnitInstitution = paymentUnitInstitution;
                        var accountModeName = db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == item.AccountModeCode).Descrption;
                        var isAny = db.Queryable<Business_BankFlowTemplate>().Where(x => x.Batch == items.Batch && x.BankAccount == item.BankAccount).ToList();
                        if (isAny.Count == 1)
                        {
                            //if (items.Batch == "FT19270044869552" || items.Batch == "BEA192450414366493500")
                            //{
                            //    items.AccountModeCode = item.AccountModeCode;
                            //    items.AccountModeName = accountModeName;
                            //    items.AccountModeName = accountModeName;
                            //    items.CompanyCode = item.CompanyCode;
                            //    newBankFlowList.Add(items);
                            //}
                            items.AccountModeCode = item.AccountModeCode;
                            items.AccountModeName = accountModeName;
                            items.CompanyCode = item.CompanyCode;
                            db.Updateable(items).IgnoreColumns(it => it == "CreateTime").Where(x => x.Batch == items.Batch && x.BankAccount == item.BankAccount).ExecuteCommand();
                            continue;
                        }
                        items.BankAccount = item.BankAccount;
                        items.AccountModeCode = item.AccountModeCode;
                        items.AccountModeName = accountModeName;
                        items.CompanyCode = item.CompanyCode;
                        items.CreateTime = DateTime.Now;
                        items.CreatePerson = UserInfo.LoginName;
                        newBankFlowList.Add(items);
                    }
                    if (newBankFlowList.Count > 0)
                    {
                        db.Insertable(newBankFlowList).ExecuteCommand();
                        //根据流水自动生成凭证
                        var userData = new List<Sys_User>();
                        //DbService.Command(_db =>
                        //{
                        //    userData = _db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
                        //});
                        GenerateVoucherList(db, newBankFlowList, UserInfo.LoginName, userData);
                    }
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SyncYesterdayBankData()
        {
            ResultModel<string> resultModel = null;
            DbBusinessDataService.Command(db =>
            {
                try
                {
                    db.Ado.BeginTran();
                    resultModel = new ResultModel<string>() { IsSuccess = true, Status = "1" };
                    var companyBankData = db.Queryable<Business_CompanyBankInfo>().Where(x => x.OpeningDirectBank == true).ToList();
                    var bankData = db.Queryable<Business_CompanyBankInfo>().ToList();
                    foreach (var item in companyBankData)
                    {
                        List<Business_BankFlowTemplate> bankFlowList = new List<Business_BankFlowTemplate>();
                        List<Business_BankFlowTemplate> newBankFlowList = new List<Business_BankFlowTemplate>();
                        if (item.BankName.Contains("上海银行"))
                        {
                            bankFlowList = ShanghaiBankAPI.GetShangHaiBankYesterdayTradingFlow(item.BankAccount);
                        }
                        foreach (var items in bankFlowList)
                        {
                            items.BankAccount = item.BankAccount;
                            var paymentUnitInstitution = bankData.Where(x => x.BankAccount == items.BankAccount).First().BankName;
                            items.PaymentUnitInstitution = paymentUnitInstitution;
                            var accountModeName = db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == item.AccountModeCode).Descrption;
                            var isAny = db.Queryable<Business_BankFlowTemplate>().Where(x => x.Batch == items.Batch && x.BankAccount == items.BankAccount).ToList();
                            if (isAny.Count > 0)
                            {
                                //if (items.Batch == "BEA192470414233583300" || items.Batch == "BEA192450414366493500")
                                //{
                                //    items.AccountModeCode = item.AccountModeCode;
                                //    items.AccountModeName = accountModeName;
                                //    items.AccountModeName = accountModeName;
                                //    items.CompanyCode = item.CompanyCode;
                                //    newBankFlowList.Add(items);
                                //}
                                items.BankAccount = item.BankAccount;
                                items.AccountModeCode = item.AccountModeCode;
                                items.AccountModeName = accountModeName;
                                items.CompanyCode = item.CompanyCode;
                                db.Updateable(items).IgnoreColumns(it => it == "CreateTime").Where(x => x.Batch == items.Batch && x.BankAccount == item.BankAccount).ExecuteCommand();
                                continue;
                            }
                            items.AccountModeCode = item.AccountModeCode;
                            items.AccountModeName = accountModeName;
                            items.CompanyCode = item.CompanyCode;
                            items.CreateTime = DateTime.Now;
                            items.CreatePerson = "admin";

                            newBankFlowList.Add(items);
                        }
                        if (newBankFlowList.Count > 0)
                        {
                            db.Insertable<Business_BankFlowTemplate>(newBankFlowList).ExecuteCommand();
                            //根据流水自动生成凭证
                            var userData = new List<Sys_User>();
                            //DbService.Command(_db =>
                            //{
                            //    userData = _db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
                            //});
                            GenerateVoucherList(db, newBankFlowList, UserInfo.LoginName, userData);
                        }
                    }
                    db.Ado.CommitTran();
                }
                catch (Exception ex)
                {
                    db.Ado.RollbackTran();
                    throw ex;
                }

            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBankInfo()
        {
            var result = new List<Business_CompanyBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_CompanyBankInfo>().Where(x => x.AccountModeCode == UserInfo.AccountModeCode).OrderBy("BankAccount asc").ToList();
            });
            return Json(result, JsonRequestBehavior.AllowGet); ;
        }
        public static void GenerateVoucherList(SqlSugarClient db, List<Business_BankFlowTemplate> newBankFlowList, string loginName, List<Sys_User> userData)
        {
            List<Business_VoucherList> VoucherList = new List<Business_VoucherList>();
            List<Business_VoucherDetail> BVDetailList = new List<Business_VoucherDetail>();
            List<AssetsGeneralLedger_Swap> assetList = new List<AssetsGeneralLedger_Swap>();//凭证中间表List
            var x = -1;
            var guid = Guid.Empty;
            var orderListDraft = db.Queryable<Business_OrderListDraft>().ToList();
            var orderList = db.Queryable<Business_OrderList>().ToList();
            var userCompanySet = db.Queryable<Business_UserCompanySetDetail>().ToList();
            var bankChannel = db.Queryable<T_BankChannelMapping>().Where(i => (i.IsUnable == "启用" || i.IsUnable == null)).ToList();
            var paySetting = db.Queryable<Business_PaySettingDetail>().ToList();
            var voucherData = db.Queryable<Business_VoucherList>().Where(i => i.VoucherType == "银行类" && i.Automatic != "3").ToList();
            #region 循环银行流水数组
            foreach (var item in newBankFlowList)
            {
                //主信息
                Business_VoucherList voucher = new Business_VoucherList();
                voucher.AccountingPeriod = item.TransactionDate;
                voucher.AccountModeName = item.AccountModeName;
                voucher.CompanyCode = item.CompanyCode;
                voucher.CompanyName = item.PaymentUnit.ToDBC();
                voucher.BatchName = "银行类" + item.TransactionDate.GetValueOrDefault().ToString("yyyyMMdd");
                //var voucherName = GetVoucherName(item.AccountModeName, item.CompanyCode);
                var bank = "Bank" + item.AccountModeCode + item.CompanyCode;
                voucher.VoucherNo = db.Ado.SqlQuery<string>(@"declare @output varchar(50) exec getautono '"+ bank + "', @output output  select @output").FirstOrDefault();
                voucherData = voucherData.Where(i => i.AccountModeName == item.AccountModeName && i.CompanyCode == item.CompanyCode).ToList();
                voucher.DocumentMaker = "";
                voucher.Status = "1";
                voucher.VoucherDate = item.TransactionDate;
                voucher.VoucherType = "银行类";
                voucher.Automatic = "1";//自动
                voucher.TradingBank = item.TradingBank;
                voucher.ReceivingUnit = item.ReceivingUnit;
                voucher.TransactionDate = item.TransactionDate;
                voucher.Batch = item.Batch;
                voucher.CreateTime = DateTime.Now;
                guid = Guid.NewGuid();
                voucher.VGUID = guid;
                foreach (var user in userData)
                {
                    switch (user.Role)
                    {
                        case "财务经理":voucher.FinanceDirector = user.LoginName;break;
                        case "财务主管": voucher.Bookkeeping = user.LoginName; break;
                        //case "审核岗": voucher.Auditor = user.LoginName; break;
                        case "出纳": voucher.Cashier = user.LoginName; break;
                        default: break;
                    }
                }
                //科目信息
                Business_VoucherDetail BVDetail = new Business_VoucherDetail();
                BVDetail.Abstract = item.Remark;
                var sevenSubject = GetSevenSubject(item.BankAccount);
                var subject = "";
                if (item.TurnOut == 0)
                {
                    //转入(贷)
                    voucher.CreditAmountTotal = item.TurnIn;
                    BVDetail.LoanMoney = item.TurnIn;
                    BVDetail.BorrowMoney = 0;
                    BVDetail.LoanMoneyCount = item.TurnIn;
                    BVDetail.BorrowMoneyCount = 0;
                    subject = sevenSubject.Loan;
                }
                else
                {
                    //转出(借)
                    voucher.DebitAmountTotal = item.TurnOut;
                    BVDetail.BorrowMoney = item.TurnOut;
                    BVDetail.LoanMoney = 0;
                    BVDetail.BorrowMoneyCount = item.TurnOut;
                    BVDetail.LoanMoneyCount = 0;
                    subject = sevenSubject.Borrow;
                    var bankChannelOne = bankChannel.Where(it => it.BankAccount == item.ReceivableAccount).ToList().FirstOrDefault();
                    if(bankChannelOne != null)
                    {
                        //对方账号下借贷配置信息
                        var loadData = paySetting.Where(j => j.PayVGUID == bankChannelOne.VGUID.ToString() && j.Loan != null && j.AccountModeCode == item.AccountModeCode && j.CompanyCode == item.CompanyCode).ToList();
                        var borrowData = paySetting.Where(j => j.PayVGUID == bankChannelOne.VGUID.ToString() && j.Borrow != null && j.AccountModeCode == item.AccountModeCode && j.CompanyCode == item.CompanyCode).ToList();
                        if (borrowData.Count >= 1)
                        {
                            var index = 0;
                            foreach (var borrow in borrowData)
                            {
                                index++;
                                Business_VoucherDetail BVDetail3 = new Business_VoucherDetail();
                                subject = borrow.Borrow;
                                if (subject != "" && subject != null)
                                {
                                    if (subject.Contains("\n"))
                                    {
                                        subject = subject.Substring(0, subject.Length - 1);
                                    }
                                    var seven = subject.Split(".");
                                    BVDetail3.CompanySection = seven[0];
                                    BVDetail3.SubjectSection = seven[1];
                                    BVDetail3.AccountSection = seven[2];
                                    BVDetail3.CostCenterSection = seven[3];
                                    BVDetail3.SpareOneSection = seven[4];
                                    BVDetail3.SpareTwoSection = seven[5];
                                    BVDetail3.IntercourseSection = seven[6];
                                    //BVDetail.SubjectSectionName = item.SubjectSectionName;
                                    BVDetail3.SevenSubjectName = subject + "\n" + GetSevenSubjectName(subject, item.AccountModeCode, item.CompanyCode);
                                }
                                BVDetail3.BorrowMoney = 0;
                                BVDetail3.BorrowMoneyCount = 0;
                                if (index == 1)
                                {
                                    BVDetail3.BorrowMoney = item.TurnOut;
                                    BVDetail3.BorrowMoneyCount = item.TurnOut;
                                }
                                BVDetail3.Abstract = borrow.Remark;
                                BVDetail3.VGUID = Guid.NewGuid();
                                BVDetail3.VoucherVGUID = guid;
                                BVDetailList.Add(BVDetail3);
                            }
                            GetOtherSubject2(db, BVDetailList, guid, item, assetList, voucher, orderListDraft, orderList, userCompanySet);//通过流水找银行渠道 
                            VoucherList.Add(voucher);
                            continue;
                        }
                    }
                    else//if ((loadData.Count == 1 || loadData.Count == 0) && borrowData.Count == 1)
                    {
                        
                    }
                }
                if (subject != "" && subject != null)
                {
                    if (subject.Contains("\n"))
                    {
                        subject = subject.Substring(0, subject.Length - 1);
                    }
                    var seven = subject.Split(".");
                    BVDetail.CompanySection = seven[0];
                    BVDetail.SubjectSection = seven[1];
                    BVDetail.AccountSection = seven[2];
                    BVDetail.CostCenterSection = seven[3];
                    BVDetail.SpareOneSection = seven[4];
                    BVDetail.SpareTwoSection = seven[5];
                    BVDetail.IntercourseSection = seven[6];
                    //BVDetail.SubjectSectionName = item.SubjectSectionName;
                    BVDetail.SevenSubjectName = subject + "\n" + GetSevenSubjectName(subject, item.AccountModeCode, item.CompanyCode);
                }
                BVDetail.JE_LINE_NUMBER = 0;
                BVDetail.VGUID = Guid.NewGuid();
                BVDetail.VoucherVGUID = guid;
                VoucherList.Add(voucher);
                BVDetailList.Add(BVDetail);
                //GetAssetsGeneralLedger(BVDetail, assetList, voucher, guid, 0);//将借贷数据同步中间表
                //GetOtherSubject(BVDetailList, newBankFlowList, guid, item);//通过银行渠道找流水
                GetOtherSubject2(db, BVDetailList, guid, item, assetList, voucher, orderListDraft, orderList, userCompanySet);//通过流水找银行渠道 
            }
            #endregion
            if (VoucherList.Count > 0 && BVDetailList.Count > 0)
            {
                db.Insertable(VoucherList).ExecuteCommand();
                db.Insertable(BVDetailList).ExecuteCommand();
            }
            if (assetList.Count > 0)
            {
                db.Insertable(assetList).ExecuteCommand();
            }
        }
        public static void GetOtherSubject(List<Business_VoucherDetail> BVDetailList, List<Business_BankFlowTemplate> newBankFlowList, Guid guid, Business_BankFlowTemplate item)
        {
            SqlSugarClient db = DbBusinessDataConfig.GetInstance();
            Business_VoucherDetail BVDetail = new Business_VoucherDetail();
            var bankChannel = db.Queryable<T_BankChannelMapping>().Where(x => x.IsUnable != "禁用").ToList();
            foreach (var it in bankChannel)
            {
                var subject = "";
                var newBank = newBankFlowList.Where(x => x.ReceivableAccount == item.BankAccount).ToList();
                if (newBank.Count > 0)
                {
                    foreach (var bank in newBank)
                    {
                        if (bank.VGUID != item.VGUID)
                        {
                            continue;
                        }
                        if (item.TurnOut == 0)
                        {
                            subject = it.Borrow;
                        }
                        else
                        {
                            subject = it.Loan;
                        }
                        if (subject != "" || subject != null)
                        {
                            if (subject.Contains("\n"))
                            {
                                subject = subject.Substring(0, subject.Length - 1);
                            }
                            var seven = subject.Split(".");
                            BVDetail.CompanySection = seven[0];
                            BVDetail.SubjectSection = seven[1];
                            BVDetail.AccountSection = seven[2];
                            BVDetail.CostCenterSection = seven[3];
                            BVDetail.SpareOneSection = seven[4];
                            BVDetail.SpareTwoSection = seven[5];
                            BVDetail.IntercourseSection = seven[6];
                            //BVDetail.SubjectSectionName = item.SubjectSectionName;
                            BVDetail.SevenSubjectName = subject + "\n" + GetSevenSubjectName(subject, item.AccountModeCode, item.CompanyCode);
                        }
                        BVDetail.VGUID = Guid.NewGuid();
                        BVDetail.VoucherVGUID = guid;
                        BVDetailList.Add(BVDetail);
                        return;
                    }
                }
            }
        }
        public static void GetOtherSubject2(SqlSugarClient db, List<Business_VoucherDetail> BVDetailList, Guid guid, Business_BankFlowTemplate item, List<AssetsGeneralLedger_Swap> assetList, Business_VoucherList voucher
            , List<Business_OrderListDraft> orderListDraft, List<Business_OrderList> orderList, List<Business_UserCompanySetDetail> userCompanySet)
        {
            Business_VoucherDetail BVDetail = new Business_VoucherDetail();
            var bankChannel = db.Queryable<T_BankChannelMapping>().Where(x => (x.IsUnable == "启用" || x.IsUnable == null) && x.BankAccount == item.ReceivableAccount).ToList();
            if (bankChannel.Count > 0)
            {
                var bankChannelOne = bankChannel.First();
                //对方账号下借贷配置信息
                var borrowLoadData = db.Queryable<Business_PaySettingDetail>().Where(x => x.PayVGUID == bankChannelOne.VGUID.ToString()).ToList();
                //金额报表,数据源
                var month = item.TransactionDate.TryToDate().ToString("yyyy-MM");
                var bankFlowList = db.Ado.SqlQuery<usp_RevenueAmountReport>(@"exec usp_RevenueAmountReport @Month,@Channel", new { Month = month, Channel = "" }).ToList();
                var subject = "";
                if (item.TurnOut == 0)
                {
                    //付款业务
                    Regex regExp = new Regex("^[0-9]*$");
                    if (item.Purpose.Length == 19 && regExp.IsMatch(item.Purpose))
                    {
                        //保险系统银行流水数据通过备注中的流水号匹配订单配置信息
                        var osno = orderListDraft.Where(x => x.OSNO == item.Purpose).ToList();
                        if (osno.Count == 1)
                        {
                            var order = orderList.Where(x => x.BusinessSubItem1 == osno[0].BusinessSubItem1).First();
                            var orderDetail = userCompanySet.Where(x => x.OrderVGUID == order.VGUID.ToString() && x.CompanyName == osno[0].OrderCompany && x.Isable == true).ToList();
                            if (orderDetail.Count > 0)
                            {
                                subject = orderDetail[0].Borrow;
                                if (subject != "" && subject != null)
                                {
                                    if (subject.Contains("\n"))
                                    {
                                        subject = subject.Substring(0, subject.Length - 1);
                                    }
                                    var seven = subject.Split(".");
                                    BVDetail.CompanySection = seven[0];
                                    BVDetail.SubjectSection = seven[1];
                                    BVDetail.AccountSection = seven[2];
                                    BVDetail.CostCenterSection = seven[3];
                                    BVDetail.SpareOneSection = seven[4];
                                    BVDetail.SpareTwoSection = seven[5];
                                    BVDetail.IntercourseSection = seven[6];
                                    //BVDetail.SubjectSectionName = item.SubjectSectionName;
                                    BVDetail.SevenSubjectName = subject + "\n" + GetSevenSubjectName(subject, item.AccountModeCode, item.CompanyCode);
                                }
                                BVDetail.VGUID = Guid.NewGuid();
                                BVDetail.VoucherVGUID = guid;
                                BVDetailList.Add(BVDetail);
                            }
                        }
                    }
                    else
                    {
                        //subject = bankChannelOne.Borrow;
                    }
                }
                else
                {
                    //对方账号下贷方配置（多贷）
                    var loanData = borrowLoadData.Where(x => x.Loan != null && x.AccountModeCode == item.AccountModeCode && x.CompanyCode == item.CompanyCode).ToList();
                    foreach (var it in loanData)
                    {
                        Business_VoucherDetail BVDetail2 = new Business_VoucherDetail();
                        //收款业务
                        subject = it.Loan;
                        if (subject != "" && subject != null)
                        {
                            if (subject.Contains("\n"))
                            {
                                subject = subject.Substring(0, subject.Length - 1);
                            }
                            var seven = subject.Split(".");
                            BVDetail2.CompanySection = seven[0];
                            BVDetail2.SubjectSection = seven[1];
                            BVDetail2.AccountSection = seven[2];
                            BVDetail2.CostCenterSection = seven[3];
                            BVDetail2.SpareOneSection = seven[4];
                            BVDetail2.SpareTwoSection = seven[5];
                            BVDetail2.IntercourseSection = seven[6];
                            //BVDetail.SubjectSectionName = item.SubjectSectionName;
                            BVDetail2.SevenSubjectName = subject + "\n" + GetSevenSubjectName(subject, item.AccountModeCode, item.CompanyCode);
                        }
                        if (loanData.Count > 1)
                        {
                            //从金额报表中按配置获取金额
                            var amountReport = bankFlowList.Where(x => x.OrganizationName == it.TransferCompany && x.Channel_Id == it.Channel && x.RevenueDate == item.TransactionDate.TryToDate().ToString("yyyy-MM-dd")).ToList();
                            if (amountReport.Count > 0 && it.TransferType != null)
                            {
                                switch (it.TransferType)
                                {
                                    case "银行收款": BVDetail2.LoanMoney = amountReport[0].ActualAmountTotal; break;
                                    case "营收缴款": BVDetail2.LoanMoney = amountReport[0].PaymentAmountTotal; break;
                                    case "手续费": BVDetail2.LoanMoney = amountReport[0].CompanyBearsFeesTotal; break;
                                    default:
                                        break;
                                }
                                //BVDetail2.LoanMoneyCount = amountReport[0].ActualAmountTotal + amountReport[0].PaymentAmountTotal + amountReport[0].CompanyBearsFeesTotal;
                            }
                        }
                        else
                        {
                            //一借一贷,借贷相平
                            BVDetail2.LoanMoney = item.TurnOut;
                            BVDetail2.LoanMoneyCount = item.TurnOut;
                            voucher.CreditAmountTotal = item.TurnOut;
                            voucher.DebitAmountTotal = item.TurnOut;
                        }
                        BVDetail2.ReceivableAccount = item.ReceivableAccount;//对方账号,用于轮循贷方明细找到对应金额
                        BVDetail2.Abstract = it.Remark;
                        BVDetail2.VGUID = Guid.NewGuid();
                        BVDetail2.VoucherVGUID = guid;
                        BVDetailList.Add(BVDetail2);
                    }

                }
                //GetAssetsGeneralLedger(BVDetail, assetList, voucher, guid, 1);//将借贷数据同步中间表
            }
        }
        public static void GetAssetsGeneralLedger(Business_VoucherDetail BVDetail, List<AssetsGeneralLedger_Swap> assetList, Business_VoucherList voucher, Guid guid, int i)
        {
            //凭证中间表
            var type = "";
            switch (voucher.VoucherType)
            {
                case "现金类": type = "x.现金"; break;
                case "银行类": type = "y.银行"; break;
                case "转账类": type = "z.转账"; break;
                default: break;
            }
            AssetsGeneralLedger_Swap asset = new AssetsGeneralLedger_Swap();
            asset.CREATE_DATE = DateTime.Now;
            //asset.SubjectVGUID = guid;
            asset.LINE_ID = guid.ToString();
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
            asset.CURRENCY_CONVERSION_DATE = DateTime.Now;
            asset.CURRENCY_CONVERSION_RATE = null;//币种是RMB时为空
            asset.STATUS = "1";
            //asset.VGUID = Guid.NewGuid();
            asset.TRASACTION_ID = Guid.NewGuid().ToString();
            asset.JE_LINE_NUMBER = i;
            asset.SEGMENT1 = BVDetail.CompanySection;
            asset.SEGMENT2 = BVDetail.SubjectSection;
            asset.SEGMENT3 = BVDetail.AccountSection;
            asset.SEGMENT4 = BVDetail.CostCenterSection;
            asset.SEGMENT5 = BVDetail.SpareOneSection;
            asset.SEGMENT6 = BVDetail.SpareTwoSection;
            asset.SEGMENT7 = BVDetail.IntercourseSection;
            asset.ENTERED_CR = BVDetail.LoanMoney.TryToString();
            asset.ENTERED_DR = BVDetail.BorrowMoney.TryToString();
            asset.ACCOUNTED_DR = BVDetail.BorrowMoney.TryToString();
            asset.ACCOUNTED_CR = BVDetail.LoanMoney.TryToString();
            assetList.Add(asset);
        }
        public static string GetVoucherName(string accountModeName,string companyCode)
        {
            using (SqlSugarClient db = DbBusinessDataConfig.GetInstance())
            {
                var date = DateTime.Now;
                var voucherNo = db.Ado.GetString(@"select top 1 VoucherNo from Business_VoucherList a where DATEDIFF(month,a.CreateTime,@NowDate)=0 and VoucherType='银行类' and  Automatic!='3' and AccountModeName=@AccountModeName and CompanyCode=@CompanyCode
                              order by VoucherNo desc", new { @NowDate = date, @AccountModeName = accountModeName, @CompanyCode = companyCode });
                var batchNo = 0;
                if (voucherNo.IsValuable() && voucherNo.Length > 4)
                {
                    batchNo = voucherNo.Substring(voucherNo.Length - 4, 4).TryToInt();
                }
                return DateTime.Now.ToString("yyyyMM") + (batchNo + 1).TryToString().PadLeft(4, '0');
            }
        }
        public static string GetTransferVoucherName()
        {
            using (SqlSugarClient db = DbBusinessDataConfig.GetInstance())
            {
                var date = DateTime.Now;
                var voucherNo = db.Ado.GetString(@"select top 1 VoucherNo from Business_VoucherList a where DATEDIFF(month,a.CreateTime,@NowDate)=0 and VoucherType='转账类' and  Automatic!='3'
                              order by VoucherNo desc", new { @NowDate = date });
                var batchNo = 0;
                if (voucherNo.IsValuable() && voucherNo.Length > 4)
                {
                    batchNo = voucherNo.Substring(voucherNo.Length - 4, 4).TryToInt();
                }
                return DateTime.Now.ToString("yyyyMM") + (batchNo + 1).TryToString().PadLeft(4, '0');
            }
        }
        public static Business_CompanyBankInfo GetSevenSubject(string bankAccount)
        {
            using (SqlSugarClient db = DbBusinessDataConfig.GetInstance())
            {
                var result = db.Queryable<Business_CompanyBankInfo>().Single(x => x.BankAccount == bankAccount);
                return result;
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
        internal static void TransferVoucherList(SqlSugarClient _db, List<usp_RevenueAmountReport> bankData, string loginName)
        {
            var x = -1;
            var guid = Guid.Empty;
            var transferData = _db.Queryable<Business_TransferSetting>().ToList();
            List<Business_VoucherList> VoucherList = new List<Business_VoucherList>();
            List<Business_VoucherDetail> BVDetailList = new List<Business_VoucherDetail>();
            //bankFlowList昨天金额报表数据
            foreach (var item in bankData)
            {
                //主信息
                Business_VoucherList voucher = new Business_VoucherList();
                voucher.AccountingPeriod = DateTime.Now.AddDays(-1);
                voucher.AccountModeName = "大众营运总帐帐簿2016";
                voucher.CompanyCode = "01";
                voucher.CompanyName = "大众交通(集团)股份有限公司大众出租汽车分公司";
                voucher.BatchName = "转账类" + voucher.AccountingPeriod.GetValueOrDefault().ToString("yyyyMMdd");
                var voucherName = GetTransferVoucherName();
                x++;
                var voucherNo = voucherName.Substring(voucherName.Length - 4, 4).TryToInt();
                voucher.VoucherNo = voucher.AccountingPeriod.GetValueOrDefault().ToString("yyyyMMdd") + (voucherNo + x).TryToString().PadLeft(4, '0');
                voucher.DocumentMaker = loginName;
                voucher.Status = "1";
                voucher.VoucherDate = DateTime.Now;
                voucher.VoucherType = "转账类";
                voucher.Automatic = "1";//自动
                voucher.CreateTime = DateTime.Now;
                guid = Guid.NewGuid();
                voucher.VGUID = guid;
                voucher.CreditAmountTotal = item.ActualAmountTotal + item.PaymentAmountTotal + item.CompanyBearsFeesTotal;//银行收款+营收缴款+手续费
                voucher.DebitAmountTotal = voucher.CreditAmountTotal;
                VoucherList.Add(voucher);
                //科目明细信息
                Business_VoucherDetail BVDetail = new Business_VoucherDetail();
                BVDetail.Abstract = "驾驶员缴费";
                var sevenSubject = GetSevenSubject("31685803002369318");
                var subject = "";
                subject = sevenSubject.Borrow;
                BVDetail.BorrowMoney = voucher.DebitAmountTotal;
                BVDetail.BorrowMoneyCount = voucher.DebitAmountTotal;
                if (subject != "" && subject != null)
                {
                    if (subject.Contains("\n"))
                    {
                        subject = subject.Substring(0, subject.Length - 1);
                    }
                    var seven = subject.Split(".");
                    BVDetail.CompanySection = seven[0];
                    BVDetail.SubjectSection = seven[1];
                    BVDetail.AccountSection = seven[2];
                    BVDetail.CostCenterSection = seven[3];
                    BVDetail.SpareOneSection = seven[4];
                    BVDetail.SpareTwoSection = seven[5];
                    BVDetail.IntercourseSection = seven[6];
                    //BVDetail.SubjectSectionName = item.SubjectSectionName;
                    BVDetail.SevenSubjectName = subject + "\n" + GetSevenSubjectName(subject, "1002", "01");
                }
                BVDetail.JE_LINE_NUMBER = 0;
                BVDetail.VGUID = Guid.NewGuid();
                BVDetail.VoucherVGUID = guid;
                BVDetailList.Add(BVDetail);
                //多贷
                var index = 0;
                if (item.ActualAmountTotal != 0)
                {
                    index = index + 1;
                }
                if (item.PaymentAmountTotal != 0)
                {
                    index = index + 1;
                }
                if (item.CompanyBearsFeesTotal != 0)
                {
                    index = index + 1;
                }
                for (int i = 0; i < index; i++)
                {
                    Business_VoucherDetail BVDetail2 = new Business_VoucherDetail();
                    List<Business_TransferSetting> sevenSubject2 = null;
                    var sevenSubjectName = "";
                    var accountModeCode = "";
                    var companyCode = "";
                    switch (i)
                    {
                        case 0:
                            BVDetail2.LoanMoney = item.ActualAmountTotal;
                            sevenSubject2 = transferData.Where(it => it.TransferCompany == item.OrganizationName && it.Channel == item.Channel_Id && it.TransferType == "银行收款").ToList();
                            if (sevenSubject2.Count == 1)
                            {
                                BVDetail2.Abstract = "驾驶员缴费--银行收款";
                                accountModeCode = sevenSubject2[0].AccountModeCode;
                                companyCode = sevenSubject2[0].CompanyCode;
                                sevenSubjectName = sevenSubject2[0].Loan;
                            }
                            break;
                        case 1:
                            BVDetail2.LoanMoney = item.PaymentAmountTotal;
                            sevenSubject2 = transferData.Where(it => it.TransferCompany == item.OrganizationName && it.Channel == item.Channel_Id && it.TransferType == "营收缴款").ToList();
                            if (sevenSubject2.Count == 1)
                            {
                                BVDetail2.Abstract = "驾驶员缴费--营收缴款";
                                accountModeCode = sevenSubject2[0].AccountModeCode;
                                companyCode = sevenSubject2[0].CompanyCode;
                                sevenSubjectName = sevenSubject2[0].Loan;
                            }
                            break;
                        case 2:
                            BVDetail2.LoanMoney = item.CompanyBearsFeesTotal;
                            sevenSubject2 = transferData.Where(it => it.TransferCompany == item.OrganizationName && it.Channel == item.Channel_Id && it.TransferType == "手续费").ToList();
                            if (sevenSubject2.Count == 1)
                            {
                                BVDetail2.Abstract = "驾驶员缴费--手续费";
                                accountModeCode = sevenSubject2[0].AccountModeCode;
                                companyCode = sevenSubject2[0].CompanyCode;
                                sevenSubjectName = sevenSubject2[0].Loan;
                            }
                            break;
                        default:
                            break;
                    }
                    if (sevenSubjectName != "" && sevenSubjectName != null)
                    {
                        if (sevenSubjectName.Contains("\n"))
                        {
                            sevenSubjectName = sevenSubjectName.Substring(0, sevenSubjectName.Length - 1);
                        }
                        var seven = sevenSubjectName.Split(".");
                        BVDetail2.CompanySection = seven[0];
                        BVDetail2.SubjectSection = seven[1];
                        BVDetail2.AccountSection = seven[2];
                        BVDetail2.CostCenterSection = seven[3];
                        BVDetail2.SpareOneSection = seven[4];
                        BVDetail2.SpareTwoSection = seven[5];
                        BVDetail2.IntercourseSection = seven[6];
                        //BVDetail.SubjectSectionName = item.SubjectSectionName;
                        BVDetail2.SevenSubjectName = sevenSubjectName + "\n" + GetSevenSubjectName(sevenSubjectName, accountModeCode, companyCode);
                    }
                    BVDetail2.LoanMoneyCount = voucher.CreditAmountTotal;
                    BVDetail2.JE_LINE_NUMBER = i + 1;
                    BVDetail2.VGUID = Guid.NewGuid();
                    BVDetail2.VoucherVGUID = guid;
                    BVDetailList.Add(BVDetail2);
                }
            }
            if (VoucherList.Count > 0 && BVDetailList.Count > 0)
            {
                _db.Insertable(VoucherList).ExecuteCommand();
                _db.Insertable(BVDetailList).ExecuteCommand();
            }
        }
    }
}