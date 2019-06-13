using DaZhongTransitionLiquidation.Infrastructure.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;

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
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.OrderListVguid);
            return View();
        }
        public JsonResult GetVoucherListDatas(Business_VoucherList searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_VoucherList>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_VoucherList>()
                .Where(i => i.Status == searchParams.Status)
                .Where(i => i.Automatic == searchParams.Automatic)
                .WhereIF(searchParams.VoucherType != null, i => i.VoucherType == searchParams.VoucherType)
                .WhereIF(searchParams.AccountingPeriod != null, i => i.AccountingPeriod == searchParams.AccountingPeriod)
                .OrderBy("VoucherDate desc,VoucherNo desc").ToPageList(para.pagenum, para.pagesize, ref pageCount);
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
                    db.Deleteable<AssetsGeneralLedger_Swap>(x => x.SubjectVGUID == item).ExecuteCommand(); 
                    //删除附件信息
                    db.Deleteable<Business_VoucherAttachmentList>(x => x.VoucherVGUID == item).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == 1;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult UpdataVoucherListInfo(List<Guid> vguids, string status)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var i = 1;
                int saveChanges = 1;
                foreach (var item in vguids)
                {
                    
                    var voucher = db.Queryable<Business_VoucherDetail>().Where(it => it.VoucherVGUID == item).ToList();
                    var loanMoney = voucher == null ? null : voucher.Sum(x => x.LoanMoney);//贷方总金额
                    var borrowMoney = voucher == null ? null : voucher.Sum(x => x.BorrowMoney);//借方总金额
                    if(loanMoney == borrowMoney)
                    {
                        //更新主表信息
                        saveChanges = db.Updateable<Business_VoucherList>().UpdateColumns(it => new Business_VoucherList()
                        {
                            Status = status,
                        }).Where(it => it.VGUID == item).ExecuteCommand();

                    }
                    else
                    {
                        var j = i++;
                        resultModel.Status = "2";
                        resultModel.ResultInfo = j.ToString();
                        continue;
                    } 
                }
                if(resultModel.Status != "2")
                {
                    resultModel.IsSuccess = saveChanges == 1;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }               
            });
            return Json(resultModel);
        }
    }
}