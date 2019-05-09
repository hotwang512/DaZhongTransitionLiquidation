using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.LoanApplicationDetail
{
    public class LoanApplicationDetailController : BaseController
    {
        public LoanApplicationDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/LoanApplicationDetail
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
        public JsonResult GetOrgInfo(string accountMode, string companyCode)
        {
            var jsonResult = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult = db.Queryable<Business_SevenSection>().Where(x => x.AccountModeCode == accountMode && x.CompanyCode == companyCode && x.SectionVGUID == "D63BD715-C27D-4C47-AB66-550309794D43"
                                                                    && x.Code.Substring(0,2) == "10" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}