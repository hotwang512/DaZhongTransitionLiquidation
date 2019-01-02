using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Models;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.ReimbursementCenter.Controllers
{
    public class ROrderListDraftDetailController : BaseController
    {
        public ROrderListDraftDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: ReimbursementCenter/ROrderListDraftDetail
        public ActionResult Index()
        {
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
                    var attachment = sevenSection.Attachment;
                    var attach = attachment.Split(",");
                    sevenSection.Attachment = "";
                    foreach (var item in attach)
                    {
                        sevenSection.Attachment += item + ",";
                    }
                    sevenSection.Attachment = sevenSection.Attachment.Substring(0, sevenSection.Attachment.Length - 1);
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
                    if (attachment != null)
                    {                      
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
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
    }
}