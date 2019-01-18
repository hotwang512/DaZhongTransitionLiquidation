using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
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
            ViewBag.CompanyCode = GetCompanyCode();
            return View();
        }
        public List<Sys_UserCompany> GetCompanyCode()
        {
            var result = new List<Sys_UserCompany>();
            if (UserInfo.LoginName.ToLower() == "sysadmin")
            {
                DbBusinessDataService.Command(db => 
                {
                    var data = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Status == "1").OrderBy("Code asc").ToList();
                    foreach (var item in data)
                    {
                        Sys_UserCompany uc = new Sys_UserCompany();
                        uc.CompanyCode = item.Code;
                        uc.CompanyCodeName = item.Descrption;
                        result.Add(uc);
                    } 
                });
            }
            else
            {
                DbService.Command(db =>
                {
                    result = db.Queryable<Sys_UserCompany>().Where(x => x.UserName == UserInfo.LoginName && x.Status == "1").ToList();
                });
            }
            return result;
        }
        public JsonResult SaveUserInfo(string ComapnyCode)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = true, Status = "0" };
            DbService.Command<Sys_User>((db, o) =>
            {
                var cache = CacheManager<Sys_User>.GetInstance();
                cache[PubGet.GetUserKey].CompanyCode = ComapnyCode;
                //Sys_User userInfos = new Sys_User();
                //userInfos.CompanyCode = ComapnyCode;
                //CacheManager<Sys_User>.GetInstance().Add("ComapnyCode", userInfos, 8 * 60 * 60);
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}