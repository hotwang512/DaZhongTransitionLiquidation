using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.HomePage.Controllers
{
    public class CompanyHomePageController : BaseController
    {
        public CompanyHomePageController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: HomePage/CompanyHomePage
        public ActionResult Index()
        {
            //ViewBag.CompanyCode = GetCompanyCode();
            ViewBag.AccountModeCode = GetAccountModeCode();
            return View();
        }
        public List<Business_UserCompanySet> GetAccountModeCode()
        {
            var result = new List<Business_UserCompanySet>();
            DbBusinessDataService.Command(db =>
            {
                if (UserInfo.LoginName.ToLower() == "admin")
                {
                    var data = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
                    //var datas = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
                    foreach (var item in data)
                    {
                        Business_UserCompanySet uc = new Business_UserCompanySet();
                        uc.Code = item.Code;
                        uc.Descrption = item.Descrption;
                        result.Add(uc);
                    }
                }
                else
                {
                    result = db.Queryable<Business_UserCompanySet>().Where(x => x.UserVGUID == UserInfo.Vguid.TryToString() && x.Block == "1" && x.IsCheck == true).PartitionBy(it => new { it.Code }).Take(1).ToList();
                }

            });
            return result;
        }
        public JsonResult GetCompanyCode(string accountMode)
        {
            List<Business_UserCompanySet> result = new List<Business_UserCompanySet>();
            DbBusinessDataService.Command(db =>
            {
                if (UserInfo.LoginName.ToLower() == "admin")
                {
                    var data = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43"
                                                                    && x.Status == "1" && x.AccountModeCode == accountMode).OrderBy("Code asc").ToList();
                    //var datas = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
                    foreach (var item in data)
                    {
                        Business_UserCompanySet uc = new Business_UserCompanySet();
                        uc.CompanyCode = item.Code;
                        uc.CompanyName = item.Descrption;
                        result.Add(uc);
                    }
                }
                else
                {
                    result = db.Queryable<Business_UserCompanySet>().Where(x => x.UserVGUID == UserInfo.Vguid.TryToString() && x.Code == accountMode && x.Block == "1" && x.IsCheck == true).OrderBy("CompanyCode asc").ToList();
                }
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveUserInfo(string ComapnyCode,string AccountModeCode,string CompanyName,string AccountModeName)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = true, Status = "0" };
            DbService.Command<Sys_User>((db, o) =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                cache[PubGet.GetUserKey].CompanyCode = ComapnyCode;
                cache[PubGet.GetUserKey].AccountModeCode = AccountModeCode;
                cache[PubGet.GetUserKey].CompanyName = CompanyName;
                cache[PubGet.GetUserKey].AccountModeName = AccountModeName;
                //Sys_User userInfos = new Sys_User();
                //userInfos.CompanyCode = ComapnyCode;
                //CacheManager<Sys_User>.GetInstance().Add("ComapnyCode", userInfos, 8 * 60 * 60);
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult SaveUserInfoChange(string AccountModeCode)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = true, Status = "0" };
            var ComapnyCode = "";
            var CompanyName = "";
            var AccountModeName = "";
            DbBusinessDataService.Command(db =>
            {
                var data = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == AccountModeCode).ToList().FirstOrDefault();
                AccountModeName = data.Descrption;
                var companyData = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == AccountModeCode).OrderBy("Code asc").ToList().FirstOrDefault();
                ComapnyCode = companyData.Code;
                CompanyName = companyData.Descrption;
            });
            DbService.Command<Sys_User>((db, o) =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                cache[PubGet.GetUserKey].CompanyCode = ComapnyCode;
                cache[PubGet.GetUserKey].AccountModeCode = AccountModeCode;
                cache[PubGet.GetUserKey].CompanyName = CompanyName;
                cache[PubGet.GetUserKey].AccountModeName = AccountModeName;
                //Sys_User userInfos = new Sys_User();
                //userInfos.CompanyCode = ComapnyCode;
                //CacheManager<Sys_User>.GetInstance().Add("ComapnyCode", userInfos, 8 * 60 * 60);
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult GetUserCompanyInfo()//Guid[] vguids
        {
            var response = new List<Business_UserCompanySet>();
            DbBusinessDataService.Command(db =>
            {
                //var data = db.Queryable<Business_UserCompanySet>().Where(x => x.UserVGUID == UserVGUID && x.Block == "1").Count();
                if (UserInfo.LoginName.ToLower() == "admin")
                {
                    response = db.SqlQueryable<Business_UserCompanySet>(@"select t1.Code,t1.Descrption,t2.Code as CompanyCode ,t2.Descrption as CompanyName,
 (t1.Code+t2.Code) as KeyData from Business_SevenSection t1 
 JOIN Business_SevenSection t2 on t1.Code = t2.AccountModeCode
where t1.SectionVGUID='H63BD715-C27D-4C47-AB66-550309794D43' and t2.SectionVGUID='A63BD715-C27D-4C47-AB66-550309794D43'").OrderBy("Code asc,CompanyCode asc").ToList();
                }
                else
                {
                    response = db.Queryable<Business_UserCompanySet>().Where(x=>x.UserVGUID == UserInfo.Vguid.TryToString() && x.Block=="1" && x.IsCheck == true).OrderBy("Code asc,CompanyCode asc").ToList();
                }
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}