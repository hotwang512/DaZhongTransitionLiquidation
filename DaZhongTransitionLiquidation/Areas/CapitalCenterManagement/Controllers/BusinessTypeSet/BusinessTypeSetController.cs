using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BusinessTypeSet
{
    public class BusinessTypeSetController : BaseController
    {
        public BusinessTypeSetController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/BusinessTypeSet
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public ActionResult GetBusiness()
        {
            var jsonResult = new JsonResultModel<Business_BusinessTypeSet>();
            List<Business_BusinessTypeSet> Business_BusinessTypeSets = new List<Business_BusinessTypeSet>();
            DbBusinessDataService.Command(db =>
            {
                Business_BusinessTypeSets = db.Queryable<Business_BusinessTypeSet>().OrderBy(c => c.Code).ToList();
            });
            jsonResult.Rows = Business_BusinessTypeSets;
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteBusiness(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };

            DbBusinessDataService.Command(db =>
            {
                var data = db.Queryable<Business_BusinessTypeSet>();
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
            DbBusinessDataService.Command(db =>
            {
                var datas = db.Queryable<Business_BusinessTypeSet>();
                var isAnyParent = datas.Where(x => x.ParentVGUID == vguid.ToString()).ToList();
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
                    db.Deleteable<Business_BusinessTypeSet>(x => x.VGUID == vguid).ExecuteCommand();
                }
            });
        }
        public JsonResult SaveBusiness(Business_BusinessTypeSet module, bool isEdit)
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
                    var code = module.Code;
                    var parentVGUID = module.ParentVGUID;
                    var isAny = db.Queryable<Business_BusinessTypeSet>().Any(x => x.Code == code && x.VGUID != guid);
                    if (isAny)
                    {
                        IsSuccess = "2";
                        return;
                    }
                    if (isEdit)
                    {
                        db.Updateable<Business_BusinessTypeSet>().UpdateColumns(it => new Business_BusinessTypeSet()
                        {
                            Code = module.Code,
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
                    //列出组合插入业务配置表
                    //InsertOrderList();
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

        private void InsertOrderList()
        {
            DbBusinessDataService.Command(db =>
            {
                db.Ado.UseTran(() =>
                {
                    var data = db.Queryable<Business_BusinessTypeSet>().Where(x=>x.ParentVGUID != null).OrderBy("Code asc").ToList();
                    var czGuid = "C86CA480-74B7-415C-A8A4-741955627727";
                    var zlGuid = "C86CA480-74B7-415C-A8A4-741955627728";
                    foreach (var item in data)
                    {
                        GetNextItem(db, item, czGuid, zlGuid);
                    }
                });
            });
        }

        private void GetNextItem(SqlSugarClient db, Business_BusinessTypeSet item, string czGuid, string zlGuid)
        {
            var it = item.ParentVGUID == czGuid;
            if (it)
            {

            }
        }
    }
}