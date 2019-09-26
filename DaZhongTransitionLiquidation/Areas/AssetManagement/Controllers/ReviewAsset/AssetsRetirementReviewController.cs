using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.ReviewAsset
{
    public class AssetsRetirementReviewController : BaseController
    {
        // GET: AssetManagement/AssetsRetirementReview
        public AssetsRetirementReviewController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("b7d38059-c04f-45fc-86ce-835dbd44315f");
            return View();
        }
        public JsonResult GetReviewAssetListDatas(Boolean ISVerify, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_ScrapVehicleShowModel>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                //var date = "2019-09-01".TryToDate();
                jsonResult.Rows = db.SqlQueryable<Business_ScrapVehicleShowModel>(@"select mi.TAG_NUMBER
                         , mi.VEHICLE_SHORTNAME
                         , mi.MANAGEMENT_COMPANY
                         , mi.BELONGTO_COMPANY
                         , mi.ENGINE_NUMBER
                         , mv.*
	                     , mi.EXP_ACCOUNT_SEGMENT
	                     , mi.COMMISSIONING_DATE as PERIOD
	                     , mi.QUANTITY
	                     , mi.ASSET_COST
	                     , mi.ASSET_ID
	                     , mi.GROUP_ID
	                     , mi.BOOK_TYPE_CODE
						 , mi.LISENSING_DATE
                    from Business_ScrapVehicle mv
                        left join Business_AssetMaintenanceInfo mi
                            on mv.ORIGINALID = mi.ORIGINALID")
                    .Where(i => !i.ISVERIFY)
                    //.Where(x => x.BACK_CAR_DATE <= date)
                    //.WhereIF(!searchParams.PLATE_NUMBER.IsNullOrEmpty(), i => i.PLATE_NUMBER.Contains(searchParams.PLATE_NUMBER))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToList();
                if (ISVerify)
                {
                    //校验数据
                    foreach (var item in jsonResult.Rows)
                    {
                        if (item.BOOK_TYPE_CODE.IsNullOrEmpty() ||
                            item.ASSET_COST.IsNullOrEmpty() || item.BACK_CAR_DATE.IsNullOrEmpty() ||
                            item.ASSET_ID.IsNullOrEmpty())
                        {
                            item.GROUP_ID = "1";
                        }
                    }
                }
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubmitRetirementVehicleReview(List<Guid> guids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var cache = CacheManager<Sys_User>.GetInstance();
                    var modifyVehicleList = db.SqlQueryable<Business_ScrapVehicleModel>(@"select sv.*
                         , mi.ASSET_ID
                         , mi.BELONGTO_COMPANY
                         , mi.DESCRIPTION
                         , mi.ASSET_COST
                         , mi.LISENSING_DATE
                         , mi.BOOK_TYPE_CODE
                         , mi.COMMISSIONING_DATE as PERIOD
                    from Business_ScrapVehicle                  sv
                        left join Business_AssetMaintenanceInfo mi
                            on sv.ORIGINALID = mi.ORIGINALID").Where(x => guids.Contains(x.VGUID)).ToList();
                    //获取所有的经营模式
                    var assetSwapList = new List<AssetMaintenanceInfo_Swap>();
                    var assetDisposeIncomeList = new List<Business_DisposeIncome>();
                    var assetDisposeNetValueList = new List<Business_DisposeNetValue>();
                    //var assetDisposeTaxList = new List<Business_DisposeTax>();
                    var assetDisposeProfitLossList = new List<Business_DisposeProfitLoss>();
                    foreach (var item in modifyVehicleList)
                    {
                        //先更新资产维护表，再写入Oracle 中间表
                        //计算车龄
                        var age = ((item.BACK_CAR_DATE.Year - item.LISENSING_DATE.Year) * 12) + item.BACK_CAR_DATE.Month - item.LISENSING_DATE.Month;
                        db.Updateable<Business_AssetMaintenanceInfo>()
                            .UpdateColumns(x => new Business_AssetMaintenanceInfo { BACK_CAR_DATE = item.BACK_CAR_DATE, VEHICLE_AGE = age }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                        db.Updateable<Business_ScrapVehicle>()
                            .UpdateColumns(x => new Business_ScrapVehicle { ISVERIFY = true}).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                        var assetSwapModel = new AssetMaintenanceInfo_Swap();
                        assetSwapModel.TRANSACTION_ID = item.VGUID;
                        assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                        assetSwapModel.CREATE_DATE = DateTime.Now;
                        assetSwapModel.ASSET_ID = item.ASSET_ID;
                        assetSwapModel.STATUS = "N";
                        assetSwapModel.RETIRE_DATE = item.BACK_CAR_DATE;
                        assetSwapModel.RETIRE_FLAG = "Y";
                        assetSwapModel.RETIRE_QUANTITY = 1;
                        assetSwapModel.RETIRE_COST = item.ASSET_COST;
                        //var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                        //    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Abbreviation == item.BELONGTO_COMPANY).First();
                        //assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                        //assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                        //assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR;
                        //assetSwapModel.MODEL_MINOR = item.MODEL_MINOR;
                        assetSwapModel.PERIOD = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2,'0');
                        assetSwapModel.BOOK_TYPE_CODE = item.BOOK_TYPE_CODE;
                        assetSwapModel.PROCESS_TYPE = "RETIRE";
                        assetSwapModel.CHECK_STATE = false;
                        assetSwapList.Add(assetSwapModel);
                        //提交到处置收入
                        var assetInfo = db.Queryable<Business_AssetMaintenanceInfo>()
                            .Where(x => x.ORIGINALID == item.ORIGINALID).First();
                        var disposeIncome = new Business_DisposeIncome();
                        disposeIncome.VGUID = Guid.NewGuid();
                        disposeIncome.DepartmentVehiclePlateNumber = assetInfo.PLATE_NUMBER;
                        //disposeIncome.OraclePlateNumber = assetInfo.PLATE_NUMBER;
                        disposeIncome.VehicleModel = assetInfo.VEHICLE_SHORTNAME;
                        disposeIncome.BelongToCompany = assetInfo.BELONGTO_COMPANY;
                        disposeIncome.ManageCompany = assetInfo.MANAGEMENT_COMPANY;
                        disposeIncome.CommissioningDate = assetInfo.COMMISSIONING_DATE;
                        disposeIncome.BackCarDate = assetInfo.BACK_CAR_DATE;
                        disposeIncome.BackCarAge = assetInfo.BACK_CAR_DATE.TryToDate().Year * 12 + assetInfo.BACK_CAR_DATE.TryToDate().Month - assetInfo.COMMISSIONING_DATE.TryToDate().Year * 12 - assetInfo.COMMISSIONING_DATE.TryToDate().Month; ;
                        disposeIncome.CreateDate = DateTime.Now;
                        disposeIncome.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        disposeIncome.AssetID = assetInfo.ASSET_ID;
                        disposeIncome.BusinessModel = assetInfo.MODEL_MAJOR + "-" + assetInfo.MODEL_MINOR;
                        assetDisposeIncomeList.Add(disposeIncome);
                        //提交到处置税金
                        //var disposeTax = new Business_DisposeTax();
                        //disposeTax.VGUID = Guid.NewGuid();
                        //disposeTax.DepartmentVehiclePlateNumber = assetInfo.PLATE_NUMBER;
                        ////disposeIncome.OraclePlateNumber = assetInfo.PLATE_NUMBER;
                        //disposeTax.CreateDate = DateTime.Now;
                        //disposeTax.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        //assetDisposeTaxList.Add(disposeTax);
                        //提交到处置净值
                        var disposeNetValue = new Business_DisposeNetValue();
                        disposeNetValue.VGUID = Guid.NewGuid();
                        disposeNetValue.AssetID = assetInfo.ASSET_ID;
                        disposeNetValue.DepartmentVehiclePlateNumber = assetInfo.PLATE_NUMBER;
                        //disposeIncome.OraclePlateNumber = assetInfo.PLATE_NUMBER;
                        disposeNetValue.CreateDate = DateTime.Now;
                        disposeNetValue.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        assetDisposeNetValueList.Add(disposeNetValue);
                        //提交到处置损益

                        var disposeProfitLoss = new Business_DisposeProfitLoss();
                        disposeProfitLoss.VGUID = Guid.NewGuid();
                        disposeProfitLoss.DepartmentVehiclePlateNumber = assetInfo.PLATE_NUMBER;
                        disposeProfitLoss.AssetID = assetInfo.ASSET_ID;
                        //disposeIncome.OraclePlateNumber = assetInfo.PLATE_NUMBER;
                        disposeProfitLoss.CreateDate = DateTime.Now;
                        disposeProfitLoss.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        assetDisposeProfitLossList.Add(disposeProfitLoss);
                    }
                    db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                    db.Insertable<Business_DisposeIncome>(assetDisposeIncomeList).ExecuteCommand();
                    db.Insertable<Business_DisposeNetValue>(assetDisposeNetValueList).ExecuteCommand();
                    db.Insertable<Business_DisposeProfitLoss>(assetDisposeProfitLossList).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetScrapVehicleReview(string YearMonth)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    List<Api_ScrapVehicleAsset> assetScrapFlowList = new List<Api_ScrapVehicleAsset>();
                    //退车
                    {
                        var startDate = YearMonth.TryToDate();
                        var endDate  = startDate.AddMonths(1);
                        //var YearMonth = DateTime.Now.Year + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0');
                        var ORIGINALIDList = db.Queryable<Business_AssetMaintenanceInfo>()
                            .Where(x => x.BACK_CAR_DATE == null && x.GROUP_ID == "出租车").Select(x => new { x.ORIGINALID}).ToList();
                        var ORIGINALIDs = "";
                        foreach (var item in ORIGINALIDList)
                        {
                            ORIGINALIDs += item.ORIGINALID + ",";
                        }
                        ORIGINALIDs = ORIGINALIDs.Substring(0, ORIGINALIDs.Length - 1);
                        var apiReaultScrap = AssetMaintenanceAPI.GetScrapVehicleAsset(ORIGINALIDs);
                        var resultApiScrapModel = apiReaultScrap
                            .JsonToModel<JsonResultListApi<Api_VehicleAssetResult<string, string>>>();
                        var resultColumn = resultApiScrapModel.data[0].COLUMNS;
                        var resultData = resultApiScrapModel.data[0].DATA;
                        foreach (var item in resultData)
                        {
                            var nv = new Api_ScrapVehicleAsset();
                            var t = nv.GetType();
                            for (var k = 0; k < resultColumn.Count; k++)
                            {
                                var pi = t.GetProperty(resultColumn[k]);
                                if (pi != null) pi.SetValue(nv, item[k], null);
                            }
                            assetScrapFlowList.Add(nv);
                        }
                        assetScrapFlowList =
                            assetScrapFlowList.Where(x => x.BACK_CAR_DATE.Contains(YearMonth)).ToList();
                        var dt =assetScrapFlowList.TryToDataTable();
                        AutoSyncAssetsMaintenance.WirterScrapSyncAssetFlow(assetScrapFlowList);
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}