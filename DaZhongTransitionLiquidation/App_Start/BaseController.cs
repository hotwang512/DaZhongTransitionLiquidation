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
            if (UserInfo != null)   
            {
                ViewBag.CurrentUserRoleModules = GetCurrentUserRoleModules();
                if (UserInfo.LoginName == "sysAdmin") return;
                //缓存当前用户的权限
                var roleModules = GetCurrentUserRoleModules().Select(i => new U_RoleModule { PageID = i.PageID, PageName = i.PageName }).ToList();
                CacheManager<List<U_RoleModule>>.GetInstance().Add(UserInfo.Vguid.ToString(), roleModules);
            }
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
                    roleModuleInfo = db.Queryable<Sys_Role_Module>().Single(i => i.RoleVGUID == UserInfo.Role.TryToGuid() && i.ModuleVGUID == moduleVguid.TryToGuid());
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