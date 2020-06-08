using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using System;
using System.Linq;
using System.Web.Mvc;
using SqlSugar;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherList;
using SyntacticSugar;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using System.Data;
using Aspose.Cells;
using Aspose.Pdf;
using DaZhongTransitionLiquidation.Controllers;
using System.Collections.Generic;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail
{
    public class VoucherListDetailController : BaseController
    {
        public VoucherListDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: VoucherManageManagement/VoucherList
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            ViewBag.GetAccountMode = GetAccountModes();
            ViewBag.GetLastYearMonth = GetLastYearMonth();
            ViewBag.GetNowYearMonth = GetNowYearMonth();
            return View();
        }
        public JsonResult GetSelectSection(string name, string companyCode, string subjectCode)
        {
            var response = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                if (name == "A")
                {
                    response = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == UserInfo.AccountModeCode && x.Status == "1").OrderBy("Code asc").ToList();
                    //response = db.Queryable<Business_UserCompanySet>().Where(x => x.UserVGUID == UserInfo.Vguid.ToString() && x.Code == UserInfo.AccountModeCode
                    //            && x.Block == "1" && x.IsCheck == true).OrderBy("Code asc").ToList();
                }
                else
                {
                    var sVGUID = "";
                    var colname = "";
                    switch (name)
                    {
                        case "C": sVGUID = "C63BD715-C27D-4C47-AB66-550309794D43"; colname = "AccountingCode"; break;
                        case "D": sVGUID = "D63BD715-C27D-4C47-AB66-550309794D43"; colname = "CostCenterCode"; break;
                        case "E": sVGUID = "E63BD715-C27D-4C47-AB66-550309794D43"; colname = "SpareOneCode"; break;
                        case "F": sVGUID = "F63BD715-C27D-4C47-AB66-550309794D43"; colname = "SpareTwoCode"; break;
                        case "G": sVGUID = "G63BD715-C27D-4C47-AB66-550309794D43"; colname = "IntercourseCode"; break;
                        default: break;
                    }
                    response = db.SqlQueryable<Business_SevenSection>(@"select bss.checked,bs.Code,bs.Descrption,bs.ParentCode from Business_SevenSection bs 
 left join Business_SubjectSettingInfo bss on bs.Code=bss." + colname + " and bss.CompanyCode='" + subjectCode + @"' 
 and bss.SubjectCode='" + companyCode + "' and bss.AccountModeCode='" + UserInfo.AccountModeCode + "'  where bs.SectionVGUID='" + sVGUID + "' and bs.CompanyCode='" + companyCode + "' and bs.AccountModeCode='" + UserInfo.AccountModeCode + "' and bs.Status='1' and bs.Code is not null and bss.Checked='1'").OrderBy("Code asc").ToList();
                }

            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveVoucherListDetail(VoucherListDetail voucher)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var IsSuccess = "0";
                var guid = Guid.Empty;
                var result = db.Ado.UseTran(() =>
                {
                    var loanMoney = voucher.Detail == null ? null : voucher.Detail.Where(i => i.LoanMoney != -1).Sum(x => x.LoanMoney);//贷方总金额
                    var borrowMoney = voucher.Detail == null ? null : voucher.Detail.Where(i => i.BorrowMoney != -1).Sum(x => x.BorrowMoney);//借方总金额
                    var attachment = voucher.Attachment;
                    var voucherType = voucher.VoucherType;//凭证类型
                    var batchName = voucher.BatchName; 
                    //凭证主表
                    Business_VoucherList voucherList = new Business_VoucherList();
                    voucherList.AttachmentDetail = "";
                    string[] attach = null;
                    if (attachment != null)
                    {
                        attach = attachment.Split(",");
                        foreach (var item in attach)
                        {
                            voucherList.AttachmentDetail += item + ",";
                        }
                        voucherList.AttachmentDetail = voucherList.AttachmentDetail.Substring(0, voucherList.AttachmentDetail.Length - 1);
                    }

                    //主表信息 
                    voucherList.AccountingPeriod = voucher.AccountingPeriod;
                    voucherList.AccountModeName = voucher.AccountModeName;
                    voucherList.Auditor = voucher.Auditor;
                    voucherList.Bookkeeping = voucher.Bookkeeping;
                    voucherList.Cashier = voucher.Cashier;
                    voucherList.CompanyCode = voucher.CompanyCode;
                    voucherList.CompanyName = voucher.CompanyName;
                    voucherList.Currency = voucher.Currency;
                    voucherList.DocumentMaker = voucher.DocumentMaker;
                    voucherList.FinanceDirector = voucher.FinanceDirector;
                    voucherList.Status = "1";
                    voucherList.VoucherDate = voucher.VoucherDate;
                    voucherList.VoucherType = voucherType;
                    voucherList.CreditAmountTotal = loanMoney;
                    voucherList.DebitAmountTotal = borrowMoney;
                    voucherList.CreateTime = DateTime.Now;
                    guid = voucher.VGUID;
                    if (guid == Guid.Empty)
                    {
                        guid = Guid.NewGuid();
                        var month = "";
                        month = voucher.VoucherDate.Value.Month < 10 ? "0" + voucher.VoucherDate.Value.Month.ToString() : voucher.VoucherDate.Value.Month.ToString();
                        //if (voucher.VoucherDate.Value.Year == 2020 && voucher.VoucherDate.Value.Month == 1)
                        //{
                        //    //上个版本的错误,造成1月凭证生成格式有误,暂时如此,2月开始格式正常
                        //    month = voucher.VoucherDate.Value.Month < 10 ? voucher.VoucherDate.Value.Month.ToString() : voucher.VoucherDate.Value.Month.ToString();
                        //}
                        var bank = "B" + UserInfo.AccountModeCode + UserInfo.CompanyCode + voucher.VoucherDate.Value.Year.ToString() + month;
                        if (voucherType == "现金类")
                        {
                            bank = "M" + UserInfo.AccountModeCode + UserInfo.CompanyCode + voucher.VoucherDate.Value.Year.ToString() + month;
                        }
                        if (voucherType == "转账类")
                        {
                            bank = "Z" + UserInfo.AccountModeCode + UserInfo.CompanyCode + voucher.VoucherDate.Value.Year.ToString() + month;
                        }
                        var no = CreateNo.GetCreateNo(db, bank);
                        voucherList.VoucherNo = UserInfo.AccountModeCode + UserInfo.CompanyCode + voucherType + no;
                        voucherList.BatchName = voucherList.VoucherNo;
                        voucherList.VGUID = guid;
                        voucherList.Automatic = "0";//手动
                        db.Insertable(voucherList).ExecuteCommand();
                    }
                    else
                    {
                        voucherList.VGUID = voucher.VGUID;
                        db.Updateable(voucherList).IgnoreColumns(it => new { it.BatchName, it.VoucherNo, it.Automatic, it.TradingBank, it.ReceivingUnit, it.TransactionDate, it.Batch }).ExecuteCommand();
                    }
                    //科目信息
                    List<Business_VoucherDetail> voucherdetailList = new List<Business_VoucherDetail>();
                    //凭证中间表List
                    List<AssetsGeneralLedger_Swap> assetList = new List<AssetsGeneralLedger_Swap>();
                    //删除现有明细数据
                    var receivableAccount = "";
                    var Account = db.Queryable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == voucher.VGUID).OrderBy("ReceivableAccount desc").ToList();
                    if (Account.Count > 0)
                    {
                        receivableAccount = Account[0].ReceivableAccount;
                    }
                    db.Deleteable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == voucher.VGUID).ExecuteCommand();

                    if (voucher.Detail != null)
                    {
                        foreach (var item in voucher.Detail)
                        {
                            Business_VoucherDetail BVDetail = new Business_VoucherDetail();
                            BVDetail.Abstract = item.Abstract;
                            BVDetail.AccountSection = item.AccountSection;
                            BVDetail.BorrowMoney = item.BorrowMoney;
                            BVDetail.CompanySection = item.CompanySection;
                            BVDetail.CostCenterSection = item.CostCenterSection;
                            BVDetail.IntercourseSection = item.IntercourseSection;
                            BVDetail.LoanMoney = item.LoanMoney;
                            BVDetail.SpareOneSection = item.SpareOneSection;
                            BVDetail.SpareTwoSection = item.SpareTwoSection;
                            BVDetail.SubjectSection = item.SubjectSection;
                            BVDetail.SubjectSectionName = item.SubjectSectionName;
                            BVDetail.SevenSubjectName = item.SevenSubjectName;
                            BVDetail.BorrowMoneyCount = borrowMoney;
                            BVDetail.LoanMoneyCount = loanMoney;
                            BVDetail.JE_LINE_NUMBER = item.JE_LINE_NUMBER;
                            BVDetail.VGUID = Guid.NewGuid();
                            BVDetail.VoucherVGUID = guid;
                            if (item.LoanMoney != null && item.LoanMoney != 0)
                            {
                                BVDetail.ReceivableAccount = receivableAccount;
                            }
                            voucherdetailList.Add(BVDetail);
                        }
                        db.Insertable(voucherdetailList).ExecuteCommand();
                    }
                    if (attachment != null)
                    {
                        List<Business_VoucherAttachmentList> BVAttachList = new List<Business_VoucherAttachmentList>();
                        //删除现有附件数据
                        db.Deleteable<Business_VoucherAttachmentList>().Where(x => x.VoucherVGUID == voucher.VGUID).ExecuteCommand();
                        foreach (var it in attach)
                        {
                            Business_VoucherAttachmentList BVAttach = new Business_VoucherAttachmentList();
                            var att = it.Split("&");
                            if (att[1] != null)
                            {
                                BVAttach.Attachment = att[1];
                                BVAttach.AttachmentType = att[0];
                                BVAttach.CreateTime = DateTime.Now;
                                BVAttach.VGUID = Guid.NewGuid();
                                BVAttach.VoucherVGUID = guid;
                            }
                            BVAttachList.Add(BVAttach);
                        }
                        db.Insertable(BVAttachList).ExecuteCommand();
                    }

                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = guid.TryToString();
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                if (IsSuccess == "2")
                {
                    resultModel.Status = IsSuccess;
                }
            });
            return Json(resultModel);
        }
        private string GetBatchName(string voucherType, string flowNo)
        {
            var batchNo = 0;
            if (flowNo.IsValuable() && flowNo.Length > 4)
            {
                batchNo = flowNo.Substring(flowNo.Length - 4, 4).TryToInt();
            }
            return voucherType + DateTime.Now.ToString("yyyyMMdd") + (batchNo + 1).TryToString().PadLeft(4, '0');
        }
        public static string GetVoucherName(string voucherNo)
        {
            var batchNo = 0;
            if (voucherNo.IsValuable() && voucherNo.Length > 4)
            {
                batchNo = voucherNo.Substring(voucherNo.Length - 4, 4).TryToInt();
            }
            return DateTime.Now.ToString("yyyyMM") + (batchNo + 1).TryToString().PadLeft(4, '0');
        }
        public JsonResult GetVoucherDetail(Guid vguid)
        {
            var voucherList = new VoucherListDetail();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                var voucher = db.Queryable<Business_VoucherList>().Single(x => x.VGUID == vguid);
                if (voucher.FinanceDirector == "" || voucher.FinanceDirector == null)
                {
                    var userData = new List<Sys_User>();
                    DbService.Command(_db =>
                    {
                        userData = _db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role,a.RoleStation from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
                    });
                    foreach (var user in userData)
                    {
                        switch (user.RoleStation)
                        {
                            case "财务经理": voucher.FinanceDirector = user.LoginName; break;
                            case "财务主管": voucher.Bookkeeping = user.LoginName; break;
                            //case "审核岗": voucher.Auditor = user.LoginName; break;
                            case "出纳": voucher.Cashier = user.LoginName; break;
                            default: break;
                        }
                    }
                    //voucher.DocumentMaker = UserInfo.LoginName;
                    db.Updateable(voucher).ExecuteCommand();
                }
                //明细信息
                var voucherDetail = db.Queryable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == vguid).OrderBy("JE_LINE_NUMBER asc,BorrowMoney desc").ToList();
                //附件信息
                //var voucherAttach = db.Queryable<Business_VoucherAttachmentList>().Where(x => x.VoucherVGUID == vguid).ToList();
                voucherList.AccountingPeriod = voucher.AccountingPeriod.TryToDate();
                voucherList.Auditor = voucher.Auditor;
                voucherList.BatchName = voucher.BatchName;//批名自动生成
                voucherList.Bookkeeping = voucher.Bookkeeping;
                voucherList.Cashier = voucher.Cashier;
                voucherList.CompanyCode = voucher.CompanyCode;
                voucherList.CompanyName = voucher.CompanyName;
                voucherList.AccountModeName = voucher.AccountModeName;
                voucherList.Currency = voucher.Currency;
                voucherList.DocumentMaker = voucher.DocumentMaker;
                voucherList.FinanceDirector = voucher.FinanceDirector;
                voucherList.Status = voucher.Status;
                voucherList.VoucherDate = voucher.VoucherDate;
                voucherList.VoucherNo = voucher.VoucherNo;//凭证号自动生成
                voucherList.VoucherType = voucher.VoucherType;
                voucherList.Detail = voucherDetail;
                voucherList.Attachment = voucher.AttachmentDetail;
                voucherList.VGUID = voucher.VGUID;
                voucherList.Automatic = voucher.Automatic;
            });
            return Json(voucherList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult UploadImg(string ImageBase64Str)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            int len = ImageBase64Str.IndexOf("base64,") + 7;
            int len1 = ImageBase64Str.IndexOf("data:") + 5;
            string ext = ImageBase64Str.Substring(len1, len - len1 - 8);
            var uploadPath = ConfigSugar.GetAppString("UploadPath") + "\\" + "AcceptFile\\" +
                DateTime.Now.ToString("yyyyMMddHHmmssfff.") +
                (ext.ToLower().Contains("png") ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg);
            var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath;
            var savePath = FileHelper.Base64ToImg(filePath, ImageBase64Str);
            try
            {
                resultModel.IsSuccess = true;
                resultModel.ResultInfo = "\\" + uploadPath;//路径
                resultModel.ResultInfo2 = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);//名称
                resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", filePath, ex.ToString()));
            }
            return Json(resultModel);
        }
        public string GetAccountModes()
        {
            var result = UserInfo.AccountModeName;
            return result;
        }
        public JsonResult GetPersonInfo()
        {
            var result = new List<Sys_User>();
            DbService.Command(db =>
            {
                result = db.SqlQueryable<Sys_User>(@"select a.LoginName,b.Role from Sys_User as a left join Sys_Role as b on a.Role = b.Vguid").ToList();
            });
            return Json(result, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult GetSettingData(Guid vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var voucherData = db.Queryable<Business_VoucherList>().Where(x => x.VGUID == vguids).ToList().FirstOrDefault();
                var voucherDetailData = db.Queryable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == vguids).ToList();
                var bankFlowData = db.Queryable<Business_BankFlowTemplate>().Where(x => x.Batch == voucherData.Batch).ToList().FirstOrDefault();
                var receivableAccount = bankFlowData.ReceivableAccount;
                var paySettingData = db.Queryable<Business_PaySetting>().Where(x => x.BankAccount == receivableAccount).ToList();
                if (paySettingData.Count == 1)
                {
                    //账号下存在收款业务配置
                    var payGuid = paySettingData[0].VGUID.TryToString();
                    var borrowLoadData = db.Queryable<Business_PaySettingDetail>().Where(x => x.PayVGUID == payGuid && x.AccountModeCode == bankFlowData.AccountModeCode && x.CompanyCode == bankFlowData.CompanyCode).OrderBy("Borrow desc").ToList();
                    if(voucherDetailData.Count != borrowLoadData.Count)
                    {
                        GetSubjectSet(db, vguids, borrowLoadData, bankFlowData, voucherData, voucherDetailData);
                        AutoSyncBankFlow.DoGetVoucherMoney(vguids);
                    }
                    else
                    {
                        AutoSyncBankFlow.DoGetVoucherMoney(vguids);
                    }
                    resultModel.Status = "1";
                }
                else
                {
                    resultModel.Status = "2";
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        private void GetSubjectSet(SqlSugarClient db, Guid vguids, List<Business_PaySettingDetail> borrowLoadData, Business_BankFlowTemplate item, Business_VoucherList voucherData,List<Business_VoucherDetail> voucherDetailData)
        {
            var subject = "";
            List<Business_VoucherDetail> BVDetailList = new List<Business_VoucherDetail>();
            foreach (var it in borrowLoadData)
            {
                Business_VoucherDetail BVDetail2 = new Business_VoucherDetail();
                //收款业务
                if(it.Loan == null && it.Borrow != null)
                {
                    subject = it.Borrow;
                    BVDetail2.BorrowMoney = 0;
                    BVDetail2.ReceivableAccount = null;
                    if (it.TransferType != "手续费")
                    {
                        BVDetail2.BorrowMoney = item.TurnOut;
                    }
                }
                else if(it.Loan != null && it.Borrow == null)
                {
                    subject = it.Loan;
                    BVDetail2.LoanMoney = 0;
                    BVDetail2.ReceivableAccount = item.ReceivableAccount;//对方账号,用于轮循贷方明细找到对应金额
                }
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
                    BVDetail2.SevenSubjectName = subject + "\n" + BankFlowTemplateController.GetSevenSubjectName(subject, item.AccountModeCode, item.CompanyCode);
                }
                if (borrowLoadData.Count == 2)
                {
                    //一借一贷,借贷相平
                    BVDetail2.LoanMoney = item.TurnOut;
                    BVDetail2.LoanMoneyCount = item.TurnOut;
                    voucherData.CreditAmountTotal = item.TurnOut;
                    voucherData.DebitAmountTotal = item.TurnOut;
                }
                BVDetail2.Abstract = it.Remark;
                BVDetail2.VGUID = Guid.NewGuid();
                BVDetail2.VoucherVGUID = vguids;
                BVDetailList.Add(BVDetail2);
            }
            var result = db.Ado.UseTran(() =>
            {
                voucherData.CreditAmountTotal = 0;
                voucherData.DebitAmountTotal = 0;
                db.Deleteable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == vguids).ExecuteCommand();
                db.Updateable(voucherData).ExecuteCommand();
                db.Insertable(BVDetailList).ExecuteCommand();
            });
        }
        public JsonResult PrintVoucherList(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                List<string> pdfPathList = new List<string>();
                foreach (var item in vguids)
                {
                    DataTable dt = new DataTable();
                    var data = db.Ado.SqlQuery<PrintVoucherList>(@"select CONVERT(varchar(100), a.AccountingPeriod, 23) as AccountingPeriod,CONVERT(varchar(100), a.VoucherDate, 23) as VoucherDate,a.BatchName,a.VoucherNo,a.CompanyName,
                            a.FinanceDirector,a.Bookkeeping,a.Auditor,a.DocumentMaker,a.Cashier,a.Status  from 
                            Business_VoucherList as a where a.VGUID = @VGUID ", new { VGUID = item }).ToList().FirstOrDefault();
                    if(data == null)
                    {
                        resultModel.Status = "3";
                        resultModel.ResultInfo = "找不到数据,请先保存";
                        return;
                    }
                    var pdfPath = "/Temp/NewVoucherReport" + data.VoucherNo + ".pdf";
                    var isAny = System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(pdfPath));
                    if (isAny && data.Status == "3")
                    {
                        //存在已生成的PDF文件直接添加进打印集合
                        pdfPathList.Add(pdfPath);
                        continue;
                    }
                    var dataDetail = db.Ado.SqlQuery<PrintVoucherDetail>(@"select b.Abstract,b.SevenSubjectName,convert(varchar(1000),cast(b.BorrowMoney as money),1) as BorrowMoney,
                            convert(varchar(1000),cast(b.LoanMoney as money),1) as LoanMoney,convert(varchar(1000),cast(b.BorrowMoneyCount as money),1) as BorrowMoneyCount,
                            convert(varchar(1000),cast(b.LoanMoneyCount as money),1) as LoanMoneyCount,b.JE_LINE_NUMBER from 
                            Business_VoucherDetail  as b where b.VoucherVGUID = @VGUID ", new { VGUID = item }).OrderBy(x => x.JE_LINE_NUMBER).ToList();
                    var attachmentCount = db.Queryable<Business_VoucherAttachmentList>().Where(x => x.VoucherVGUID == item).ToList().Count;
                    for (int i = 0; i < dataDetail.Count; i++)
                    {
                        if (dataDetail[i].BorrowMoney == null)
                        {
                            dataDetail[i].BorrowMoney = "";
                        }
                        if (dataDetail[i].LoanMoney == null)
                        {
                            dataDetail[i].LoanMoney = "";
                        }
                        if (dataDetail[i].BorrowMoneyCount == null)
                        {
                            dataDetail[i].BorrowMoneyCount = "";
                        }
                        if (dataDetail[i].LoanMoneyCount == null)
                        {
                            dataDetail[i].LoanMoneyCount = "";
                        }
                    }
                    
                    if (dataDetail.Count <= 7)
                    {
                        //打印一张
                        PrintExcel(dt, data, dataDetail, attachmentCount, pdfPathList);
                        resultModel.Status = "1";
                    }
                    else
                    {
                        //打印多张
                        PrintExcelMore(dt, data, dataDetail, attachmentCount, pdfPathList);
                        resultModel.Status = "2";
                    }
                }
                //合并PDF
                resultModel.ResultInfo = MergePdf(pdfPathList);
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet); ;
        }
        private void PrintExcelMore(DataTable dt, PrintVoucherList data, List<PrintVoucherDetail> dataDetail, int attachmentCount, List<string> pdfPathList)
        {
            string rootPath = System.Web.HttpContext.Current.Server.MapPath("/Template/财务打印样式模板.xlsx");
            string url = System.Web.HttpContext.Current.Server.MapPath("/Temp");
            var pageCount = (dataDetail.Count / 7).TryToInt();//总张数
            var lastPage = dataDetail.Count % 7;//最后一张的条数
            if (lastPage != 0)
            {
                pageCount = pageCount + 1;
            }
            var borrowCount = dataDetail.Sum(x => x.BorrowMoney.TryToDecimal());
            var loanCount = dataDetail.Sum(x => x.LoanMoney.TryToDecimal());
            for (int i = 1; i <= pageCount; i++)
            {
                var excelPath = "/Temp/VoucherReport" + data.VoucherNo + "-" + i + ".xlsx";
                var pdfPath = "/Temp/NewVoucherReport" + data.VoucherNo + "-" + i + ".pdf";
                string fileName = "VoucherReport" + data.VoucherNo + "-" + i + ".xlsx";
                string filePath = System.IO.Path.Combine(url, fileName);
                Workbook wk = new Workbook(rootPath);
                Worksheet sheet = wk.Worksheets[0]; //工作表
                Aspose.Cells.Cells cells = sheet.Cells;//单元格
                dt = dataDetail.Skip((i - 1) * 7).Take(7).ToList().TryToDataTable();
                dt.TableName = "VoucherReport";
                if (i == pageCount)
                {
                    //循环至最后一张,补全7行
                    var rowCount = dt.Rows.Count;
                    for (int j = 0; j < 7 - rowCount; j++)
                    {
                        dt.AddRow();
                    }
                    cells[6, 15].PutValue(borrowCount);
                    cells[6, 18].PutValue(loanCount);
                }
                cells[0, 0].PutValue(data.CompanyName);
                cells[1, 4].PutValue(data.AccountingPeriod);
                cells[1, 15].PutValue(data.BatchName);
                cells[2, 15].PutValue(data.VoucherNo + "-" + i);
                cells[3, 10].PutValue(data.VoucherDate);
                cells[3, 19].PutValue(attachmentCount);
                cells[7, 2].PutValue(data.FinanceDirector);
                cells[7, 6].PutValue(data.Bookkeeping);
                cells[7, 10].PutValue(data.Auditor);
                cells[7, 15].PutValue(data.DocumentMaker);
                cells[7, 19].PutValue(data.Cashier);
                WorkbookDesigner designer = new WorkbookDesigner(wk);
                designer.SetDataSource(dt);
                designer.Process();
                designer.Workbook.Save(filePath);
                Workbook wb = new Workbook(System.Web.HttpContext.Current.Server.MapPath(excelPath));
                wb.Save(System.Web.HttpContext.Current.Server.MapPath(pdfPath), Aspose.Cells.SaveFormat.Pdf);
                pdfPathList.Add(pdfPath);
            }
        }
        private void PrintExcel(DataTable dt, PrintVoucherList data, List<PrintVoucherDetail> dataDetail, int attachmentCount, List<string> pdfPathList)
        {
            var excelPath = "/Temp/VoucherReport" + data.VoucherNo + ".xlsx";
            var pdfPath = "/Temp/NewVoucherReport" + data.VoucherNo + ".pdf";
            dt = dataDetail.TryToDataTable();
            dt.TableName = "VoucherReport";
            if (dataDetail.Count != 7)
            {
                for (int i = 0; i < 7 - dataDetail.Count; i++)
                {
                    dt.AddRow();
                }
            }
            var borrowCount = dataDetail.Sum(x => x.BorrowMoney.TryToDecimal());
            var loanCount = dataDetail.Sum(x => x.LoanMoney.TryToDecimal());
            string rootPath = System.Web.HttpContext.Current.Server.MapPath("/Template/财务打印样式模板.xlsx");
            string url = System.Web.HttpContext.Current.Server.MapPath("/Temp");
            string fileName = "VoucherReport" + data.VoucherNo + ".xlsx";
            string filePath = System.IO.Path.Combine(url, fileName);
            Workbook wk = new Workbook(rootPath);
            Worksheet sheet = wk.Worksheets[0]; //工作表
            Aspose.Cells.Cells cells = sheet.Cells;//单元格
            cells[0, 0].PutValue(data.CompanyName);
            cells[1, 4].PutValue(data.AccountingPeriod);
            cells[1, 15].PutValue(data.BatchName);
            cells[2, 15].PutValue(data.VoucherNo);
            cells[3, 10].PutValue(data.VoucherDate);
            cells[3, 19].PutValue(attachmentCount);
            cells[7, 2].PutValue(data.FinanceDirector);
            cells[7, 6].PutValue(data.Bookkeeping);
            cells[7, 10].PutValue(data.Auditor);
            cells[7, 15].PutValue(data.DocumentMaker);
            cells[7, 19].PutValue(data.Cashier);
            cells[6, 15].PutValue(borrowCount);
            cells[6, 18].PutValue(loanCount);
            WorkbookDesigner designer = new WorkbookDesigner(wk);
            designer.SetDataSource(dt);
            designer.Process();
            designer.Workbook.Save(filePath);
            Workbook wb = new Workbook(System.Web.HttpContext.Current.Server.MapPath(excelPath));
            wb.Save(System.Web.HttpContext.Current.Server.MapPath(pdfPath), Aspose.Cells.SaveFormat.Pdf);
            pdfPathList.Add(pdfPath);
        }
        public bool DeleteFile(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string MergePdf(List<string> pdfPathList)
        {
            Document pdf = new Document();
            for (int i = 0; i < pdfPathList.Count; i++)
            {
                var path = pdfPathList[i];
                Document pdf2 = new Document(System.Web.HttpContext.Current.Server.MapPath(path));
                pdf.Pages.Add(pdf2.Pages);
            }
            var guid = Guid.NewGuid();
            var lastPath = "/Temp/LastVoucherReport" + guid + ".pdf";
            pdf.Save(System.Web.HttpContext.Current.Server.MapPath(lastPath));
            return lastPath;
        }
        public string GetLastYearMonth()
        {
            //开始日期为去年12月
            var lastDate = DateTime.Now.AddYears(-1).Year.ToString() + "-12-01 00:00:00";
            var startMonth = lastDate;
            return startMonth;
        }
        public string GetNowYearMonth()
        {
            //结束日期为今年当月
            DateTime s = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
            DateTime ss = s.AddDays(1 - s.Day);
            var endMonth = ss.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
            return endMonth;
        }
    }
}