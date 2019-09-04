using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.ModuleManagement
{
    public class ModuleManagementController : BaseController
    {
        public ModuleManagementController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        // GET: SystemManagement/Module
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.AuthorityManagement);
            return View();
        }

        public ActionResult GetModules()
        {
            var jsonResult = new JsonResultModel<Sys_Module>();
            List<Sys_Module> sys_Modules = new List<Sys_Module>();
            DbService.Command(db =>
            {
                sys_Modules = db.Queryable<Sys_Module>().OrderBy(c => c.Zorder).OrderBy(z=>z.CreatedDate).ToList();
                //var data = sys_Modules.Where(x => x.ModuleVGUID == Guid.Empty).ToList();
                //foreach (var item in data)
                //{
                //    item.ModuleVGUID = item.Vguid;
                //    db.Updateable(item).ExecuteCommand();
                //}
            });
            jsonResult.Rows = sys_Modules;
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteModule(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };

            DbService.Command(db =>
            {
                var data = db.Queryable<Sys_Module>();
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
                var datas = db.Queryable<Sys_Module>();
                var isAnyParent = datas.Where(x => x.Parent == vguid).ToList();
                if (isAnyParent.Count > 0)
                {
                    foreach (var item in isAnyParent)
                    {
                        Delete(item.Vguid);
                    }
                    //db.Deleteable<Business_SevenSection>(x => x.ParentCode == code && x.SectionVGUID == "B63BD715-C27D-4C47-AB66-550309794D43").ExecuteCommand();
                }
                else
                {
                    db.Deleteable<Sys_Module>(x => x.Vguid == vguid).ExecuteCommand();
                }
            });
        }
        public JsonResult SaveModules(Sys_Module module, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (!isEdit)
            {
                module.CreatedUser = UserInfo.LoginName;
                module.CreatedDate = DateTime.Now;
                module.Vguid = Guid.NewGuid();
                module.ModuleVGUID = module.Vguid;
                module.Reads = 1;
                module.Adds = 1;
                module.Edit = 1;
                module.Deletes = 1;
            }
            DbService.Command(db =>
            {
                var IsSuccess = "0";
                var result = db.Ado.UseTran(() =>
                {
                    var guid = module.Vguid;
                    var moduleName = module.ModuleName;
                    var parent = module.Parent;
                    var isAny = db.Queryable<Sys_Module>().Any(x => x.ModuleName == moduleName && x.Parent == parent && x.Vguid != guid);
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
                        db.Updateable<Sys_Module>().UpdateColumns(it => new Sys_Module()
                        {
                            Reads = 1,
                            Adds = 1,
                            Edit = 1,
                            Deletes = 1,
                            ModuleName = module.ModuleName,
                            Parent = module.Parent,
                            ModuleVGUID = module.Vguid,
                            ChangeDate = DateTime.Now,
                            ChangeUser = UserInfo.LoginName
                        }).Where(it => it.Vguid == guid).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(module).ExecuteCommand();
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
    }
}