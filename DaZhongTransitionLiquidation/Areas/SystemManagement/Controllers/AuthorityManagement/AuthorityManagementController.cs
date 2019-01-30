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
                    var moduleInfos = db.Queryable<Sys_Module>().OrderBy(i => i.CreatedDate, OrderByType.Desc).ToList();
                    var sysRoles = o.GetRoleModule(roleInfo, moduleInfos, permissionList);
                    if (sysRoles.Count > 0)
                    {
                        //先删除再新增
                        db.Deleteable<Sys_Role_Module>().Where(i => i.RoleVGUID == roleInfo.Vguid).ExecuteCommand();
                        db.Insertable(sysRoles).ExecuteCommand();
                    }
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
        public ActionResult GetModules()
        {
            var jsonResult = new JsonResultModel<Sys_Module>();
            List<Sys_Module> sys_Modules = new List<Sys_Module>();
            DbService.Command(db =>
            {
                sys_Modules = db.Queryable<Sys_Module>().OrderBy(c => c.Zorder).OrderBy(z => z.CreatedDate).ToList();
            });
            jsonResult.Rows = sys_Modules;
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}