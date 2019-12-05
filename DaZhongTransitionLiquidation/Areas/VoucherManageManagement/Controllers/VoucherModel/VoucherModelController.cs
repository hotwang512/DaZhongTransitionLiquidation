using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherModel
{
    public class VoucherModelController : BaseController
    {
        public VoucherModelController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: VoucherManageManagement/VoucherModel
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetVoucherModelData(string ModelName, DateTime? AccountingPeriod, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_VoucherModel>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_VoucherModel>()
                .WhereIF(!string.IsNullOrEmpty(ModelName), i => i.ModelName.Contains(ModelName))
                .Where(x=>x.AccountModeCode == UserInfo.AccountModeCode && x.CompanyCode ==UserInfo.CompanyCode)
                .OrderBy(i => i.ModelName, OrderByType.Asc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteVoucherModel(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = db.Deleteable<Business_VoucherModel>(vguids).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}