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
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
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
                        var companyBankData = db.Queryable<Business_CompanyBankInfo>().Single(x =>x.BankAccount == bankAccount);
                        var accountMode = db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == companyBankData.AccountModeCode);
                        Business_BankFlowTemplate bankFlow = new Business_BankFlowTemplate();
                        bankFlow.TradingBank = "建设银行";
                        bankFlow.AccountModeCode = accountMode.Code;
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
                        GenerateVoucherList(bankFlowList, UserInfo.LoginName);
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
                if (worksheet.Cells.MaxDataRow > 0)
                {
                    for (int i = 0; i < worksheet.Cells.MaxDataRow-1; i++)
                    {
                        var isAny = db.Queryable<Business_BankFlowTemplate>().Any(x => x.Batch == datatable.Rows[i]["核心流水号"].ToString() && x.TradingBank == "交通银行" && x.BankAccount == bankAccount.ObjToString());
                        if (isAny)
                        {
                            continue;
                        }
                        var companyBankData = db.Queryable<Business_CompanyBankInfo>().Single(x => x.BankAccount == bankAccount.ObjToString());
                        var accountMode = db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == companyBankData.AccountModeCode);
                        Business_BankFlowTemplate bankFlow = new Business_BankFlowTemplate();
                        bankFlow.TradingBank = "交通银行";
                        bankFlow.AccountModeCode = accountMode.Code;
                        bankFlow.AccountModeName = accountMode.Descrption;
                        bankFlow.BankAccount = bankAccount.ToString();
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
                        GenerateVoucherList(bankFlowList, UserInfo.LoginName);
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
        public JsonResult GetBankFlowData(Business_BankFlowTemplate searchParams, GridParams para,string TransactionDateEnd)
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
                .WhereIF(searchParams.PaymentUnit != null, i => i.PaymentUnit == searchParams.PaymentUnit)
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
                        var accountModeName = db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == item.AccountModeCode).Descrption;
                        var isAny = db.Queryable<Business_BankFlowTemplate>().Where(x => x.Batch == items.Batch && x.BankAccount == item.BankAccount).ToList();
                        if (isAny.Count == 1)
                        {
                            //newBankFlowList.Add(items);
                            //isAny[0].BankAccount = item.BankAccount;
                            //isAny[0].TurnIn = items.TurnIn;
                            //isAny[0].TurnOut = items.TurnOut;
                            items.AccountModeCode = item.AccountModeCode;
                            items.AccountModeName = accountModeName;
                            items.CompanyCode = item.CompanyCode;
                            db.Updateable<Business_BankFlowTemplate>(items).Where(x => x.Batch == items.Batch && x.BankAccount == item.BankAccount).ExecuteCommand();
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
                        db.Insertable<Business_BankFlowTemplate>(newBankFlowList).ExecuteCommand();
                        //根据流水自动生成凭证
                        GenerateVoucherList(newBankFlowList, UserInfo.LoginName);
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
                resultModel = new ResultModel<string>() { IsSuccess = true, Status = "1" };
                var companyBankData = db.Queryable<Business_CompanyBankInfo>().Where(x => x.OpeningDirectBank == true).ToList();
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
                        var accountModeName = db.Queryable<Business_SevenSection>().Single(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == item.AccountModeCode).Descrption;
                        var isAny = db.Queryable<Business_BankFlowTemplate>().Where(x => x.Batch == items.Batch && x.BankAccount == items.BankAccount).ToList();
                        if (isAny.Count > 0)
                        {
                            //isAny[0].BankAccount = item.BankAccount;
                            //isAny[0].TurnIn = items.TurnIn;
                            //isAny[0].TurnOut = items.TurnOut;
                            items.AccountModeName = item.AccountModeCode;
                            items.AccountModeName = accountModeName;
                            items.CompanyCode = item.CompanyCode;
                            db.Updateable<Business_BankFlowTemplate>(items).Where(x => x.Batch == items.Batch && x.BankAccount == item.BankAccount).ExecuteCommand();
                            continue;
                        }
                        items.BankAccount = item.BankAccount;
                        items.AccountModeCode = item.AccountModeCode;
                        items.AccountModeName = accountModeName;
                        items.CompanyCode = item.CompanyCode;
                        items.CreateTime = DateTime.Now;
                        items.CreatePerson = "sysAdmin";
                        
                        newBankFlowList.Add(items);
                    }
                    if (newBankFlowList.Count > 0)
                    {
                        db.Insertable<Business_BankFlowTemplate>(newBankFlowList).ExecuteCommand();
                        //根据流水自动生成凭证
                        GenerateVoucherList(newBankFlowList, UserInfo.LoginName);
                    }                }
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
        public static void GenerateVoucherList(List<Business_BankFlowTemplate> newBankFlowList, string loginName)
        {
            SqlSugarClient db = DbBusinessDataConfig.GetInstance();
            List<Business_VoucherList> VoucherList = new List<Business_VoucherList>();
            List<Business_VoucherDetail> BVDetailList = new List<Business_VoucherDetail>();
            List<Business_AssetsGeneralLedger_Swap> assetList = new List<Business_AssetsGeneralLedger_Swap>();//凭证中间表List
            var x = -1;
            var guid = Guid.Empty;
            var orderListDraft = db.Queryable<Business_OrderListDraft>().ToList();
            var orderList = db.Queryable<Business_OrderList>().ToList();
            var userCompanySet = db.Queryable<Business_UserCompanySetDetail>().ToList();
            #region 循环银行流水数组
            foreach (var item in newBankFlowList)
            {
                //主信息
                Business_VoucherList voucher = new Business_VoucherList();
                voucher.AccountingPeriod = item.TransactionDate;
                voucher.AccountModeName = item.AccountModeName;
                voucher.CompanyCode = item.CompanyCode;
                voucher.CompanyName = item.PaymentUnit;
                voucher.BatchName = "银行类" + item.TransactionDate.GetValueOrDefault().ToString("yyyyMMdd");
                var voucherName = GetVoucherName();
                x++;
                var voucherNo = voucherName.Substring(voucherName.Length - 4, 4).TryToInt();
                voucher.VoucherNo = item.TransactionDate.GetValueOrDefault().ToString("yyyyMMdd") + (voucherNo + x).TryToString().PadLeft(4, '0');
                voucher.DocumentMaker = loginName;
                voucher.Status = "1";
                voucher.VoucherDate = item.TransactionDate;
                voucher.VoucherType = "银行类";
                voucher.Automatic = "1";//自动
                voucher.CreateTime = DateTime.Now;
                guid = Guid.NewGuid();
                voucher.VGUID = guid;
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
                }
                if(subject != "" && subject != null)
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
                GetAssetsGeneralLedger(BVDetail, assetList, voucher, guid, 0);//将借贷数据同步中间表
                //GetOtherSubject(BVDetailList, newBankFlowList, guid, item);//通过银行渠道找流水
                GetOtherSubject2(BVDetailList, guid, item, assetList, voucher, orderListDraft, orderList, userCompanySet);//通过流水找银行渠道 
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
        public static void GetOtherSubject2(List<Business_VoucherDetail> BVDetailList, Guid guid, Business_BankFlowTemplate item, List<Business_AssetsGeneralLedger_Swap> assetList, Business_VoucherList voucher
            ,List<Business_OrderListDraft> orderListDraft, List<Business_OrderList> orderList, List<Business_UserCompanySetDetail> userCompanySet)
        {
            SqlSugarClient db = DbBusinessDataConfig.GetInstance();
            Business_VoucherDetail BVDetail = new Business_VoucherDetail();
            var bankChannel = db.Queryable<T_BankChannelMapping>().Where(x => x.IsUnable != "禁用" && x.BankAccount == item.ReceivableAccount).ToList();
            if (bankChannel.Count > 0)
            {
                var bankChannelOne = bankChannel.First();
                var subject = "";
                if (item.TurnOut == 0)
                {
                    //保险系统银行流水数据通过备注中的流水号匹配订单配置信息
                    Regex regExp = new Regex("^[0-9]*$");
                    if (item.Purpose.Length == 19 && regExp.IsMatch(item.Purpose))
                    {
                        var osno = orderListDraft.Where(x => x.OSNO == item.Purpose).ToList();
                        if (osno.Count == 1)
                        {
                            var order = orderList.Where(x => x.BusinessSubItem1 == osno[0].BusinessSubItem1).First();
                            var orderDetail = userCompanySet.Where(x => x.OrderVGUID == order.VGUID.ToString() && x.CompanyName == osno[0].OrderCompany && x.Isable == true).ToList();
                            if(orderDetail.Count > 0)
                            {
                                subject = orderDetail[0].Borrow;
                            }
                        }
                    }
                    else
                    {
                        subject = bankChannelOne.Borrow;
                    }
                }
                else
                {
                    subject = bankChannelOne.Loan;
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
                GetAssetsGeneralLedger(BVDetail, assetList, voucher, guid, 1);//将借贷数据同步中间表
            }
        }
        public static void GetAssetsGeneralLedger(Business_VoucherDetail BVDetail, List<Business_AssetsGeneralLedger_Swap> assetList, Business_VoucherList voucher, Guid guid, int i)
        {
            //凭证中间表
            Business_AssetsGeneralLedger_Swap asset = new Business_AssetsGeneralLedger_Swap();
            asset.CREATE_DATE = DateTime.Now;
            asset.SubjectVGUID = guid;
            asset.LEDGER_NAME = voucher.AccountModeName;
            asset.JE_BATCH_NAME = voucher.BatchName;
            asset.JE_BATCH_DESCRIPTION = "";
            asset.JE_HEADER_NAME = voucher.VoucherNo;
            asset.JE_HEADER_DESCRIPTION = "";
            asset.JE_SOURCE_NAME = "财务共享平台";
            asset.JE_CATEGORY_NAME = voucher.VoucherType;
            asset.ACCOUNTING_DATE = voucher.VoucherDate;
            asset.CURRENCY_CODE = "人民币";
            asset.CURRENCY_CONVERSION_TYPE = "用户";
            asset.CURRENCY_CONVERSION_DATE = DateTime.Now;
            asset.CURRENCY_CONVERSION_RATE = 1;
            asset.STATUS = "1";
            asset.VGUID = Guid.NewGuid();
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
            asset.ACCOUNTED_DR = BVDetail.BorrowMoneyCount.TryToString();
            asset.ACCOUNTED_CR = BVDetail.LoanMoneyCount.TryToString();
            assetList.Add(asset);
        }
        public static string GetVoucherName()
        {
            SqlSugarClient db = DbBusinessDataConfig.GetInstance();
            var date = DateTime.Now;
            var voucherNo = db.Ado.GetString(@"select top 1 VoucherNo from Business_VoucherList a where DATEDIFF(month,a.CreateTime,@NowDate)=0 and VoucherType='银行类'
                              order by VoucherNo desc", new { @NowDate = date });
            var batchNo = 0;
            if (voucherNo.IsValuable() && voucherNo.Length > 4)
            {
                batchNo = voucherNo.Substring(voucherNo.Length - 4, 4).TryToInt();
            }
            return DateTime.Now.ToString("yyyyMMdd") + (batchNo + 1).TryToString().PadLeft(4, '0');
        }
        public static Business_CompanyBankInfo GetSevenSubject(string bankAccount)
        {
            SqlSugarClient db = DbBusinessDataConfig.GetInstance();
            var result = db.Queryable<Business_CompanyBankInfo>().Single(x => x.BankAccount == bankAccount);
            return result;
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
                if(i == 1)
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