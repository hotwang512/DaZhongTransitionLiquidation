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
            return View();
        }
        public JsonResult GetSelectSection(string name,string companyCode,string subjectCode)
        {
            var response = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                if(name == "A")
                {
                    response = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
                }
                else
                {
                    var sVGUID = "";
                    var colname = "";
                    switch (name)
                    {
                        case "C":sVGUID = "C63BD715-C27D-4C47-AB66-550309794D43"; colname = "AccountingCode"; break;
                        case "D":sVGUID = "D63BD715-C27D-4C47-AB66-550309794D43"; colname = "CostCenterCode"; break;
                        case "E":sVGUID = "E63BD715-C27D-4C47-AB66-550309794D43"; colname = "SpareOneCode"; break;
                        case "F":sVGUID = "F63BD715-C27D-4C47-AB66-550309794D43"; colname = "SpareTwoCode"; break;
                        case "G":sVGUID = "G63BD715-C27D-4C47-AB66-550309794D43"; colname = "IntercourseCode"; break;
                        default:break;
                    }
                    response = db.SqlQueryable<Business_SevenSection>(@"select * from Business_SevenSection where SectionVGUID='"+ sVGUID + @"'
                            and Code in (select "+ colname + @" from Business_SubjectSettingInfo where SubjectVGUID='B63BD715-C27D-4C47-AB66-550309794D43'
                            and SubjectCode='"+ companyCode + "' and CompanyCode='"+ subjectCode + "')").OrderBy("Code asc").ToList();
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
                    var flowNo = db.Ado.GetString(@"select top 1 BatchName from Business_VoucherList
                                  order by BatchName desc", new { @NowDate = date });
                    var voucherNo = db.Ado.GetString(@"select top 1 VoucherNo from Business_VoucherList a where DATEDIFF(month,a.CreateTime,@NowDate)=0 
                                  order by VoucherNo desc", new { @NowDate = date });
                    var batchName = GetBatchName(voucherType, flowNo);
                    var voucherName = GetVoucherName(voucherNo);
                    //主表信息
                    Business_VoucherList voucherList = new Business_VoucherList();
                    voucherList.AccountingPeriod = voucher.AccountingPeriod;
                    voucherList.Auditor = voucher.Auditor;
                    voucherList.Bookkeeping = voucher.Bookkeeping;
                    voucherList.Cashier = voucher.Cashier;
                    voucherList.CompanyCode = "";
                    voucherList.CompanyName = voucher.CompanyName;
                    voucherList.Currency = voucher.Currency;
                    voucherList.DocumentMaker = voucher.DocumentMaker;
                    voucherList.FinanceDirector = voucher.FinanceDirector;
                    voucherList.Status = "1";
                    voucherList.VoucherDate = voucher.VoucherDate;
                    voucherList.VoucherType = voucherType;
                    voucherList.CreditAmountTotal = loanMoney;
                    voucherList.DebitAmountTotal = borrowMoney;
                    voucherList.AttachmentDetail = attachment;
                    voucherList.CreateTime = DateTime.Now;
                    var guid = voucher.VGUID;
                    if(guid == Guid.Empty)
                    {
                        guid = Guid.NewGuid();
                        voucherList.BatchName = batchName;//批名自动生成(凭证类型+日期+4位流水)
                        voucherList.VoucherNo = voucherName;//凭证号自动生成
                        voucherList.VGUID = guid;
                        db.Insertable<Business_VoucherList>(voucherList).ExecuteCommand();
                    }
                    else
                    {
                        voucherList.VGUID = voucher.VGUID;
                        db.Updateable<Business_VoucherList>(voucherList).ExecuteCommand();
                    }
                    //明细信息
                    List<Business_VoucherDetail> voucherdetailList = new List<Business_VoucherDetail>();
                    //删除现有明细数据
                    db.Deleteable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == voucher.VGUID).ExecuteCommand();
                    if(voucher.Detail != null)
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
                            BVDetail.VGUID = Guid.NewGuid();
                            BVDetail.VoucherVGUID = guid;
                            voucherdetailList.Add(BVDetail);
                        }
                        db.Insertable(voucherdetailList).ExecuteCommand();
                    }
                    if(attachment != null)
                    {
                        var attach = attachment.Split(",");
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
                    //附件信息 
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

        private string GetBatchName(string voucherType,string flowNo)
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
                var voucherDetail = db.Queryable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == vguid).ToList();
                //附件信息
                var voucherAttach = db.Queryable<Business_VoucherAttachmentList>().Where(x => x.VoucherVGUID == vguid).ToList();
                voucherList.AccountingPeriod = voucher.AccountingPeriod.TryToDate();
                voucherList.Auditor = voucher.Auditor;
                voucherList.BatchName = voucher.BatchName;//批名自动生成
                voucherList.Bookkeeping = voucher.Bookkeeping;
                voucherList.Cashier = voucher.Cashier;
                voucherList.CompanyCode = "";
                voucherList.CompanyName = voucher.CompanyName;
                voucherList.Currency = voucher.Currency;
                voucherList.DocumentMaker = voucher.DocumentMaker;
                voucherList.FinanceDirector = voucher.FinanceDirector;
                //voucherList.Status = "1";
                voucherList.VoucherDate = voucher.VoucherDate;
                voucherList.VoucherNo = voucher.VoucherNo;//凭证号自动生成
                voucherList.VoucherType = voucher.VoucherType;
                voucherList.Detail = voucherDetail;
                voucherList.Attachment = voucher.AttachmentDetail;
            });
            return Json(voucherList, JsonRequestBehavior.AllowGet); ;
        }
    }
}