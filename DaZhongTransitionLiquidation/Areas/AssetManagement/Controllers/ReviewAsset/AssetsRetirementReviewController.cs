using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
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
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetReviewAssetListDatas(Business_ScrapVehicle searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_ScrapVehicleShowModel>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.SqlQueryable<Business_ScrapVehicleShowModel>(@"select mi.TAG_NUMBER
                         , mi.VEHICLE_SHORTNAME
                         , mi.MANAGEMENT_COMPANY
                         , mi.BELONGTO_COMPANY
                         , mi.VEHICLE_STATE
                         , mi.OPERATING_STATE
                         , mi.ENGINE_NUMBER
                         , mi.CHASSIS_NUMBER
                         , mi.MODEL_MAJOR
                         , mi.MODEL_MINOR
                         , mv.*
	                     , mi.EXP_ACCOUNT_SEGMENT
	                     , mi.COMMISSIONING_DATE as PERIOD
	                     , mi.QUANTITY
	                     , mi.ASSET_COST
	                     , mi.ASSET_ID
                    from Business_ScrapVehicle mv
                        left join Business_AssetMaintenanceInfo mi
                            on mv.ORIGINALID = mi.ORIGINALID")
                    .Where(i => !i.ISVERIFY)
                    .WhereIF(!searchParams.PLATE_NUMBER.IsNullOrEmpty(), i => i.PLATE_NUMBER.Contains(searchParams.PLATE_NUMBER))
                    .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
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
                    var modifyVehicleList = db.SqlQueryable<Business_ScrapVehicleModel>(@"SELECT sv.*,mi.ASSET_ID,mi.BELONGTO_COMPANY,mi.MODEL_MAJOR,mi.MODEL_MINOR,mi.DESCRIPTION,mi.ASSET_COST,mi.LISENSING_DATE,mi.COMMISSIONING_DATE AS PERIOD    FROM Business_ScrapVehicle sv LEFT JOIN Business_AssetMaintenanceInfo mi ON sv.ORIGINALID = mi.ORIGINALID").Where(x => guids.Contains(x.VGUID)).ToList();
                    //获取所有的经营模式
                    var assetSwapList = new List<AssetMaintenanceInfo_Swap>();
                    var assetDisposeIncomeList = new List<Business_DisposeIncome>();
                    var assetDisposeNetValueList = new List<Business_DisposeNetValue>();
                    var assetDisposeTaxList = new List<Business_DisposeTax>();
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
                        //    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Descrption == item.BELONGTO_COMPANY).First();
                        //assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                        //assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                        //assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR;
                        //assetSwapModel.MODEL_MINOR = item.MODEL_MINOR;
                        assetSwapModel.PERIOD = item.PERIOD;
                        assetSwapModel.BOOK_TYPE_CODE = "营运公司2019";
                        assetSwapList.Add(assetSwapModel);
                        //提交到处置收入
                        var assetInfo = db.Queryable<Business_AssetMaintenanceInfo>()
                            .Where(x => x.ORIGINALID == item.ORIGINALID).First();
                        var disposeIncome = new Business_DisposeIncome();
                        disposeIncome.VGUID = Guid.NewGuid();
                        disposeIncome.DepartmentVehiclePlateNumber = assetInfo.PLATE_NUMBER;
                        //disposeIncome.OraclePlateNumber = assetInfo.PLATE_NUMBER;
                        disposeIncome.VehicleModel = assetInfo.VEHICLE_SHORTNAME;
                        disposeIncome.CommissioningDate = assetInfo.COMMISSIONING_DATE;
                        disposeIncome.BackCarDate = assetInfo.BACK_CAR_DATE;
                        disposeIncome.CreateDate = DateTime.Now;
                        disposeIncome.CreateUser = cache[PubGet.GetUserKey].UserName;
                        assetDisposeIncomeList.Add(disposeIncome);
                        //提交到处置税金
                        var disposeTax = new Business_DisposeTax();
                        disposeTax.VGUID = Guid.NewGuid();
                        disposeTax.DepartmentVehiclePlateNumber = assetInfo.PLATE_NUMBER;
                        //disposeIncome.OraclePlateNumber = assetInfo.PLATE_NUMBER;
                        disposeTax.CreateDate = DateTime.Now;
                        disposeTax.CreateUser = cache[PubGet.GetUserKey].UserName;
                        assetDisposeTaxList.Add(disposeTax);
                        //提交到处置净值
                        var disposeNetValue = new Business_DisposeNetValue();
                        disposeNetValue.VGUID = Guid.NewGuid();
                        disposeNetValue.DepartmentVehiclePlateNumber = assetInfo.PLATE_NUMBER;
                        //disposeIncome.OraclePlateNumber = assetInfo.PLATE_NUMBER;
                        disposeNetValue.CreateDate = DateTime.Now;
                        disposeNetValue.CreateUser = cache[PubGet.GetUserKey].UserName;
                        assetDisposeNetValueList.Add(disposeNetValue);
                        //提交到处置损益

                        var disposeProfitLoss = new Business_DisposeProfitLoss();
                        disposeProfitLoss.VGUID = Guid.NewGuid();
                        disposeProfitLoss.DepartmentVehiclePlateNumber = assetInfo.PLATE_NUMBER;
                        //disposeIncome.OraclePlateNumber = assetInfo.PLATE_NUMBER;
                        disposeProfitLoss.CreateDate = DateTime.Now;
                        disposeProfitLoss.CreateUser = cache[PubGet.GetUserKey].UserName;
                        assetDisposeProfitLossList.Add(disposeProfitLoss);

                    }
                    db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                    db.Insertable<Business_DisposeIncome>(assetDisposeIncomeList).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

    }
}