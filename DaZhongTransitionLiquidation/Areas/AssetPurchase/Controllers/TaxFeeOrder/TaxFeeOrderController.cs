using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Models;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.TaxFeeOrder
{
    public class TaxFeeOrderController : BaseController
    {
        public TaxFeeOrderController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetPurchase/TaxFeeOrder
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetOrderListDatas(Business_TaxFeeOrder searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_TaxFeeOrder>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_TaxFeeOrder>()
                    .WhereIF(searchParams.VehicleModelCode != null, i => i.VehicleModelCode == searchParams.VehicleModelCode)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteTaxFeeOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                var isAnySubmited = db.Queryable<Business_TaxFeeOrder>().Any(c => vguids.Contains(c.VGUID) && (c.SubmitStatus != FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt()));
                if (isAnySubmited)
                {
                    resultModel.ResultInfo = "存在已提交的订单，订单提交后不允许删除";
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                }
                else
                {
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_TaxFeeOrder>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult SubmitTaxFeeOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    foreach (var vguid in vguids)
                    {
                        var model = db.Queryable<Business_TaxFeeOrder>().Where(c => c.VGUID == vguid).First();
                        if (model.SubmitStatus == FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt())
                        {
                            model.SubmitStatus = FixedAssetsSubmitStatusEnum.Submited.TryToInt();
                            model.SubmitDate = DateTime.Now;
                            model.SubmitUser = cache[PubGet.GetUserKey].UserName;
                            db.Updateable<Business_TaxFeeOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                        }
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess.ObjToBool() ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}