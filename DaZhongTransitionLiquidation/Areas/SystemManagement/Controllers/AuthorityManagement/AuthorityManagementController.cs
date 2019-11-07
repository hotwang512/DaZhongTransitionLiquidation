using System;
using System.Linq;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System.Collections.Generic;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.AuthorityManagement
{
    public class AuthorityManagementController : BaseController
    {
        // GET: SystemManagement/AuthorityManagement

        public AuthorityManagementController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        public ActionResult AuthorityInfo()
        {
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.AuthorityManagement);
            return View();
        }

        public ActionResult AuthorityDetail()
        {
            ViewBag.IsEdit = Request["isEdit"].TryToBoolean();
            ViewData["Vguid"] = Request["Vguid"] ?? "";
            return View();
        }
        /// <summary>
        /// 获取所有的角色信息
        /// </summary>
        /// <param name="roleInfo"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public JsonResult GetRoleInfos(Sys_Role roleInfo, GridParams para)
        {
            var jsonResult = new JsonResultModel<Sys_Role>();
            DbService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Sys_Role>().WhereIF(!string.IsNullOrEmpty(roleInfo.Role), i => i.Role.Contains(roleInfo.Role))
                .OrderBy(i => i.CreatedDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 删除角色信息
        /// </summary>
        /// <param name="vguids">主键</param>
        /// <returns></returns>
        public JsonResult DeleteRoleInfos(Guid[] vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbService.Command(db =>
            {
                int saveChanges = db.Deleteable<Sys_Role>().In(vguids).ExecuteCommand();
                db.Deleteable<Sys_Role_Module>().Where(i => vguids.Contains(i.RoleVGUID)).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Length;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        /// <summary>
        /// 从Sys_Role_Fixed获取每个界面权限列表
        /// </summary>
        /// <param name="para"></param>
        /// <param name="roleVguid"></param>
        /// <returns></returns>
        public JsonResult GetModulePermissions(GridParams para, string roleVguid)
        {
            var jsonResult = new JsonResultModel<Sys_Role_Fixed>();
            DbService.Command<AuthorityManagementPack>((db, o) =>
            {
                var query = db.Queryable<Sys_Role_Fixed>().OrderBy(i => i.PageID);
                jsonResult.Rows = query.ToList();
                jsonResult.TotalRows = query.Count();
                var roleInfos = db.Queryable<Sys_Role_Module>().Where(i => i.RoleVGUID == roleVguid.TryToGuid()).ToList();
                o.GetModulePermissions(jsonResult.Rows, roleInfos);
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存角色（新增或更新）
        /// </summary>
        /// <param name="roleInfo">角色信息</param>
        /// <param name="permissionList">用户选择的每个模块中有哪些权限（即按钮）</param>
        /// <param name="isEdit">是否编辑界面</param>
        /// <returns></returns>
        public JsonResult SaveRole(Sys_Role roleInfo, string permissionList, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (isEdit)
            {
                roleInfo.ChangeDate = DateTime.Now;
                roleInfo.ChangeUser = UserInfo.LoginName;
            }
            else
            {
                roleInfo.CreatedUser = UserInfo.LoginName;
                roleInfo.CreatedDate = DateTime.Now;
                roleInfo.Vguid = Guid.NewGuid();
            }
            DbService.Command<AuthorityManagementPack>((db, o) =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (o.IsExistRoleName(db, roleInfo, isEdit))
                    {
                        resultModel.Status = "2";
                        return;
                    }
                    if (isEdit)
                    {
                        db.Updateable(roleInfo).IgnoreColumns(i => new { i.CreatedDate, i.CreatedUser }).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(roleInfo).ExecuteCommand();
                    }
                    var roleMenu = permissionList.JsonToModel<List<Sys_Role_ModuleMenu>>();
                    
                    List<Sys_Role_ModuleMenu> roleMenuList = new List<Sys_Role_ModuleMenu>();
                    foreach (var item in roleMenu)
                    {
                        var data = roleMenuList.Where(x => x.ModuleMenuVGUD == item.ModuleMenuVGUD).ToList();
                        if (data.Count > 0)
                        {
                            data[0].Look = item.Look == true? true: data[0].Look; data[0].New = item.New == true ? true : data[0].New; data[0].Edit = item.Edit == true ? true : data[0].Edit;
                            data[0].StrikeOut = item.StrikeOut == true ? true : data[0].StrikeOut; data[0].Obsolete = item.Obsolete == true ? true : data[0].Obsolete; data[0].Submit = item.Submit == true ? true : data[0].Submit;
                            data[0].Review = item.Review == true ? true : data[0].Review; data[0].GoBack = item.GoBack == true ? true : data[0].GoBack; data[0].Import = item.Import == true ? true : data[0].Import;
                            data[0].Export = item.Export == true ? true : data[0].Export; data[0].Generate = item.Generate == true ? true : data[0].Generate; data[0].Calculation = item.Calculation == true ? true : data[0].Calculation;
                            data[0].Preview = item.Preview == true ? true : data[0].Preview; data[0].Enable = item.Enable == true ? true : data[0].Enable; data[0].ComOrMan = item.ComOrMan == "1" ? "1" : data[0].ComOrMan;
                        }
                        else
                        {
                            item.VGUID = Guid.NewGuid();
                            item.RoleVGUID = roleInfo.Vguid;
                            roleMenuList.Add(item);
                        }
                    }
                    if (roleMenuList.Count > 0)
                    {
                        //先删除再新增
                        db.Deleteable<Sys_Role_ModuleMenu>().Where(i => i.RoleVGUID == roleInfo.Vguid).ExecuteCommand();
                        db.Insertable(roleMenuList).ExecuteCommand();
                    }
                    //var moduleInfos = db.Queryable<Sys_Module>().OrderBy(i => i.CreatedDate, OrderByType.Desc).ToList();
                    //var sysRoles = o.GetRoleModule(roleInfo, moduleInfos, permissionList);
                    //if (sysRoles.Count > 0)
                    //{
                    //    //先删除再新增
                    //    db.Deleteable<Sys_Role_Module>().Where(i => i.RoleVGUID == roleInfo.Vguid).ExecuteCommand();
                    //    db.Insertable(sysRoles).ExecuteCommand();
                    //}
                });
                if (resultModel.Status == "2") return;
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        /// <summary>
        /// 通过主键获取角色信息
        /// </summary>
        /// <param name="vguid"></param>
        /// <returns></returns>
        public JsonResult GetRoleInfoByVguid(Guid vguid)
        {
            var roleInfo = new Sys_Role();
            DbService.Command(db =>
            {
                roleInfo = db.Queryable<Sys_Role>().Single(i => i.Vguid == vguid);
            });
            return Json(roleInfo);
        }
        public JsonResult GetModules(string roleVguid)
        {
            var jsonResult = new JsonResultModel<Sys_Module>();
            List<Sys_Module> sys_Modules = new List<Sys_Module>();
            DbService.Command<AuthorityManagementPack>((db,o) =>
            {
                sys_Modules = db.Queryable<Sys_Module>().OrderBy(c => c.Zorder).OrderBy(z => z.CreatedDate).ToList();
                var roleInfos = db.Queryable<Sys_Role_Module>().Where(i => i.RoleVGUID == roleVguid.TryToGuid()).ToList();
                o.GetModulePermissionsNew(sys_Modules, roleInfos);
            });
            jsonResult.Rows = sys_Modules;
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewModules(Guid? roleVguid)
        {
            var jsonResult = new JsonResultModel<SysNewModulesModule>();
            DbService.Command(db =>
            {
                
                if(roleVguid != null)
                {
                    jsonResult.Rows = db.Ado.SqlQuery<SysNewModulesModule>(@"select a.Name,a.VGUID as KeyVGUID,a.Parent,a.Look as IsLook,a.New as IsNew,a.Edit as IsEdit,a.StrikeOut as IsStrikeOut,a.Obsolete as IsObsolete,
                                        a.Submit as IsSubmit,a.Review as IsReview,a.GoBack as IsGoBack,a.Import as IsImport,a.Export as IsExport,a.Generate as IsGenerate,a.Calculation as IsCalculation,a.Preview as IsPreview,a.Enable as IsEnable,a.Zorder,a.Type,
                                        b.* from Sys_ModuleMenu as a left join Sys_Role_ModuleMenu as b on a.VGUID = b.ModuleMenuVGUD and b.RoleVGUID=@RoleVGUID  order by a.Zorder",
                                        new { RoleVGUID = roleVguid }).ToList();
                }
                else
                {
                    jsonResult.Rows = db.Ado.SqlQuery<SysNewModulesModule>(@"select a.Name,a.VGUID as KeyVGUID,a.Parent,a.Look as IsLook,a.New as IsNew,a.Edit as IsEdit,a.StrikeOut as IsStrikeOut,a.Obsolete as IsObsolete,
                                        a.Submit as IsSubmit,a.Review as IsReview,a.GoBack as IsGoBack,a.Import as IsImport,a.Export as IsExport,a.Generate as IsGenerate,a.Calculation as IsCalculation,a.Preview as IsPreview,a.Enable as IsEnable,a.Zorder,a.Type,
                                        b.* from Sys_ModuleMenu as a left join Sys_Role_ModuleMenu as b on a.VGUID = b.ModuleMenuVGUD  and b.RoleVGUID='00000000-0000-0000-0000-000000000000'  order by a.Zorder").ToList();
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveRoleInfo(string roleName,string description)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    Sys_Role role = new Sys_Role();
                    role.Role = roleName;
                    role.Description = description;
                    role.CreatedUser = UserInfo.LoginName;
                    role.CreatedDate = DateTime.Now;
                    role.Vguid = Guid.NewGuid();
                    var isAny = db.Queryable<Sys_Role>().Any(x => x.Description == role.Description);
                    if (isAny)
                    {
                        resultModel.Status = "2";
                        return;
                    }
                    db.Insertable(role).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                if(resultModel.Status != "2")
                {
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}