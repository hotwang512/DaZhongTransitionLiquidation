using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetDispose
{
    public class DisposeNetValueController : BaseController
    {
        public DisposeNetValueController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        // GET: AssetManagement/DisposeNetValue
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("fc52f562-f549-4bcd-b0a1-5eb2886e491d");
            return View();
        }
        public JsonResult GetAssetsDisposeNetValueListDatas(string PlateNumber, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_DisposeNetValue>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_DisposeNetValue>()
                    .Where(x => x.SubmitStatus == 0)
                    .WhereIF(PlateNumber != null, i => i.DepartmentVehiclePlateNumber.Contains(PlateNumber) || i.OraclePlateNumber.Contains(PlateNumber))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToList();
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAssetsRetirement()
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var RetirementList = db.Queryable<AssetsRetirement_Swap>().ToList();
                    var NetValueList = db.Queryable<Business_DisposeNetValue>().Where(x => x.SubmitStatus == 0).ToList();
                    foreach (var item in NetValueList)
                    {
                        var retirement = RetirementList.First(x => x.ASSET_ID == item.AssetID);
                        item.NetValue = Math.Abs(retirement.RETIRE_PL.TryToDecimal());
                        item.OriginalValue = retirement.RETIRE_COST;
                        item.OraclePlateNumber = retirement.TAG_NUMBER;
                        item.AcctDepreciation = retirement.RETIRE_ACCT_DEPRECIATION.TryToDecimal();
                    }
                    db.Updateable<Business_DisposeNetValue>(NetValueList).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult SubmitDisposeNetValue(List<Guid> guids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var DisposeProfitLossList = new List<Business_DisposeProfitLoss>();
                    var NetValueList = db.Queryable<Business_DisposeNetValue>().Where(x => x.SubmitStatus == 0 && guids.Contains(x.VGUID)).ToList();
                    foreach (var item in NetValueList)
                    {
                        var retirement = db.Queryable<Business_DisposeProfitLoss>().First(x => x.AssetID == item.AssetID);
                        retirement.NetValue = item.NetValue;
                        retirement.OraclePlateNumber = item.OraclePlateNumber;
                        DisposeProfitLossList.Add(retirement);
                        item.SubmitStatus = 1;
                    }
                    db.Updateable<Business_DisposeProfitLoss>(DisposeProfitLossList).ExecuteCommand();
                    db.Updateable<Business_DisposeNetValue>(NetValueList).UpdateColumns(x => new { x.SubmitStatus}).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}