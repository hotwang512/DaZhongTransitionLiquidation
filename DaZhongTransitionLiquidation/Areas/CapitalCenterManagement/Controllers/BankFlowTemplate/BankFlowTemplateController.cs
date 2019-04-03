using Aspose.Cells;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.BankData;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
                    }
                    //同步银行流水到银行数据表
                    BankDataPack.SyncBackFlow(bankFlowList[0].TransactionDate);
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
                    }
                    //同步银行流水到银行数据表
                    BankDataPack.SyncBackFlow(bankFlowList[0].TransactionDate);
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
                        items.CreatePerson = "sysAdmin";
                        newBankFlowList.Add(items);
                    }
                    if (newBankFlowList.Count > 0)
                    {
                        db.Insertable<Business_BankFlowTemplate>(newBankFlowList).ExecuteCommand();
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
                    }
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
    }
}