using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BusinessTypeSet;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers
{
    public class ManageModelController : BaseController
    {
        // GET: AssetManagement/ManageModel
        public ManageModelController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public ActionResult GetBusiness()
        {
            var jsonResult = new JsonResultModel<Business_ManageModel>();
            List<Business_ManageModel> Business_ManageModels = new List<Business_ManageModel>();
            DbBusinessDataService.Command(db =>
            {
                Business_ManageModels = db.Queryable<Business_ManageModel>().OrderBy(c => c.BusinessName).ToList();
            });
            jsonResult.Rows = Business_ManageModels;
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteBusiness(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };

            DbBusinessDataService.Command(db =>
            {
                var data = db.Queryable<Business_ManageModel>();
                foreach (var item in vguids)
                {
                    Delete(item);
                    resultModel.IsSuccess = true;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public void Delete(Guid vguid)
        {
            DbBusinessDataService.Command(db =>
            {
                var datas = db.Queryable<Business_ManageModel>();
                var isAnyParent = datas.Where(x => x.ParentVGUID == vguid).ToList();
                if (isAnyParent.Count > 0)
                {
                    foreach (var item in isAnyParent)
                    {
                        Delete(item.VGUID);
                    }
                }
                else
                {
                    db.Deleteable<Business_ManageModel>(x => x.VGUID == vguid).ExecuteCommand();
                }
            });
        }
        public JsonResult SaveBusiness(Business_ManageModel module, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (!isEdit)
            {
                module.Founder = UserInfo.LoginName;
                module.CreateTime = DateTime.Now;
                module.VGUID = Guid.NewGuid();
            }
            DbBusinessDataService.Command(db =>
            {
                var IsSuccess = "0";
                var result = db.Ado.UseTran(() =>
                {
                    var guid = module.VGUID;
                    var parentVGUID = module.ParentVGUID;
                    if (isEdit)
                    {
                        db.Updateable<Business_ManageModel>().UpdateColumns(it => new Business_ManageModel()
                        {
                            BusinessName = module.BusinessName,
                            ParentVGUID = parentVGUID,
                            CreateTime = DateTime.Now,
                            Founder = UserInfo.LoginName
                        }).Where(it => it.VGUID == guid).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(module).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}