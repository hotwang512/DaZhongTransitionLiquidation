using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetsMaintenance
{
    public class AssetsMaintenanceController : BaseController
    {
        public AssetsMaintenanceController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetManagement/AssetsMaintenance
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetAssetMaintenanceInfoListDatas(Business_AssetMaintenanceInfo searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_AssetMaintenanceInfo>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_AssetMaintenanceInfo>()
                .WhereIF(searchParams.TAG_NUMBER != null, i => i.TAG_NUMBER.Contains(searchParams.TAG_NUMBER))
                .WhereIF(searchParams.ASSET_CATEGORY_MAJOR != null, i => i.ASSET_CATEGORY_MAJOR.Contains(searchParams.ASSET_CATEGORY_MAJOR))
                .WhereIF(searchParams.ASSET_CATEGORY_MINOR != null, i => i.ASSET_CATEGORY_MINOR.Contains(searchParams.ASSET_CATEGORY_MINOR))
                //.WhereIF(searchParams.STATUS != null, i => i.STATUS == searchParams.STATUS)
                .OrderBy(i => i.CREATE_DATE, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteAssetMaintenanceInfo(List<Guid> vguids)//Guid[] vguids
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //删除主表信息
                saveChanges = db.Deleteable<Business_AssetMaintenanceInfo>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == vguids.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        public JsonResult GetBusinessModel()
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var list = db.Queryable<Business_AssetMaintenanceInfo>().ToList();
                //获取所有的经营模式
                var manageModelList = db.Queryable<Business_ManageModel>().ToList();
                //获取所有的公司
                var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                foreach (var item in list)
                {
                    //车龄 月末时间减去上牌时间（计算两个时间的月数，可能有小数点，保留整位）
                    var months = ((DateTime.Now.Year - item.LISENSING_DATE.TryToDate().Year) * 12) + (DateTime.Now.Month - item.LISENSING_DATE.TryToDate().Month);
                    if (!item.MODEL_MINOR.IsNullOrEmpty())
                    {
                        //经营模式子类 传过来的经营模式上级
                        var minor = manageModelList.FirstOrDefault(x => x.BusinessName == item.MODEL_MINOR);
                        //根据车龄判断经营模式子类
                        //如果经营模式子类有多个
                        if (minor != null && manageModelList.Count(x => x.VGUID == minor.ParentVGUID) > 1)
                        {
                            item.MODEL_MINOR = manageModelList.Where(x => x.VGUID == minor.ParentVGUID && x.VehicleAge > months).OrderBy(x => x.VehicleAge).First().BusinessName;
                        }
                        else if (minor != null && manageModelList.Count(x => x.VGUID == minor.ParentVGUID) == 1)
                        {
                            item.MODEL_MINOR = manageModelList
                                .First(x => x.VGUID == minor.ParentVGUID).BusinessName;
                        }
                        //经营模式主类 传过来的经营模式上上级
                        var major = manageModelList.FirstOrDefault(x => x.BusinessName == item.MODEL_MINOR);
                        item.MODEL_MAJOR = manageModelList
                            .First(x => major != null && x.VGUID == major.ParentVGUID).BusinessName;
                    }
                    item.VEHICLE_AGE = months;
                    var temp = "";
                    temp = item.BELONGTO_COMPANY;
                    item.BELONGTO_COMPANY = item.MANAGEMENT_COMPANY;
                    item.MANAGEMENT_COMPANY = temp;
                    //存在197,480的情况
                    if (item.BELONGTO_COMPANY.TryToInt() < 56)
                    {
                        item.BELONGTO_COMPANY =
                            ssList.First(x => x.OrgID == item.BELONGTO_COMPANY).Abbreviation;
                    }
                    item.MANAGEMENT_COMPANY =
                        ssList.First(x => x.OrgID == item.MANAGEMENT_COMPANY).Descrption;
                    var PurchaseGoodsInfo = db.Queryable<Business_PurchaseOrderSetting>()
                        .Where(x => x.PurchaseGoods == "出租车").First();
                    var AssetsCategory = db.Queryable<Business_AssetsCategory>()
                        .Where(x => x.VGUID == PurchaseGoodsInfo.AssetCategoryMinorVguid).First();
                    item.ASSET_CATEGORY_MAJOR = AssetsCategory.ASSET_CATEGORY_MAJOR;
                    item.ASSET_CATEGORY_MINOR = AssetsCategory.ASSET_CATEGORY_MINOR;
                    item.LIFE_YEARS = AssetsCategory.LIFE_YEARS;
                    item.LIFE_MONTHS = AssetsCategory.LIFE_MONTHS;
                    item.SALVAGE_PERCENT = AssetsCategory.SALVAGE_PERCENT;
                    item.METHOD = AssetsCategory.METHOD;
                    item.BOOK_TYPE_CODE = AssetsCategory.BOOK_TYPE_CODE;
                    item.ASSET_COST_ACCOUNT = AssetsCategory.ASSET_COST_ACCOUNT;
                    item.ASSET_SETTLEMENT_ACCOUNT = AssetsCategory.ASSET_SETTLEMENT_ACCOUNT;
                    item.DEPRECIATION_EXPENSE_SEGMENT = AssetsCategory.DEPRECIATION_EXPENSE_SEGMENT;
                    item.ACCT_DEPRECIATION_ACCOUNT = AssetsCategory.ACCT_DEPRECIATION_ACCOUNT;
                    //item.DESCRIPTION = item.VEHICLE_SHORTNAME;
                    var ssModel = db.Queryable<Business_SevenSection>().Where(x =>
                        x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.Abbreviation == item.BELONGTO_COMPANY).First();
                    var accountMode = db.Queryable<Business_SevenSection>().Where(x =>
                        x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" &&
                        x.Code == ssModel.AccountModeCode).First().Descrption;
                    item.EXP_ACCOUNT_SEGMENT = accountMode;
                }
                db.Updateable<Business_AssetMaintenanceInfo>(list)
                    .UpdateColumns(x => new
                    {
                        x.MODEL_MINOR, x.MODEL_MAJOR,x.VEHICLE_AGE,x.BELONGTO_COMPANY,x.MANAGEMENT_COMPANY,
                        x.ASSET_CATEGORY_MAJOR,
                        x.ASSET_CATEGORY_MINOR,
                        x.LIFE_YEARS,
                        x.LIFE_MONTHS,
                        x.SALVAGE_PERCENT,
                        x.METHOD,
                        x.BOOK_TYPE_CODE,
                        x.ASSET_COST_ACCOUNT,
                        x.ASSET_SETTLEMENT_ACCOUNT,
                        x.DEPRECIATION_EXPENSE_SEGMENT,
                        x.ACCT_DEPRECIATION_ACCOUNT
                    }).ExecuteCommand();
                resultModel.IsSuccess = true;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}