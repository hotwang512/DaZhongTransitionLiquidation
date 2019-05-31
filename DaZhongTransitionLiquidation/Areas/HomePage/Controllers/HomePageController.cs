using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.HomePage.Controllers
{
    public class HomePageController : BaseController
    {
        public HomePageController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: HomePage/HomePage
        public ActionResult Index()
        {
            ViewBag.AccountMode = GetAccountMode();
            ViewBag.SysUser = GetSys_User();
            ViewBag.CurrentModulePermission = GetRoleModule();
            return View();
        }
        public List<Business_UserCompanySet> GetAccountMode()
        {
            var response = new List<Business_UserCompanySet>();
            DbBusinessDataService.Command(db =>
            {
                //var data = db.Queryable<Business_UserCompanySet>().Where(x => x.UserVGUID == UserVGUID && x.Block == "1").Count();
                if (UserInfo.LoginName.ToLower() == "admin")
                {
                    //                    response = db.SqlQueryable<Business_UserCompanySet>(@"select t1.Code,t1.Descrption,t2.Code as CompanyCode ,t2.Descrption as CompanyName,
                    // (t1.Code+t2.Code) as KeyData from Business_SevenSection t1 
                    // JOIN Business_SevenSection t2 on t1.Code = t2.AccountModeCode
                    //where t1.SectionVGUID='H63BD715-C27D-4C47-AB66-550309794D43' and t2.SectionVGUID='A63BD715-C27D-4C47-AB66-550309794D43'").OrderBy("Code asc,CompanyCode asc").ToList();
                    response = db.SqlQueryable<Business_UserCompanySet>(@"select Code,Descrption from Business_UserCompanySet where Block = '1' and  Code is not null group by Code,Descrption")
                                       .OrderBy("Code asc").ToList();
                }
                else
                {
                    //response = db.Queryable<Business_UserCompanySet>().Where(x => x.UserVGUID == UserInfo.Vguid.TryToString() && x.Block == "1" && x.IsCheck == true)
                    //           .OrderBy("Code asc,CompanyCode asc").ToList();
                    response = db.SqlQueryable<Business_UserCompanySet>(@"select Code,Descrption from Business_UserCompanySet
                    where UserVGUID = '" + UserInfo.Vguid.TryToString() + "' and Block = '1' and IsCheck = 1 and Code is not null group by Code,Descrption")
                    .OrderBy("Code asc").ToList();
                }
            });
            return response;
        }
        public Sys_User GetSys_User()
        {
            var result = new Sys_User();
            DbService.Command(db =>
            {
                result = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            });
            return result;
        }
        public List<Sys_Role_Module> GetRoleModule()
        {
            var result = new List<Sys_Role_Module>();
            DbService.Command(db =>
            {
                var data = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
                var results = db.SqlQueryable<Sys_Role_Module>(@"select * from Sys_Role_Module where RoleVGUID='" + data.Role + @"' and ModuleVGUID in(
select ModuleVGUID from Sys_Module where Parent is null)").ToList();

                if (results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "50F3C129-5C30-4B2F-A942-6B309C6278C6").Count() != 0)
                {
                    result.Add(results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "50F3C129-5C30-4B2F-A942-6B309C6278C6").FirstOrDefault());
                }
                if (results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "185A294A-8BC7-4F9B-943A-B4F6F8791587").Count() != 0)
                {
                    result.Add(results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "185A294A-8BC7-4F9B-943A-B4F6F8791587").FirstOrDefault());
                }
                if (results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "2C708F01-9229-48B8-9CAB-63407D1945E0").Count() != 0)
                {
                    result.Add(results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "2C708F01-9229-48B8-9CAB-63407D1945E0").FirstOrDefault());
                }
                if (results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "368296C7-667C-4316-96BC-1370ED9C50BC").Count() != 0)
                {
                    result.Add(results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "368296C7-667C-4316-96BC-1370ED9C50BC").FirstOrDefault());
                }
                if (results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "B9B5E47C-C646-4D0B-877D-BA456968C346").Count() != 0)
                {
                    result.Add(results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "B9B5E47C-C646-4D0B-877D-BA456968C346").FirstOrDefault());
                }
                if (results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "F1715105-E901-46E4-83A1-52B26BCAADF3").Count() != 0)
                {
                    result.Add(results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "F1715105-E901-46E4-83A1-52B26BCAADF3").FirstOrDefault());
                }
                if (results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "6BAA4F3E-BCEA-45C4-8D57-F2E9A96CE06C").Count() != 0)
                {
                    result.Add(results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "6BAA4F3E-BCEA-45C4-8D57-F2E9A96CE06C").FirstOrDefault());
                }
                if (results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "9F0F218B-9AEA-4917-A322-66604E9C29CF").Count() != 0)
                {
                    result.Add(results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "9F0F218B-9AEA-4917-A322-66604E9C29CF").FirstOrDefault());
                }
                if (results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "23EF85FF-E6F1-46B7-8DBD-8FD99BEBB24A").Count() != 0)
                {
                    result.Add(results.Where(x => x.ModuleVGUID.TryToString().ToUpper() == "23EF85FF-E6F1-46B7-8DBD-8FD99BEBB24A").FirstOrDefault());
                }

            });
            return result;
        }
    }
}