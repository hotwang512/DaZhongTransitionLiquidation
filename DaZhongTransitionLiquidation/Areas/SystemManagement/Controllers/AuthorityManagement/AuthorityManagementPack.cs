using System;
using System.Collections.Generic;
using System.Linq;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.AuthorityManagement
{
    public class AuthorityManagementPack
    {
        /// <summary>
        /// 从Sys_Role_Fixed获取每个界面权限列表
        /// </summary>
        /// <param name="roleFixeds"></param>
        /// <param name="sysRoleList"></param>
        public void GetModulePermissions(List<Sys_Role_Fixed> roleFixeds, List<Sys_Role_Module> sysRoleList)
        {
            foreach (var modulePermission in roleFixeds)
            {
                if (modulePermission.Reads == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Reads == 1))
                    {
                        modulePermission.Reads = 2;
                    }
                }
                if (modulePermission.Adds == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Adds == 1))
                    {
                        modulePermission.Adds = 2;
                    }
                }
                if (modulePermission.Edit == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Edit == 1))
                    {
                        modulePermission.Edit = 2;
                    }
                }
                if (modulePermission.Deletes == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Deletes == 1))
                    {
                        modulePermission.Deletes = 2;
                    }
                }
                if (modulePermission.Enable == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Enable == 1))
                    {
                        modulePermission.Enable = 2;
                    }
                }
                if (modulePermission.Disable == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Disable == 1))
                    {
                        modulePermission.Disable = 2;
                    }
                }
                if (modulePermission.Import == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Import == 1))
                    {
                        modulePermission.Import = 2;
                    }
                }
                if (modulePermission.Export == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Export == 1))
                    {
                        modulePermission.Export = 2;
                    }
                }
            }
        }
        public void GetModulePermissionsNew(List<Sys_Module> roleFixeds, List<Sys_Role_Module> sysRoleList)
        {
            foreach (var modulePermission in roleFixeds)
            {
                if (modulePermission.Reads == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Reads == 1))
                    {
                        modulePermission.Reads = 2;
                    }
                }
                if (modulePermission.Adds == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Adds == 1))
                    {
                        modulePermission.Adds = 2;
                    }
                }
                if (modulePermission.Edit == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Edit == 1))
                    {
                        modulePermission.Edit = 2;
                    }
                }
                if (modulePermission.Deletes == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Deletes == 1))
                    {
                        modulePermission.Deletes = 2;
                    }
                }
                if (modulePermission.Enable == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Enable == 1))
                    {
                        modulePermission.Enable = 2;
                    }
                }
                if (modulePermission.Disable == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Disable == 1))
                    {
                        modulePermission.Disable = 2;
                    }
                }
                if (modulePermission.Import == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Import == 1))
                    {
                        modulePermission.Import = 2;
                    }
                }
                if (modulePermission.Export == 1)
                {
                    if (sysRoleList.Any(i => i.ModuleVGUID == modulePermission.ModuleVGUID && i.Export == 1))
                    {
                        modulePermission.Export = 2;
                    }
                }
            }
        }
        /// <summary>
        /// 获取当前角色下每个模块拥有的权限以及按钮权限
        /// </summary>
        /// <param name="roleInfo">角色信息</param>
        /// <param name="moduleInfos">模块信息</param>
        /// <param name="permissionList">用户选择的每个模块中有哪些权限（即按钮）</param>
        /// <returns></returns>
        public List<Sys_Role_Module> GetRoleModule(Sys_Role roleInfo, List<Sys_Module> moduleInfos, string permissionList)
        {
            var rolePermissions = new List<U_Module>();
            if (!string.IsNullOrEmpty(permissionList))
            {
                rolePermissions = permissionList.JsonToModel<List<U_Module>>();
            }            
            List<Sys_Role_Module> roleModelInfos = new List<Sys_Role_Module>();
            foreach (var module in moduleInfos)
            {
                //按照模块区分
                var permission = rolePermissions.FindAll(p => p.ModuleName == module.Vguid.ToString());
                var roleModelInfo = new Sys_Role_Module();
                //list为循环当前模块的权限集合
                foreach (var item in permission)
                {                  
                    switch (item.RightType)
                    {
                        case (int)AuthorityEnum.Reads:
                            roleModelInfo.Reads = 1;
                            break;
                        case (int)AuthorityEnum.Adds:
                            roleModelInfo.Adds = 1;
                            break;
                        case (int)AuthorityEnum.Edit:
                            roleModelInfo.Edit = 1;
                            break;
                        case (int)AuthorityEnum.Deletes:
                            roleModelInfo.Deletes = 1;
                            break;
                        case (int)AuthorityEnum.Enable:
                            roleModelInfo.Enable = 1;
                            break;
                        case (int)AuthorityEnum.Disable:
                            roleModelInfo.Disable = 1;
                            break;
                        case (int)AuthorityEnum.Import:
                            roleModelInfo.Import = 1;
                            break;
                        case (int)AuthorityEnum.Export:
                            roleModelInfo.Export = 1;
                            break;
                    } //end switch
                } //end foreach of permission
                if (permission.Count <= 0) continue;
                roleModelInfo.Vguid =Guid.NewGuid();
                roleModelInfo.RoleVGUID = roleInfo.Vguid;
                roleModelInfo.ModuleVGUID = module.Vguid;
                roleModelInfo.CreatedDate = DateTime.Now;
                roleModelInfo.CreatedUser = "admin";
                roleModelInfos.Add(roleModelInfo);
            } //end foreach of sysModules

            return roleModelInfos;
        }

        /// <summary>
        /// 角色名是否存在
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="roleInfo">角色信息</param>
        /// <param name="isEdit">是否编辑</param>
        /// <returns></returns>
        public bool IsExistRoleName(SqlSugarClient db, Sys_Role roleInfo, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<Sys_Role>().Any(i => i.Role == roleInfo.Role && i.Vguid != roleInfo.Vguid);
            }
            return db.Queryable<Sys_Role>().Any(i => i.Role == roleInfo.Role);
        }
    }
}