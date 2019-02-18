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

namespace DaZhongTransitionLiquidation.Controllers
{
    public class SharedController : BaseController
    {
        public SharedController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: Shared
        public ActionResult Error503()
        {
            return View();
        }
        public ActionResult Error404()
        {
            return View();
        }
        public ActionResult _Layout()
        {
            ViewBag.AccountMode = GetAccountMode();
            ViewBag.SysUser = GetSys_User();
            return View();
        }
        public List<Business_UserCompanySet> GetAccountMode()
        {
            var result = new List<Business_UserCompanySet>();
            DbBusinessDataService.Command(db =>
            {
                var UserInfo = new Sys_User();
                UserInfo = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
                if (UserInfo.LoginName.ToLower() == "sysadmin")
                {
                    //                    response = db.SqlQueryable<Business_UserCompanySet>(@"select t1.Code,t1.Descrption,t2.Code as CompanyCode ,t2.Descrption as CompanyName,
                    // (t1.Code+t2.Code) as KeyData from Business_SevenSection t1 
                    // JOIN Business_SevenSection t2 on t1.Code = t2.AccountModeCode
                    //where t1.SectionVGUID='H63BD715-C27D-4C47-AB66-550309794D43' and t2.SectionVGUID='A63BD715-C27D-4C47-AB66-550309794D43'").OrderBy("Code asc,CompanyCode asc").ToList();
                    result = db.SqlQueryable<Business_UserCompanySet>(@"select Code,Descrption from Business_UserCompanySet where Block = '1' and  Code is not null group by Code,Descrption")
                                       .OrderBy("Code asc").ToList();
                }
                else
                {
                    //response = db.Queryable<Business_UserCompanySet>().Where(x => x.UserVGUID == UserInfo.Vguid.TryToString() && x.Block == "1" && x.IsCheck == true)
                    //           .OrderBy("Code asc,CompanyCode asc").ToList();
                    result = db.SqlQueryable<Business_UserCompanySet>(@"select Code,Descrption from Business_UserCompanySet
                    where UserVGUID = '" + UserInfo.Vguid.TryToString() + "' and Block = '1' and IsCheck = 1 and Code is not null group by Code,Descrption")
                    .OrderBy("Code asc").ToList();
                }
            });
            return result;
        }
        public Sys_User GetSys_User()
        {
            var result = new Sys_User();
            result = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            return result;
        }
    }
}