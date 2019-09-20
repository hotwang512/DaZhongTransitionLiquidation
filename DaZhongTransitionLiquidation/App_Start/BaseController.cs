using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;

namespace DaZhongTransitionLiquidation
{
    public class BaseController : Controller
    {
        public DbService DbService;
        public DbBusinessDataService DbBusinessDataService;

        public Sys_User UserInfo;

        public BaseController(DbService dbService, DbBusinessDataService dbBusinessDataService)
        {
            DbService = dbService;
            DbBusinessDataService = dbBusinessDataService;
            string uniqueKey = PubGet.GetUserKey;
            UserInfo = CacheManager<Sys_User>.GetInstance()[uniqueKey];
            ViewBag.User = UserInfo;
            ViewBag.AccountMode = GetAccountMode();
            if (UserInfo != null)   
            {
                ViewBag.CurrentUserRoleModules = GetCurrentUserRoleModules();
                if (UserInfo.LoginName == "admin") return;
                //缓存当前用户的权限
                var roleModules = GetCurrentUserRoleModules().Select(i => new U_RoleModule { PageID = i.PageID, PageName = i.PageName }).ToList();
                CacheManager<List<U_RoleModule>>.GetInstance().Add(UserInfo.Vguid.ToString(), roleModules);
            }
        }
        public List<Business_UserCompanySet> GetAccountMode()
        {
            var result = new List<Business_UserCompanySet>();
            DbBusinessDataService.Command(db =>
            {
                var UserInfo = new Sys_User();
                UserInfo = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
                if (!UserInfo.IsNullOrEmpty())
                {
                    if (UserInfo.LoginName.ToLower() == "admin")
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
                }
                else
                {
                    Redirect("/Login/Index");
                }
            });
            return result;
        }
        /// <summary>
        /// 获取角色下每个模块的权限
        /// </summary>
        /// <param name="moduleVguid">模块</param>
        /// <returns></returns>
        public Sys_Role_Module GetRoleModuleInfo(string moduleVguid)
        {
            var roleModuleInfo = new Sys_Role_Module();
            if (UserInfo.Vguid.ToString().ToLower() == MasterVGUID.AdminVguid.ToLower())
            {
                roleModuleInfo.Reads = 1;
                roleModuleInfo.Edit = 1;
                roleModuleInfo.Deletes = 1;
                roleModuleInfo.Adds = 1;
                roleModuleInfo.Enable = 1;
                roleModuleInfo.Disable = 1;
                roleModuleInfo.Import = 1;
                roleModuleInfo.Export = 1;
            }
            else
            {
                DbService.Command(db =>
                {
                    var roleModuleData = db.Queryable<Sys_Role_Module>().ToList();
                    var roleModuleList = roleModuleData.Where(i => i.RoleVGUID == UserInfo.Role.TryToGuid() && i.ModuleVGUID == moduleVguid.TryToGuid()).ToList();
                    if(roleModuleList.Count == 0)
                    {
                        var roleModule = roleModuleData.Where(i => i.RoleVGUID == UserInfo.Role.TryToGuid()).ToList();
                        if (roleModule.Count > 0)
                        {
                            roleModuleInfo = roleModuleData.Single(i => i.RoleVGUID == UserInfo.Role.TryToGuid() && i.ModuleVGUID == roleModule[0].ModuleVGUID);
                        }
                    }
                    else if(roleModuleList.Count == 1)
                    {
                        roleModuleInfo = roleModuleList.FirstOrDefault();
                    }
                });
            }
            return roleModuleInfo;
        }

        /// <summary>
        /// 获取当前用户的角色模块信息
        /// </summary>
        /// <returns></returns>
        public List<V_Sys_Role_Module> GetCurrentUserRoleModules()
        {
            var roleModules = new List<V_Sys_Role_Module>();
            DbService.Command(db =>
            {
                roleModules = db.Queryable<Sys_Role_Module, Sys_Module>((srm, srf) => new object[] { JoinType.Left, srm.ModuleVGUID == srf.ModuleVGUID })
               .Where((srm, srf) => srm.RoleVGUID == UserInfo.Role.TryToGuid()).Select((srm, srf) => new V_Sys_Role_Module()
               {
                   Reads = srm.Reads,
                   Edit = srm.Edit,
                   Deletes = srm.Deletes,
                   Adds = srm.Adds,
                   Enable = srm.Enable,
                   Disable = srm.Disable,
                   Import = srm.Import,
                   Export = srm.Export,
                   Vguid = srm.Vguid,
                   ModuleVGUID = srm.ModuleVGUID,
                   RoleVGUID = srm.RoleVGUID,
                   //PageID = srf.PageID,
                   //ParentID = srf.ParentID,
                   PageName = srf.ModuleName
               }).ToList();
            });
            return roleModules;
        }
        /// <summary>
        /// 输出自增ID
        /// </summary>
        /// <param name="cId">公司名称</param>
        /// <param name="prefix">自增id的前缀</param>
        /// <param name="dateFormat">时间格式</param>
        /// <param name="length">id长度</param>
        /// <returns>返回id</returns>
        public string AutoGenerateId(string cId, string prefix, string dateFormat, string length)
        {
            var id = "";
            DbService.Command(db =>
            {
                var p1 = new SugarParameter("@CompanyID", cId);
                var p2 = new SugarParameter("@Prefix", prefix);
                var p3 = new SugarParameter("@ADATE", dateFormat);
                var p4 = new SugarParameter("@ALENGTH", length);
                var p5 = new SugarParameter("@AUTONO", null, true);//isOutput=true
                db.Ado.UseStoredProcedure().GetScalar("SP_Saas_SMAUTO", p1, p2, p3, p4, p5);
                id = p5.Value.ToString();
            });
            return id;
        }


        /// <summary>
        /// 获取清单数据
        /// </summary>
        /// <param name="vguid"></param>
        /// <returns></returns>
        public List<SelectListItem> GetMasterData(string vguid)
        {
            var items = new List<SelectListItem>();
            DbService.Command(db =>
            {
                var query = db.Queryable<CS_Master_2>().Where(i => i.VGUID == SqlFunc.ToGuid(vguid)).OrderBy(i=>i.Zorder).ToList();
                foreach (var csMaster2 in query)
                {
                    items.Add(new SelectListItem() { Value = csMaster2.MasterCode, Text = csMaster2.DESC0 });
                }
            });
            return items;
        }

        /// <summary>
        /// 获取当前用户的子部门
        /// </summary>
        /// <returns></returns>
        public List<string> GetUserSubDepartment()
        {
            var subDepartments = new List<string>();
            DbService.Command(db =>
            {
                var currentDepartment = UserInfo.Department;
                var mainDepVguid = db.Queryable<Master_Organization>().Where(i => i.ParentVguid == null).Select(i => i.Vguid).Single();
                var organizations = db.Ado.UseStoredProcedure().SqlQuery<Master_Organization>("usp_Organization_Underling", new { orgvguid = currentDepartment ?? mainDepVguid.ToString() });
                subDepartments = organizations.Select(i => i.Vguid.ToString()).ToList();
            });
            return subDepartments;
        }
    }
}