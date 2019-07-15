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

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.ComeOnApplicationDetail
{
    public class ComeOnApplicationDetailController : BaseController
    {
        public ComeOnApplicationDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/ComeOnApplicationDetail
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
        public JsonResult GetComeOnApplication(Guid vguid)
        {
            Business_ComeOnAllocationInfo orderList = new Business_ComeOnAllocationInfo();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_ComeOnAllocationInfo>().Single(x => x.VGUID == vguid);
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult SaveComeOnApplicationDetail(Business_ComeOnAllocationInfo sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var isAny = db.Queryable<Business_ComeOnAllocationInfo>().Any(x => x.VGUID == sevenSection.VGUID);
                    if (!isAny)
                    {
                        var no = db.Ado.GetString(@"select top 1 No from Business_ComeOnAllocationInfo a where DATEDIFF(month,a.CreateTime,@NowDate)=0 
                                 and a.No not like '%N%' order by No desc", new { @NowDate = DateTime.Now });
                        sevenSection.VGUID = Guid.NewGuid();
                        sevenSection.No = GetVoucherName(no);
                        sevenSection.Status = "1";
                        sevenSection.CreateTime = DateTime.Now;
                        sevenSection.Founder = UserInfo.LoginName;
                        db.Insertable(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.ChangeTime = DateTime.Now;
                        sevenSection.Changer = UserInfo.LoginName;
                        db.Updateable(sevenSection).IgnoreColumns(it => new { it.CreateTime, it.Founder }).ExecuteCommand();
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