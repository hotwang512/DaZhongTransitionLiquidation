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
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;

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
                asset.ENTERED_CR = items.LoanMoney.TryToString() ;
                asset.ENTERED_DR = items.BorrowMoney.TryToString();
                asset.ACCOUNTED_DR = items.BorrowMoney.TryToString();
                asset.ACCOUNTED_CR = items.LoanMoney.TryToString();
                //asset.SubjectCount = items.CompanySection + "." + items.SubjectSection + "." + items.AccountSection + "." + items.CostCenterSection + "." + items.SpareOneSection + "." + items.SpareTwoSection + "." + items.IntercourseSection;
                //同步至中间表
                db.Insertable(asset).IgnoreColumns(it => new { it.VGUID, it.SubjectVGUID }).ExecuteCommand();
            }
        }

        public JsonResult SyncAssetsData()
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var data = new List<AssetsGeneralLedger_Swap>();
                //从已审核凭证中提取数据
                var AssetsData = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select (b.CompanySection+'.'+b.SubjectSection+'.'+b.AccountSection+'.'+b.CostCenterSection+'.'+b.SpareOneSection+'.'+b.SpareTwoSection+'.'+b.IntercourseSection) as SubjectCount,a.AccountModeName as LEDGER_NAME,a.BatchName as JE_BATCH_NAME,
a.VoucherNo as JE_HEADER_NAME,a.VoucherDate as ACCOUNTING_DATE,b.BorrowMoney as ENTERED_DR,b.LoanMoney as ENTERED_CR from Business_VoucherList as a left join Business_VoucherDetail as b on a.VGUID = b.VoucherVGUID
 where a.Status='3' ").ToList();
                //从总账明细中提取数据
                var AssetsDetailData = db.Ado.SqlQuery<AssetsGeneralLedger_Swap>(@"select COMBINATION as SubjectCount,LEDGER_NAME,JE_BATCH_NAME,JE_HEADER_NAME,JE_CATEGORY_NAME,ACCOUNTING_DATE,ENTERED_DR,ENTERED_CR
from AssetsGeneralLedgerDetail_Swap ").ToList();
                foreach (var item in AssetsDetailData)
                {
                    //item.ENTERED_CR = item.ENTERED_CR == null ? "0.00" : item.ENTERED_CR;
                    //item.ENTERED_DR = item.ENTERED_DR == null ? "0.00" : item.ENTERED_DR;
                    var isAny = AssetsData.Any(x => x.LEDGER_NAME == item.LEDGER_NAME && x.SubjectCount == item.SubjectCount && x.ACCOUNTING_DATE == item.ACCOUNTING_DATE && item.JE_HEADER_NAME.Contains(x.JE_HEADER_NAME) && x.ENTERED_DR == item.ENTERED_DR && x.ENTERED_CR == item.ENTERED_CR);
                    if (!isAny)
                    {
                        data.Add(item);
                    }
                }
                //将与Oracle差异的数据同步至中间表
                var voucherList = new List<Business_VoucherList>();
                var voucherDetail = new List<Business_VoucherDetail>();
                var sevenData1 = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                var sevenData2 = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43").ToList();
                var sevenData3 = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ToList();
                //var voucherData = db.Queryable<Business_VoucherList>().Where(x => x.Automatic == "3").ToList();
                var assetsData = db.Queryable<AssetsGeneralLedgerDetail_Swap>().ToList();
                //var tableData = jsonData.JsonToModel<List<AssetsGeneralLedger_Swap>>();
                foreach (var item in data)
                {
                    var isAny = voucherList.Any(x => x.VoucherNo == item.JE_HEADER_NAME.Split(" ")[0]);
                    if (isAny)
                    {
                        continue;
                    }
                    var account = sevenData2.SingleOrDefault(x => x.Descrption == item.LEDGER_NAME).Code;
                    var company = sevenData1.SingleOrDefault(x => x.AccountModeCode == account && x.Code == item.SubjectCount.Split(".")[0]).Descrption;
                    var credit = item.ENTERED_CR == "0" ? item.ENTERED_DR : item.ENTERED_CR;
                    var debit = item.ENTERED_DR == "0" ? item.ENTERED_CR : item.ENTERED_DR;
                    Business_VoucherList voucher = new Business_VoucherList();
                    voucher.AccountingPeriod = item.ACCOUNTING_DATE;
                    voucher.AccountModeName = item.LEDGER_NAME;
                    voucher.Auditor = "";
                    voucher.Bookkeeping = "";
                    voucher.Cashier = "";
                    voucher.CompanyCode = item.SubjectCount.Split(".")[0];
                    voucher.CompanyName = company;
                    voucher.Currency = "";
                    voucher.DocumentMaker = "";
                    voucher.FinanceDirector = "";
                    voucher.Status = "2";
                    voucher.VoucherDate = item.ACCOUNTING_DATE;
                    voucher.VoucherType = item.JE_CATEGORY_NAME.Split(".")[1] + "类";
                    voucher.CreditAmountTotal = credit.TryToDecimal();
                    voucher.DebitAmountTotal = debit.TryToDecimal();
                    voucher.CreateTime = DateTime.Now;
                    var guid = Guid.NewGuid();
                    voucher.BatchName = item.JE_BATCH_NAME.Split(" ")[0];
                    voucher.VoucherNo = item.JE_HEADER_NAME.Split(" ")[0];
                    voucher.VGUID = guid;
                    voucher.Automatic = "3";//Oracle同步
                    voucherList.Add(voucher);
                    //凭证明细表
                    var assetsDataList = assetsData.Where(x => x.LEDGER_NAME == item.LEDGER_NAME && x.JE_HEADER_NAME == item.JE_HEADER_NAME && x.ACCOUNTING_DATE == item.ACCOUNTING_DATE).ToList();
                    if (assetsDataList.Count > 0)
                    {
                        foreach (var ass in assetsDataList)
                        {
                            var subject = sevenData3.Where(x => x.Code == ass.COMBINATION.Split(".")[1]).FirstOrDefault().Descrption;
                            Business_VoucherDetail BVDetail = new Business_VoucherDetail();
                            BVDetail.Abstract = "Oracle同步数据";
                            BVDetail.CompanySection = ass.COMBINATION.Split(".")[0];
                            BVDetail.SubjectSection = ass.COMBINATION.Split(".")[1];
                            BVDetail.SubjectSectionName = subject;
                            BVDetail.AccountSection = ass.COMBINATION.Split(".")[2];
                            BVDetail.CostCenterSection = ass.COMBINATION.Split(".")[3];
                            BVDetail.SpareOneSection = ass.COMBINATION.Split(".")[4];
                            BVDetail.SpareTwoSection = ass.COMBINATION.Split(".")[5];
                            BVDetail.IntercourseSection = ass.COMBINATION.Split(".")[6];
                            BVDetail.SevenSubjectName = ass.COMBINATION + ass.COMBINATION_DESCRIPTION;
                            BVDetail.BorrowMoney = ass.ENTERED_DR;
                            BVDetail.LoanMoney = ass.ENTERED_CR;
                            BVDetail.BorrowMoneyCount = ass.ENTERED_DR;
                            BVDetail.LoanMoneyCount = ass.ENTERED_CR;
                            BVDetail.JE_LINE_NUMBER = 0;
                            BVDetail.VGUID = Guid.NewGuid();
                            BVDetail.VoucherVGUID = guid;
                            voucherDetail.Add(BVDetail);
                        }
                    }
                }
                if (voucherList.Count > 0 && voucherDetail.Count > 0)
                {
                    db.Insertable(voucherList).ExecuteCommand();
                    db.Insertable(voucherDetail).ExecuteCommand();
                    resultModel.IsSuccess = true;
                }
                else
                {
                    resultModel.Status = "1";
                }
            });
            return Json(resultModel);
        }
    }
}