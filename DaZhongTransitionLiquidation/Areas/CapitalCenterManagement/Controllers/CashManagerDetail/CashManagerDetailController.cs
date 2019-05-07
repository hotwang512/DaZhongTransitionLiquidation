using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CashManagerDetail
{
    public class CashManagerDetailController : BaseController
    {
        public CashManagerDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CashManagerDetail
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
        public JsonResult GetBankInfo(string accountMode, string companyCode)
        {
            var jsonResult = new Business_CompanyBankInfo();
            DbBusinessDataService.Command(db =>
            {
                jsonResult = db.Queryable<Business_CompanyBankInfo>().Where(x=>x.AccountModeCode == accountMode && x.CompanyCode == companyCode && x.AccountType == "基本户").First();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}