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
using SyntacticSugar;

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
                        //审核成功写入中间表
                        if(status == "3")
                        {
                            InsertAssetsGeneralLedger(item, db);
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
                if(resultModel.Status != "2")
                {
                    resultModel.IsSuccess = saveChanges == 1;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }               
            });
            return Json(resultModel);
        }

        private void InsertAssetsGeneralLedger(Guid item, SqlSugarClient db)
        {
            //删除现有中间表数据
            db.Deleteable<AssetsGeneralLedger_Swap>().Where(x => x.LINE_ID == item.TryToString()).ExecuteCommand();
            //凭证中间表
            var voucher = db.Queryable<Business_VoucherList>().Where(x => x.VGUID == item).First();
            var voucherDetail = db.Queryable<Business_VoucherDetail>().Where(x => x.VoucherVGUID == item).ToList();
            var type = "";
            switch (voucher.VoucherType)
            {
                case "现金类": type = "x.现金"; break;
                case "银行类": type = "y.银行"; break;
                case "转账类": type = "z.转账"; break;
                default: break;
            }
            //asset.VGUID = Guid.NewGuid();
            foreach (var items in voucherDetail)
            {
                var four = voucher.VoucherNo.Substring(voucher.VoucherNo.Length - 4, 4);
                AssetsGeneralLedger_Swap asset = new AssetsGeneralLedger_Swap();
                asset.CREATE_DATE = DateTime.Now;
                //asset.SubjectVGUID = guid;
                asset.LINE_ID = item.TryToString();
                asset.LEDGER_NAME = voucher.AccountModeName;
                asset.JE_BATCH_NAME = voucher.BatchName + four;
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
                asset.STATUS = "N";
                asset.TRASACTION_ID = Guid.NewGuid().TryToString();
                asset.JE_LINE_NUMBER = items.JE_LINE_NUMBER;
                asset.SEGMENT1 = items.CompanySection;
                asset.SEGMENT2 = items.SubjectSection;
                asset.SEGMENT3 = items.AccountSection;
                asset.SEGMENT4 = items.CostCenterSection;
                asset.SEGMENT5 = items.SpareOneSection;
                asset.SEGMENT6 = items.SpareTwoSection;
                asset.SEGMENT7 = items.IntercourseSection;
                asset.ENTERED_CR = items.LoanMoney.TryToString() == "" ? "0" : items.LoanMoney.TryToString();
                asset.ENTERED_DR = items.BorrowMoney.TryToString() == "" ? "0" : items.BorrowMoney.TryToString();
                asset.ACCOUNTED_DR = items.BorrowMoney.TryToString();
                asset.ACCOUNTED_CR = items.LoanMoney.TryToString();
                //asset.SubjectCount = items.CompanySection + "." + items.SubjectSection + "." + items.AccountSection + "." + items.CostCenterSection + "." + items.SpareOneSection + "." + items.SpareTwoSection + "." + items.IntercourseSection;
                //同步至中间表
                db.Insertable(asset).IgnoreColumns(it => new { it.VGUID, it.SubjectVGUID }).ExecuteCommand();
            }
        }
    }
}