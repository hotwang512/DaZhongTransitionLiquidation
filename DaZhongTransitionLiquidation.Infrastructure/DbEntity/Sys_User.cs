using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DaZhongTransitionLiquidation.Infrastructure.DbEntity
{
    ///<summary>
    ///
    ///</summary>
    public class Sys_User
    {
        public Sys_User()
        {

        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string LoginName { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string UserName { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Password { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Company { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Email { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string WorkPhone { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MobileNnumber { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Enable { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Role { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Department { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Remark { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreatedUser { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? ChangeDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ChangeUser { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid Vguid { get; set; }

        /// <summary>
        /// Desc:公司Code
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        /// <summary>
        /// Desc:账套Code
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string AccountModeCode { get; set; }
        public string AccountModeName { get; set; }
        public string Token { get; set; }
        public string Name { get; set; }
        public string RoleStation { get; set; }

        public List<V_Sys_Role_ModuleMenu> Permission { get; set; } = new List<V_Sys_Role_ModuleMenu>();


        /// <summary>
        /// 当前模块权限菜单
        /// </summary>
        public List<V_Sys_Role_ModuleMenu> GetTopModuleMenu()
        {
            List<V_Sys_Role_ModuleMenu> topModuleMenus = new List<V_Sys_Role_ModuleMenu>();
            topModuleMenus = Permission.FindAll(c => c.Parent == null || c.Parent == Guid.Empty);
            if (topModuleMenus != null)
            {
                topModuleMenus.OrderBy(c => c.Zorder);
            }
            return topModuleMenus;
        }



        /// <summary>
        /// 当前模块权限菜单
        /// </summary>
        public V_Sys_Role_ModuleMenu GetCurrentModuleMenu()
        {
            foreach (var item in Permission)
            {
                item.IsOpen = false;
                item.IsActive = false;
            }
            var isOpen = true;
            var moduleMenu = GetCurrentPagePermission();
            if (moduleMenu.Type == 2)
            {
                isOpen = false;
            }
            moduleMenu.IsActive = true;
            while (true)
            {
                if (moduleMenu.Parent == null || moduleMenu.Parent == Guid.Empty)
                { break; }
                moduleMenu = Permission.Find(c => c.KeyVGUID == moduleMenu.Parent.Value);
                moduleMenu.IsOpen = isOpen;
            }
            if (moduleMenu != null)
            {
                moduleMenu.ChildModuleMenu = Permission.FindAll(c => c.Parent == moduleMenu.KeyVGUID && c.Type != 2);
                SerachModuleMenu(moduleMenu.ChildModuleMenu);
            }
            return moduleMenu;
        }
        ///// <summary>
        ///// 当前模块权限菜单
        ///// </summary>
        //public V_Sys_Role_ModuleMenu GetCurrentModuleMenu(V_Sys_Role_ModuleMenu roleModuleMenu)
        //{
        //    var moduleMenu = roleModuleMenu;
        //    while (moduleMenu.ParentModuleMenu != null)
        //    {
        //        moduleMenu = moduleMenu.ParentModuleMenu;
        //    }
        //    return moduleMenu;
        //}
        /// <summary>
        /// 当前页面权限
        /// </summary>
        public V_Sys_Role_ModuleMenu GetCurrentPagePermission()
        {
            V_Sys_Role_ModuleMenu moduleMenu = null;
            V_Sys_Role_ModuleMenu containModuleMenu = null;
            V_Sys_Role_ModuleMenu equalsModuleMenu = null;
            string url = HttpContext.Current.Request.Url.PathAndQuery;

            foreach (var item in Permission)
            {
                if (item.Url != null && item.Url != "" && item.Parent != null && item.Parent != Guid.Empty)
                {
                    string[] splitUrls = item.Url.Split(',');
                    if (splitUrls.Length > 0)
                    {
                        foreach (var splitUrl in splitUrls)
                        {
                            if (containModuleMenu == null && url.Contains(splitUrl))
                            {
                                containModuleMenu = item;
                            }
                            if (equalsModuleMenu == null && url == splitUrl)
                            {
                                equalsModuleMenu = item;
                                break;
                            }
                        }
                    }
                }
                if (equalsModuleMenu != null)
                {
                    break;
                }
            }
            moduleMenu = equalsModuleMenu != null ? equalsModuleMenu : containModuleMenu;
            return moduleMenu;
            //return Permission.Find(c => c.Url != null && c.Url != "" && url.Contains(c.Url) && c.Parent != null && c.Parent != Guid.Empty);
        }

        public void SerachModuleMenu(List<V_Sys_Role_ModuleMenu> moduleMenus)
        {
            foreach (var item in moduleMenus)
            {
                item.ChildModuleMenu = Permission.FindAll(c => c.Parent == item.KeyVGUID && c.Type != 2);
                if (item.ChildModuleMenu != null && item.ChildModuleMenu.Count > 0)
                {
                    SerachModuleMenu(item.ChildModuleMenu);
                }
            }
        }
    }
}
