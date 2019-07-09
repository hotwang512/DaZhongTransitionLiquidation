using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
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
            var jsonResult = new JsonResultModel<Business_ScrapVehicle>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_ScrapVehicle>()
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
                    var modifyVehicleList = db.SqlQueryable<Business_ScrapVehicleModel>("@SELECT sv.*,mi.ASSET_ID,mi.BELONGTO_COMPANY,mi.MODEL_MAJOR,mi.MODEL_MINOR,mi.DESCRIPTION,mi.LISENSING_DATE,mi.START_VEHICLE_DATE AS PERIOD    FROM Business_ScrapVehicle sv LEFT JOIN Business_AssetMaintenanceInfo mi ON sv.ORIGINALID = mi.ORIGINALID").Where(x => guids.Contains(x.VGUID)).ToList();
                    //获取所有的经营模式
                    var assetSwapList = new List<AssetMaintenanceInfo_Swap>();
                    foreach (var item in modifyVehicleList)
                    {
                        //先更新资产维护表，再写入Oracle 中间表
                        //计算车龄
                        var age = ((item.BACK_CAR_DATE.Year - item.LISENSING_DATE.Year) * 12) + item.BACK_CAR_DATE.Month - item.LISENSING_DATE.Month;
                        db.Updateable<Business_AssetMaintenanceInfo>()
                            .UpdateColumns(x => new Business_AssetMaintenanceInfo { BACK_CAR_DATE = item.BACK_CAR_DATE, VEHICLE_AGE = age }).Where(i => i.ORIGINALID == item.ORIGINALID).ExecuteCommand();
                        var assetSwapModel = new AssetMaintenanceInfo_Swap();
                        assetSwapModel.TRANSACTION_ID = item.VGUID;
                        assetSwapModel.LAST_UPDATE_DATE = DateTime.Now;
                        assetSwapModel.CREATE_DATE = DateTime.Now;
                        assetSwapModel.ASSET_ID = item.ASSET_ID;
                        assetSwapModel.STATUS = "N";
                        assetSwapModel.RETIRE_DATE = item.BACK_CAR_DATE;
                        assetSwapModel.RETIRE_FLAG = "N";
                        assetSwapModel.RETIRE_QUANTITY = 1;
                        var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                            x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Descrption == item.BELONGTO_COMPANY).First();
                        assetSwapModel.ACCOUNTMODE_COMPANYCODE = ssModel.AccountModeCode + ssModel.Code;
                        assetSwapModel.VEHICLE_TYPE = item.DESCRIPTION;
                        assetSwapModel.MODEL_MAJOR = item.MODEL_MAJOR;
                        assetSwapModel.MODEL_MINOR = item.MODEL_MINOR;
                        assetSwapModel.PERIOD = item.PERIOD;
                        assetSwapModel.BOOK_TYPE_CODE = "营运公司2019";
                        assetSwapList.Add(assetSwapModel);
                    }
                    db.Insertable<AssetMaintenanceInfo_Swap>(assetSwapList).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

    }
}