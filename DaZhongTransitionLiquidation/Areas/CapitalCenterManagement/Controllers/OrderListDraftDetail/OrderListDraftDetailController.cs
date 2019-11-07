using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Models;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderListDraftDetail
{
    public class OrderListDraftDetailController : BaseController
    {
        public OrderListDraftDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/OrderListDraftDetail
        public ActionResult Index(string vguid)
        {
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.VGUID = vguid;
            ViewBag.PayAccount = GetCompanyBankInfo();
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult SaveOrderListDetail(Business_OrderListDraft sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    //var companyCode = sevenSection.CompanySection;
                    //sevenSection.CompanyName = db.Queryable<Business_SevenSection>().Single(x => x.Code == companyCode && x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").Descrption;
                    if (sevenSection.VGUID == Guid.Empty)
                    {
                        sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.CreateTime = DateTime.Now;
                        db.Insertable<Business_OrderListDraft>(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        db.Updateable<Business_OrderListDraft>(sevenSection).ExecuteCommand();
                    }
                    var attachment = sevenSection.Attachment;
                    if (attachment != null)
                    {
                        var attach = attachment.Split(",");
                        List<Business_VoucherAttachmentList> BVAttachList = new List<Business_VoucherAttachmentList>();
                        //删除现有附件数据
                        db.Deleteable<Business_VoucherAttachmentList>().Where(x => x.VoucherVGUID == sevenSection.VGUID).ExecuteCommand();
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
                                BVAttach.VoucherVGUID = sevenSection.VGUID;
                            }
                            BVAttachList.Add(BVAttach);
                        }
                        db.Insertable(BVAttachList).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult GetOrderListDetail(Guid vguid)
        {
            Business_OrderListDraft orderList = new Business_OrderListDraft();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_OrderListDraft>().Single(x => x.VGUID == vguid);
                if(orderList.CollectBankAccountName.TryToGuid() != Guid.Empty)
                {
                    orderList.CollectBankAccountName = db.Queryable<Business_CustomerBankInfo>().Single(x => x.VGUID == orderList.CollectBankAccountName.TryToGuid()).BankAccountName;
                }
                
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult SaveAttachment(Business_OrderListDraft sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    //var companyCode = sevenSection.CompanySection;
                    //sevenSection.CompanyName = db.Queryable<Business_SevenSection>().Single(x => x.Code == companyCode && x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").Descrption;
                    if (sevenSection.VGUID == Guid.Empty)
                    {
                        //sevenSection.VGUID = Guid.NewGuid();
                        //sevenSection.CreateTime = DateTime.Now;
                        //db.Insertable<Business_OrderListDraft>(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        db.Updateable<Business_OrderListDraft>().UpdateColumns(it => new Business_OrderListDraft()
                        {
                            Attachment = sevenSection.Attachment,
                            InvoiceNumber = sevenSection.InvoiceNumber,
                            AttachmentNumber = sevenSection.AttachmentNumber,
                        }).Where(it => it.VGUID == sevenSection.VGUID).ExecuteCommand();
                    }
                    var attachment = sevenSection.Attachment;
                    if (attachment != null)
                    {
                        var attach = attachment.Split(",");
                        List<Business_VoucherAttachmentList> BVAttachList = new List<Business_VoucherAttachmentList>();
                        //删除现有附件数据
                        db.Deleteable<Business_VoucherAttachmentList>().Where(x => x.VoucherVGUID == sevenSection.VGUID).ExecuteCommand();
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
                                BVAttach.VoucherVGUID = sevenSection.VGUID;
                            }
                            BVAttachList.Add(BVAttach);
                        }
                        db.Insertable(BVAttachList).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        public JsonResult GetAttachmentInfo(string PaymentVGUID)//Guid[] vguids
        {
            List<Business_VoucherAttachmentList> VAList = new List<Business_VoucherAttachmentList>();
            DbBusinessDataService.Command(db =>
            {
                var VGUID = PaymentVGUID.TryToGuid();
                VAList = db.Queryable<Business_VoucherAttachmentList>().Where(x => x.VoucherVGUID == VGUID).ToList();
            });
            return Json(VAList, JsonRequestBehavior.AllowGet);
        }
        public List<Business_CompanyBankInfo> GetCompanyBankInfo()
        {
            var result = new List<Business_CompanyBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                var loginCompany = cache[PubGet.GetUserKey].CompanyCode;
                result = db.Queryable<Business_CompanyBankInfo>().Where(x => x.CompanyCode == loginCompany).OrderBy("BankStatus desc").ToList();
            });
            return result;
        }
        public JsonResult GetBankInfo(string PayBank)
        {
            var result = new Business_CompanyBankInfo();
            DbBusinessDataService.Command(db =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                var loginCompany = cache[PubGet.GetUserKey].CompanyCode;
                result = db.Queryable<Business_CompanyBankInfo>().Where(x => x.CompanyCode == loginCompany && x.BankName == PayBank).First();
            });
            return Json(result, JsonRequestBehavior.AllowGet); ;
        }
    }
}