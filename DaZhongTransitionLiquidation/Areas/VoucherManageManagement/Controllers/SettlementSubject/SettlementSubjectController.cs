using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.SettlementSubject
{
    public class SettlementSubjectController : BaseController
    {
        public SettlementSubjectController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/SettlementSubject
        public ActionResult Index()
        {
            ViewBag.AccountMode = GetAccountMode();
            return View();
        }
        public JsonResult GetSettlementSubject()
        {
            var jsonResult = new JsonResultModel<Business_SettlementSubject>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult.Rows = db.Queryable<Business_SettlementSubject>().OrderBy("Sort asc").ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSettlementData(string settlementVGUID)
        {
            var result = new List<Business_SettlementSubjectDetail>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SettlementSubjectDetail>().Where(x => x.SettlementVGUID == settlementVGUID).OrderBy("Borrow desc,AccountModeCode asc").ToList();
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveBusiness(Business_SettlementSubject module, bool isEdit)
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
                    var isAny = db.Queryable<Business_SettlementSubject>().Any(x => x.BusinessType == module.BusinessType && x.ParentVGUID == module.ParentVGUID && x.VGUID != guid);
                    if (isAny)
                    {
                        IsSuccess = "2";
                        return;
                    }
                    if (isEdit)
                    {
                        db.Updateable<Business_SettlementSubject>().UpdateColumns(it => new Business_SettlementSubject()
                        {
                            BusinessType = module.BusinessType,
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
                if (IsSuccess == "2")
                {
                    resultModel.Status = IsSuccess;
                }
            });
            return Json(resultModel);
        }
        public JsonResult DeleteBusiness(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };

            DbBusinessDataService.Command(db =>
            {
                var data = db.Queryable<Business_SettlementSubject>();
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
                var datas = db.Queryable<Business_SettlementSubject>();
                var isAnyParent = datas.Where(x => x.ParentVGUID == vguid).ToList();
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
                    db.Deleteable<Business_SettlementSubject>(x => x.VGUID == vguid).ExecuteCommand();
                }
            });
        }
        public JsonResult SaveSettlementSubject(Business_SettlementSubjectDetail bankChannel, bool isEdit)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            if (!isEdit)
            {
                bankChannel.VGUID = Guid.NewGuid();
            }
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (isEdit)
                    {
                        db.Updateable(bankChannel).ExecuteCommand();
                    }
                    else
                    {
                        db.Insertable(bankChannel).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                if (resultModel.Status != "2")
                {
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteSettlementSubject(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = db.Deleteable<Business_SettlementSubjectDetail>(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}