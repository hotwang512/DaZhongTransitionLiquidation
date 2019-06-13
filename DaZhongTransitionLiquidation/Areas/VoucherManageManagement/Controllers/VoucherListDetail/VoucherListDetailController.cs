using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqlSugar;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherList;
using SyntacticSugar;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;

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
                var result = db.Ado.UseTran(() =>
                {
                    var loanMoney = voucher.Detail == null ? null : voucher.Detail.Where(i => i.LoanMoney != -1).Sum(x => x.LoanMoney);//贷方总金额
                    var borrowMoney = voucher.Detail == null ? null : voucher.Detail.Where(i => i.BorrowMoney != -1).Sum(x => x.BorrowMoney);//借方总金额
                    var attachment = voucher.Attachment;
                    var voucherType = voucher.VoucherType;//凭证类型
                    var date = DateTime.Now;
                    //var flowNo = db.Ado.GetString(@"select top 1 BatchName from Business_VoucherList
                    //              order by BatchName desc", new { @NowDate = date });
                    var voucherNo = db.Ado.GetString(@"select top 1 VoucherNo from Business_VoucherList a where DATEDIFF(month,a.CreateTime,@NowDate)=0 
                                  order by VoucherNo desc", new { @NowDate = date });
                    var batchName = voucher.BatchName; //GetBatchName(voucherType, flowNo);
                    var voucherName = GetVoucherName(voucherNo);
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
                    var guid = voucher.VGUID;
                    if (guid == Guid.Empty)
                    {
                        guid = Guid.NewGuid();
                        voucherList.BatchName = batchName;//批名自动生成(凭证类型+日期)
                        voucherList.VoucherNo = voucherName;//凭证号自动生成
                        voucherList.VGUID = guid;
                        voucherList.Automatic = "0";//手动
                        db.Insertable<Business_VoucherList>(voucherList).ExecuteCommand();
                    }
                    else
                    {
                        voucherList.VGUID = voucher.VGUID;
                        db.Updateable<Business_VoucherList>(voucherList).IgnoreColumns(it => new { it.BatchName, it.VoucherNo }).ExecuteCommand();
                    }
                   
                    //科目信息
                    List<Business_VoucherDetail> voucherdetailList = new List<Business_VoucherDetail>();
                    //凭证中间表List
                    List<AssetsGeneralLedger_Swap> assetList = new List<AssetsGeneralLedger_Swap>();
                    
                    //删除现有明细数据
                    db.Deleteable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == voucher.VGUID).ExecuteCommand();
                    //删除现有中间表数据
                    db.Deleteable<AssetsGeneralLedger_Swap>().Where(x => x.LINE_ID == voucher.VGUID).ExecuteCommand();
                    if (voucher.Detail != null)
                    {
                        var i = 0;
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
                            BVDetail.JE_LINE_NUMBER = i++;
                            BVDetail.VGUID = Guid.NewGuid();
                            BVDetail.VoucherVGUID = guid;
                            voucherdetailList.Add(BVDetail);
                            //凭证中间表
                            var type = "";
                            switch (voucher.VoucherType)
                            {
                                case "现金": type = "x.现金"; break;
                                case "银行": type = "y.银行"; break;
                                case "转账": type = "z.转账"; break;
                                default: break;
                            }
                            AssetsGeneralLedger_Swap asset = new AssetsGeneralLedger_Swap();
                            asset.CREATE_DATE = DateTime.Now;
                            //asset.SubjectVGUID = guid;
                            asset.LINE_ID = guid;
                            asset.LEDGER_NAME = voucher.AccountModeName;
                            asset.JE_BATCH_NAME = batchName;
                            asset.JE_BATCH_DESCRIPTION = "";
                            asset.JE_HEADER_NAME = voucherName;
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
                            asset.TRASACTION_ID = Guid.NewGuid();
                            asset.JE_LINE_NUMBER = BVDetail.JE_LINE_NUMBER;
                            asset.SEGMENT1 = item.CompanySection;
                            asset.SEGMENT2 = item.SubjectSection;
                            asset.SEGMENT3 = item.AccountSection;
                            asset.SEGMENT4 = item.CostCenterSection;
                            asset.SEGMENT5 = item.SpareOneSection;
                            asset.SEGMENT6 = item.SpareTwoSection;
                            asset.SEGMENT7 = item.IntercourseSection;
                            asset.ENTERED_CR = item.LoanMoney.TryToString();
                            asset.ENTERED_DR = item.BorrowMoney.TryToString();
                            asset.ACCOUNTED_DR = item.BorrowMoney.TryToString();
                            asset.ACCOUNTED_CR = item.LoanMoney.TryToString();
                            assetList.Add(asset);
                        }
                        db.Insertable(voucherdetailList).ExecuteCommand();
                        //同步至中间表
                        db.Insertable(assetList).ExecuteCommand();
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
                resultModel.ResultInfo = result.ErrorMessage;
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
        private string GetVoucherName(string voucherNo)
        {
            var batchNo = 0;
            if (voucherNo.IsValuable() && voucherNo.Length > 4)
            {
                batchNo = voucherNo.Substring(voucherNo.Length - 4, 4).TryToInt();
            }
            return DateTime.Now.ToString("yyyyMMdd") + (batchNo + 1).TryToString().PadLeft(4, '0');
        }
        public JsonResult GetVoucherDetail(Guid vguid)
        {
            var voucherList = new VoucherListDetail();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                var voucher = db.Queryable<Business_VoucherList>().Single(x => x.VGUID == vguid);
                //明细信息
                var voucherDetail = db.Queryable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == vguid).OrderBy("JE_LINE_NUMBER asc").ToList();
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
                resultModel.ResultInfo = "\\"+ uploadPath;//路径
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
    }
}