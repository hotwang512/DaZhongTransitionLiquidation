using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashTransactionDetail
{
    public class CashTransactionDetailController : BaseController
    {
        public CashTransactionDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CashTransactionDetail
        public ActionResult Index()
        {
            ViewBag.GetAccountMode = GetAccountModes();
            return View();
        }
        public List<Business_SevenSection> GetAccountModes()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public JsonResult GetCashTransactionInfo(Guid vguid)
        {
            Business_CashTransaction orderList = new Business_CashTransaction();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_CashTransaction>().Single(x => x.VGUID == vguid);
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult SaveCashTransactionDetail(Business_CashTransaction sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var isAny = db.Queryable<Business_CashTransaction>().Any(x => x.VGUID == sevenSection.VGUID);
                    if (!isAny)
                    {
                        var no = db.Ado.GetString(@"select top 1 Batch from Business_CashTransaction a where DATEDIFF(month,a.CreateTime,@NowDate)=0 
                                  order by No desc", new { @NowDate = DateTime.Now });
                        sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.Batch = GetVoucherName(no);
                        sevenSection.CreateTime = DateTime.Now;
                        sevenSection.CreatePerson = UserInfo.LoginName;
                        db.Insertable(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        db.Updateable(sevenSection).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
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
    }
}