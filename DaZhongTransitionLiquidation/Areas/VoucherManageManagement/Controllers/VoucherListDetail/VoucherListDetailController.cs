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
            ViewBag.CompanyCode = GetCompanyCode();
            ViewBag.AccountSection = GetAccountSection();
            ViewBag.CostCenterSection = GetCostCenterSection();
            ViewBag.SpareOneSection = GetSpareOneSection();
            ViewBag.SpareTwoSection = GetSpareTwoSection();
            ViewBag.IntercourseSection = GetIntercourseSection();
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public List<Business_SevenSection> GetCompanyCode()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public List<Business_SevenSection> GetAccountSection()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "C63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public List<Business_SevenSection> GetCostCenterSection()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "D63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public List<Business_SevenSection> GetSpareOneSection()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "E63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public List<Business_SevenSection> GetSpareTwoSection()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "F63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
        }
        public List<Business_SevenSection> GetIntercourseSection()
        {
            var result = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "G63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
            });
            return result;
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
    }
}