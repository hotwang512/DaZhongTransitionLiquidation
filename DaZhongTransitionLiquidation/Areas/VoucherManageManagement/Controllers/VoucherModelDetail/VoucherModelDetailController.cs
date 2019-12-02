using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherModelDetail
{
    public class VoucherModelDetailController : BaseController
    {
        public VoucherModelDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/VoucherModelDetail
        public ActionResult Index()
        {
            ViewBag.GetGuid = Guid.NewGuid().TryToString();
            return View();
        }
        public JsonResult GetVoucherModelDetail(Guid vguid)
        {
            Business_VoucherModel orderList = new Business_VoucherModel();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_VoucherModel>().Single(x => x.VGUID == vguid);
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
        public JsonResult SaveVoucherModelDetail(Business_VoucherModel sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var isAnyName = db.Queryable<Business_VoucherModel>().Any(x => x.ModelName == sevenSection.ModelName && x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode == UserInfo.CompanyCode && x.VGUID != sevenSection.VGUID);
                    if (isAnyName)
                    {
                        resultModel.Status = "2";
                    }
                    else
                    {
                        var isAny = db.Queryable<Business_VoucherModel>().Any(x => x.VGUID == sevenSection.VGUID);
                        if (!isAny)
                        {
                            db.Insertable(sevenSection).ExecuteCommand();
                        }
                        else
                        {
                            db.Updateable(sevenSection).ExecuteCommand();
                        }
                        resultModel.Status = "1";
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
            });
            return Json(resultModel);
        }
    }
}