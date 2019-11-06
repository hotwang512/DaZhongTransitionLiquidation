using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.ModuleMenu
{
    public class ModuleMenuController : BaseController
    {
        public ModuleMenuController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: SystemManagement/ModuleMenu
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetModuleMenu()
        {
            //var jsonResult = new JsonResultModel<Sys_ModuleMenu>();
            List<Sys_ModuleMenu> moduleMenus = new List<Sys_ModuleMenu>();
            DbService.Command(db =>
            {
                moduleMenus = db.Queryable<Sys_ModuleMenu>().OrderBy(z => z.Zorder).ToList();
            });

            List<Sys_ModuleMenu> topModuleMenus = moduleMenus.FindAll(c => c.Parent == null || c.Parent == Guid.Empty);
            topModuleMenus = topModuleMenus.OrderBy(c => c.Zorder).ToList();
            foreach (var item in topModuleMenus)
            {
                SearchModuleMenus(item, moduleMenus);
            }
            //jsonResult.Rows = moduleMenus;
            return Json(topModuleMenus, JsonRequestBehavior.AllowGet);
        }

        public void SearchModuleMenus(Sys_ModuleMenu moduleMenu, List<Sys_ModuleMenu> moduleMenus)
        {
            var children = moduleMenus.FindAll(c => c.Parent == moduleMenu.VGUID);
            moduleMenu.children = children.OrderBy(c => c.Zorder).ToList();
            if (moduleMenu.children != null && moduleMenu.children.Count > 0)
            {
                foreach (var item in moduleMenu.children)
                {
                    SearchModuleMenus(item, moduleMenus);
                }
            }
        }

        public JsonResult DeleteModule(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };

            DbService.Command(db =>
            {
                var data = db.Queryable<Sys_ModuleMenu>();
                foreach (var item in vguids)
                {
                    //int saveChanges = 1;
                    Delete(item);
                    resultModel.IsSuccess = true;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public void Delete(Guid vguid)
        {
            DbService.Command(db =>
            {
                var datas = db.Queryable<Sys_ModuleMenu>();
                var isAnyParent = datas.Where(x => x.Parent == vguid).ToList();
                if (isAnyParent.Count > 0)
                {
                    foreach (var item in isAnyParent)
                    {
                        Delete(item.VGUID);
                    }
                    //db.Deleteable<Business_SevenSection>(x => x.ParentCode == code && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ExecuteCommand();
                }
                else
                {
                    db.Deleteable<Sys_ModuleMenu>(x => x.VGUID == vguid).ExecuteCommand();
                }
            });
        }
        public JsonResult SaveModules(Sys_ModuleMenu module, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (!isEdit)
            {
                module.VGUID = Guid.NewGuid();
            }
            DbService.Command(db =>
            {
                var IsSuccess = "0";
                var result = db.Ado.UseTran(() =>
                {
                    var guid = module.VGUID;
                    var moduleName = module.Name;
                    var parent = module.Parent;
                    var isAny = db.Queryable<Sys_ModuleMenu>().Any(x => x.Name == moduleName && x.Parent == parent && x.VGUID != guid);
                    if (isAny)
                    {
                        IsSuccess = "2";
                        return;
                    }
                    //var isAnyParent = db.Queryable<Sys_Module>().Any(x => x.Parent == parent);
                    //if (!isAnyParent && parent != Guid.Empty && !isEdit)
                    //{
                    //    IsSuccess = "3";
                    //    return;
                    //}
                    if (isEdit)
                    {
                        db.Updateable<Sys_ModuleMenu>().UpdateColumns(it => new Sys_ModuleMenu()
                        {
                            Name = module.Name,
                            Type = module.Type,
                            Url = module.Url,
                            Zorder = module.Zorder
                        }).Where(it => it.VGUID == guid).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(module).IgnoreColumns(it => new { it.children }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                if (IsSuccess == "2" || IsSuccess == "3")
                {
                    resultModel.Status = IsSuccess;
                }
            });
            return Json(resultModel);
        }
        public JsonResult MoveMenu(Guid VGUID, Guid? Parent)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbService.Command(db =>
            {
                var data = db.Queryable<Sys_ModuleMenu>().Where(x => x.VGUID == VGUID).ToList().FirstOrDefault();
                if (data.Parent == Parent)
                {
                    resultModel.Status = "2";
                }
                else
                {
                    db.Updateable<Sys_ModuleMenu>().UpdateColumns(it => new Sys_ModuleMenu()
                    {
                        Parent = Parent,
                    }).Where(it => it.VGUID == VGUID).ExecuteCommand();
                    resultModel.Status = "1";
                }
            });
            return Json(resultModel);
        }
        public JsonResult UpdataCheckBox(Guid vguid, string field)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbService.Command(db =>
            {
                var status = db.Ado.SqlQuery<bool>(@"select " + field + " from Sys_ModuleMenu where VGUID = '" + vguid + "'").FirstOrDefault();
                if (status)
                {
                    //原值为true更新成false
                    db.Ado.SqlQuery<dynamic>(@"update Sys_ModuleMenu set " + field + "='0'  where VGUID = '" + vguid + "'");
                }
                else
                {
                    db.Ado.SqlQuery<dynamic>(@"update Sys_ModuleMenu set " + field + "='1'  where VGUID = '" + vguid + "'");
                }
            });
            return Json(resultModel);
        }
    }
}