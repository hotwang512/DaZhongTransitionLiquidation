using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
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
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using Business_AssetOrderDetails = DaZhongTransitionLiquidation.Areas.AssetPurchase.Models.Business_AssetOrderDetails;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.PurchaseAssign
{
    public class PurchaseAssignController: BaseController
    {
        public PurchaseAssignController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        // GET: AssetPurchase/PurchaseAssign
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetBusiness_PurchaseAssignListDatas(Business_PurchaseAssign searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_PurchaseAssign>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.SqlQueryable<Business_PurchaseAssign>("SELECT pa.* FROM Business_PurchaseAssign pa LEFT JOIN Business_FixedAssetsOrder fao ON pa.FixedAssetsOrderVguid = fao.VGUID")//WHERE fao.SubmitStatus = 1
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAssetOrderDetails(string AssetType, Guid AssetsOrderVguid)
        {
            var listFixedAssetsOrder = new List<Business_AssetOrderDetails>();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                if (db.Queryable<Business_AssetOrderDetails>().Any(x => x.AssetsOrderVguid == AssetsOrderVguid))
                {
                    listFixedAssetsOrder = db.Queryable<Business_AssetOrderDetails>().Where(x => x.AssetsOrderVguid == AssetsOrderVguid).ToList();
                }
                else
                {
                    //根据采购物品获取资产管理公司
                    listFixedAssetsOrder = GetDefaultAssetOrderDetails(AssetsOrderVguid);
                }
            });
            return Json(listFixedAssetsOrder, JsonRequestBehavior.AllowGet); ;
        }
        public List<Business_AssetOrderDetails> GetDefaultAssetOrderDetails(Guid AssetsOrderVguid)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var list = new List<Business_AssetOrderDetails>();
            var listCompany = new List<string>();
            //获取采购物品配置的资产管理公司
            DbBusinessDataService.Command(db =>
            {
                //获取采购物品ID
                var PurchaseOrderSettingVguid = db.Queryable<Business_FixedAssetsOrder>()
                    .Where(c => c.VGUID == AssetsOrderVguid).First().PurchaseGoodsVguid;
                var listManagementCompany = db.Queryable<Business_PurchaseManagementCompany>()
                    .Where(c => c.PurchaseOrderSettingVguid == PurchaseOrderSettingVguid && c.IsCheck == true).ToList();
                foreach (var item in listManagementCompany)
                {
                    if (!db.Queryable<Business_AssetOrderDetails>()
                        .Any(c => c.AssetsOrderVguid == AssetsOrderVguid && c.AssetManagementCompany == item.ManagementCompany))
                    {
                        var model = new Business_AssetOrderDetails();
                        model.VGUID = Guid.NewGuid();
                        model.AssetsOrderVguid = AssetsOrderVguid;
                        model.CreateDate = DateTime.Now;
                        model.CreateUser = cache[PubGet.GetUserKey].UserName;
                        model.AssetManagementCompany = item.ManagementCompany;
                        list.Add(model);
                    }
                }
                if (list.Count > 0)
                {
                    db.Insertable<Business_AssetOrderDetails>(list).ExecuteCommand();
                }
            });
            return list.OrderBy(c => c.AssetManagementCompany).ToList();
        }
        public JsonResult GetOrderBelong(Guid AssetsOrderVguid)
        {
            var listFixedAssetsOrder = new List<Business_AssetOrderBelongToShow>();
            DbBusinessDataService.Command(db =>
            {
                listFixedAssetsOrder = db.SqlQueryable<Business_AssetOrderBelongToShow>(
                        @"SELECT belongto.*,assetsorder.PurchasePrices FROM (
                        SELECT SUM(AssetNum) AS AssetNum,BelongToCompany,AssetsOrderVguid FROM Business_AssetOrderBelongTo
                        WHERE AssetsOrderVguid =  '" + AssetsOrderVguid + @"'
                         GROUP BY BelongToCompany,AssetsOrderVguid) belongto LEFT JOIN dbo.Business_FixedAssetsOrder assetsorder ON
                         belongto.AssetsOrderVguid = assetsorder.VGUID")
                    .ToList();
            });
            return Json(listFixedAssetsOrder, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPurchaseAssign(Business_AssetOrderBelongTo searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_AssetOrderBelongTo>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_AssetOrderBelongTo>().Where(x => x.AssetOrderDetailsVguid == searchParams.AssetOrderDetailsVguid)
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBelongToCompany()
        {
            var list = new List<BelongToCompanyModel>();
            list.Add(new BelongToCompanyModel { BelongToCompany = "集团" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "虹口" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "奉贤" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "新亚" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "交运" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "万祥" });
            list.Add(new BelongToCompanyModel { BelongToCompany = "营管部" });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveBelongToRow(Guid? vguid, Guid AssetOrderDetailsVguid,Guid AssetsOrderVguid,int AssetNum, string BelongToCompany)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (!vguid.IsNullOrEmpty())
                    {
                        var saveObj = db.Queryable<Business_AssetOrderBelongTo>().Where(c => c.VGUID == vguid).First();
                        saveObj.ChangeDate = DateTime.Now;
                        saveObj.ChangeUser = cache[PubGet.GetUserKey].UserName;
                        saveObj.AssetNum = AssetNum;
                        saveObj.BelongToCompany = BelongToCompany;
                        db.Updateable(saveObj).IgnoreColumns(it => new { it.CreateDate,it.CreateUser }).ExecuteCommand();
                    }
                    else
                    {
                        var saveObj = new Business_AssetOrderBelongTo();
                        saveObj.VGUID = Guid.NewGuid();
                        saveObj.CreateDate = DateTime.Now;
                        saveObj.CreateUser = cache[PubGet.GetUserKey].UserName;
                        saveObj.AssetNum = AssetNum;
                        saveObj.BelongToCompany = BelongToCompany;
                        saveObj.AssetOrderDetailsVguid = AssetOrderDetailsVguid;
                        saveObj.AssetsOrderVguid = AssetsOrderVguid;
                        var orderDetails = db.Queryable<Business_AssetOrderDetails>().Where(c => c.VGUID == AssetOrderDetailsVguid).First();
                        saveObj.AssetManagementCompany = orderDetails.AssetManagementCompany;
                        db.Insertable<Business_AssetOrderBelongTo>(saveObj).ExecuteCommand();
                        var assign = db.Queryable<Business_PurchaseAssign>().Where(c => c.FixedAssetsOrderVguid == AssetsOrderVguid).First();
                        if (assign.SubmitStatus == 0)
                        {
                            assign.SubmitStatus = 1;
                            db.Updateable<Business_PurchaseAssign>(assign).UpdateColumns(x => new { x.SubmitStatus}).ExecuteCommand();
                        }
                    }
                    
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult DeleteBelongToRow(Guid vguid)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //删除主表信息
                saveChanges = db.Deleteable<Business_AssetOrderBelongTo>(x => x.VGUID == vguid).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult ImportAssignFile(Guid vguid, HttpPostedFileBase File)
        {
            var resultModel = new ResultModel<string,string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            if (File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "PurchaseAssign\\" + newFileName;
                var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath;
                try
                {
                    File.SaveAs(filePath);
                    var list = new List<Excel_PurchaseAssignModel>();
                    var dt = ExcelHelper.ExportToDataTable(filePath,true);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var assign = new Excel_PurchaseAssignModel();
                        assign.VehicleModel = dt.Rows[i][0].ToString();
                        assign.ChassisNumber = dt.Rows[i][1].ToString();
                        assign.EngineNumber = dt.Rows[i][2].ToString();
                        list.Add(assign);
                    }
                    //校验总数是否一致，校验管理公司是否一致
                    DbBusinessDataService.Command(db =>
                    {
                        var consistent = true;
                        var result = db.Ado.UseTran(() =>
                        {
                            //判断是否有已经发起支付的数据，如果有不允许导入
                            var fixedAssetsOrder = db.Queryable<Business_FixedAssetsOrder>()
                                .Where(c => c.VGUID == vguid).First();
                            var taxFeeOrderList = db.Queryable<Business_PurchaseOrderNum, Business_TaxFeeOrder>((pon, tfo) => new object[] {
                                    JoinType.Left,pon.FaxOrderVguid==tfo.VGUID}).Select((pon, tfo) => new { FixedAssetOrderVguid = pon.FixedAssetOrderVguid, FaxOrderVguid = pon.FaxOrderVguid, SubmitStatus = tfo.SubmitStatus })
                                .Where(x => x.FixedAssetOrderVguid == vguid).ToList();
                            if (fixedAssetsOrder.SubmitStatus >= 1 || taxFeeOrderList.Any(x => x.SubmitStatus >= 1))
                            {
                                consistent = false;
                                resultModel.ResultInfo = "已经有数据发起支付，不能重新导入 ";
                                return;
                            }
                            
                            if (fixedAssetsOrder.OrderQuantity != list.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo += "订单总数不一致 ";
                            }
                            //判断导入数据是否重复
                            var set = new HashSet<string>();
                            list.ForEach(x => {
                                set.Add(x.ChassisNumber + x.EngineNumber);
                            });
                            if (set.Count == list.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo = resultModel.ResultInfo + "Excel中有重复数据 ";
                            }
                            var purchaseAssign = db.Queryable<Business_PurchaseAssign>()
                                .Where(c => c.FixedAssetsOrderVguid == vguid).First();
                                //判断判断车架号发动机号，系统中没有才可以导入
                                var listAssetInfo = db.Queryable<Business_AssetMaintenanceInfo>()
                                .Select(x => new Excel_PurchaseAssignCompare { EngineNumber =  x.ENGINE_NUMBER, ChassisNumber = x.CHASSIS_NUMBER}).ToList();
                            var listExcel = list.Select(x => new Excel_PurchaseAssignCompare
                                {EngineNumber = x.EngineNumber, ChassisNumber = x.ChassisNumber}).ToList();
                            if (listAssetInfo.Intersect(listExcel).Any())
                            {
                                consistent = false;
                                resultModel.ResultInfo += "导入的数据系统中已存在 ";
                                return;
                            }
                            if (consistent)
                            {
                                //写入数据库
                                var sevenSectionList = new List<Business_AssetOrderBelongTo>();
                                    foreach (var item in list)
                                    {
                                        var belongTo = new Business_AssetOrderBelongTo();
                                        belongTo.VGUID = Guid.NewGuid();
                                        belongTo.VehicleModel = item.VehicleModel;
                                        belongTo.EngineNumber = item.EngineNumber;
                                        belongTo.ChassisNumber = item.ChassisNumber;
                                        belongTo.AssetNum = 1;
                                        belongTo.AssetsOrderVguid = vguid;
                                        belongTo.CreateDate = DateTime.Now;
                                        belongTo.CreateUser = cache[PubGet.GetUserKey].UserName;
                                        sevenSectionList.Add(belongTo);
                                    }
                                    db.Deleteable<Business_AssetOrderBelongTo>().Where(c => c.AssetsOrderVguid == vguid).ExecuteCommand();
                                    db.Insertable<Business_AssetOrderBelongTo>(sevenSectionList).ExecuteCommand();
                                    var orderNumData = db.Queryable<Business_PurchaseOrderNum>()
                                        .Where(x => x.FixedAssetOrderVguid == vguid).ToList();
                                    if (orderNumData.Count > 0)
                                    {
                                        foreach (var data in orderNumData)
                                        {
                                            db.Deleteable<Business_TaxFeeOrder>().Where(c => c.VGUID == data.FaxOrderVguid).ExecuteCommand();
                                        }
                                        db.Deleteable<Business_PurchaseOrderNum>().Where(c => c.FixedAssetOrderVguid == vguid).ExecuteCommand();
                                    }
                                    //车型
                                    var vehicleModelList = sevenSectionList.Select(x => new Vehicle_Model { VehicleModel = x.VehicleModel }).ToList();
                                    var vehicleModels = vehicleModelList.GroupBy(c => new { c.VehicleModel }).Select(c => c.Key).ToList();
                                    //根据车型获取各项费用单价并生成订单
                                    foreach (var vehicleModel in vehicleModels)
                                    {
                                        //根据车型获取各项费用的单价
                                        var feeList = db.Queryable<Business_VehicleExtrasFeeSetting>()
                                            .Where(x => x.VehicleModel == vehicleModel.VehicleModel && x.Status == 1 && x.Fee != 0).ToList();
                                        //采购数量
                                        var orderNum = vehicleModelList.Count(x => x.VehicleModel == vehicleModel.VehicleModel);
                                        //出库费
                                        foreach (var itemFee in feeList)
                                        {
                                            var feeOrder = new Business_TaxFeeOrder();
                                            feeOrder.PurchaseDepartmentIDs = "a8b4f808-33f3-454a-9a19-0b90d40aabea";//默认营运部
                                            feeOrder.PayItem = itemFee.BusinessSubItem.Split("|")[itemFee.BusinessSubItem.Split("|").Length - 1];
                                            feeOrder.PayItemCode = itemFee.BusinessSubItem;
                                            feeOrder.VehicleModel = vehicleModel.VehicleModel;
                                            feeOrder.VehicleModelCode = itemFee.VehicleModelCode;
                                            feeOrder.OrderQuantity = orderNum;
                                            feeOrder.UnitPrice = itemFee.Fee;
                                            feeOrder.SumPayment = orderNum * itemFee.Fee;
                                            feeOrder.SubmitStatus = 0;
                                            feeOrder.VGUID = Guid.NewGuid();
                                            feeOrder.CreateDate = DateTime.Now;
                                            feeOrder.CreateUser = "System";
                                            var orderNumberLeft = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                                            //查出当前日期数据库中最大的订单号
                                            var currentDayFixedAssetOrderList = db.Queryable<Business_FixedAssetsOrder>()
                                                .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new { c.OrderNumber }).ToList();
                                            var currentDayTaxFeeOrderList = db.Queryable<Business_TaxFeeOrder>()
                                                .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new { c.OrderNumber }).ToList();
                                            var currentDayIntangibleAssetsOrderList = db.Queryable<Business_IntangibleAssetsOrder>()
                                                .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new { c.OrderNumber }).ToList();
                                            var currentDayList = currentDayFixedAssetOrderList.Union(currentDayIntangibleAssetsOrderList).Union(currentDayTaxFeeOrderList).ToList();
                                            var maxOrderNumRight = 0;
                                            if (currentDayList.Any())
                                            {
                                                maxOrderNumRight = currentDayList.OrderBy(c => c.OrderNumber.Replace(orderNumberLeft, "").TryToInt()).First().OrderNumber.Replace(orderNumberLeft, "").TryToInt();
                                            }
                                            maxOrderNumRight = maxOrderNumRight + 1;
                                            feeOrder.OrderNumber = orderNumberLeft + maxOrderNumRight.ToString().PadLeft(4, '0');
                                            var purchaseOrderNum = new Business_PurchaseOrderNum();
                                            purchaseOrderNum.VGUID = Guid.NewGuid();
                                            purchaseOrderNum.PayItemCode = feeOrder.PayItemCode;
                                            purchaseOrderNum.PayItem = feeOrder.PayItem;
                                            purchaseOrderNum.OrderQuantity = feeOrder.OrderQuantity;
                                            purchaseOrderNum.FaxOrderVguid = feeOrder.VGUID;
                                            purchaseOrderNum.FixedAssetOrderVguid = vguid;
                                            purchaseOrderNum.CreateDate = DateTime.Now;
                                            purchaseOrderNum.CreateUser = "System";
                                            db.Insertable<Business_TaxFeeOrder>(feeOrder).ExecuteCommand();
                                            db.Insertable<Business_PurchaseOrderNum>(purchaseOrderNum).ExecuteCommand();
                                        }
                                    }
                                    //写入资产审核表
                                    //获取固定资产信息
                                    var fixedAssetsOrderInfo =
                                        db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == vguid).First();
                                    if (db.Queryable<Business_AssetReview>().Any(x => x.FIXED_ASSETS_ORDERID == vguid))
                                    {
                                        db.Deleteable<Business_AssetReview>().Where(c => c.FIXED_ASSETS_ORDERID == vguid).ExecuteCommand();
                                    }
                                    var belongToList = db.Queryable<Business_AssetOrderBelongTo>()
                                        .Where(x => x.AssetsOrderVguid == vguid).ToList();
                                    var orderNumberLeftAsset = "CZ" + DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                                    //查出当前日期数据库中最大的订单号
                                    var currentDayAssetReviewList = db.Queryable<Business_AssetReview>()
                                        .Where(c => c.ASSET_ID.StartsWith(orderNumberLeftAsset)).Select(c => new { c.ASSET_ID }).ToList();
                                    var maxOrderNumRightAsset = 0;
                                    if (currentDayAssetReviewList.Any())
                                    {
                                        maxOrderNumRightAsset = currentDayAssetReviewList.OrderBy(c => c.ASSET_ID.Replace(orderNumberLeftAsset, "").TryToInt()).First().ASSET_ID.Replace(orderNumberLeftAsset, "").TryToInt();
                                    }
                                    var assetReviewList = new List<Business_AssetReview>();
                                    foreach (var item in belongToList)
                                    {
                                        maxOrderNumRightAsset++;
                                        var assetReview = new Business_AssetReview();
                                        assetReview.VGUID = Guid.NewGuid();
                                        assetReview.ASSET_ID = orderNumberLeftAsset + maxOrderNumRightAsset.ToString().PadLeft(4, '0');
                                        assetReview.DESCRIPTION = item.VehicleModel;
                                        assetReview.ENGINE_NUMBER = item.EngineNumber;
                                        assetReview.CHASSIS_NUMBER = item.ChassisNumber;
                                        //assetReview.START_VEHICLE_DATE = fixedAssetsOrderInfo.LISENSING_DATE;
                                        assetReview.PURCHASE_DATE = fixedAssetsOrderInfo.CreateDate;
                                        assetReview.QUANTITY = 1;
                                        assetReview.NUDE_CAR_FEE = fixedAssetsOrderInfo.PurchasePrices;
                                        //车辆税费 根据车型取税费订单中对应的费用
                                        assetReview.PURCHASE_TAX = 0;
                                        assetReview.LISENSING_FEE = 0;
                                        assetReview.OUT_WAREHOUSE_FEE = 0;
                                        assetReview.DOME_LIGHT_FEE = 0;
                                        assetReview.ANTI_ROBBERY_FEE = 0;
                                        assetReview.LOADING_FEE = 0;
                                        assetReview.INNER_ROOF_FEE = 0;
                                        assetReview.TAXIMETER_FEE = 0;
                                        //资产主类次类 根据采购物品获取
                                        assetReview.ASSET_CATEGORY_MAJOR = db.Queryable<Business_PurchaseOrderSetting>()
                                            .Where(x => x.VGUID == fixedAssetsOrderInfo.PurchaseGoodsVguid).First()
                                            .AssetCategoryMajor;
                                        assetReview.ASSET_CATEGORY_MINOR = db.Queryable<Business_PurchaseOrderSetting>()
                                            .Where(x => x.VGUID == fixedAssetsOrderInfo.PurchaseGoodsVguid).First()
                                            .AssetCategoryMinor;
                                        //根据主类子类从折旧方法表中获取
                                        var assetsCategoryInfo = db.Queryable<Business_AssetsCategory>().Where(x =>
                                            x.ASSET_CATEGORY_MAJOR == assetReview.ASSET_CATEGORY_MAJOR &&
                                            x.ASSET_CATEGORY_MINOR == assetReview.ASSET_CATEGORY_MINOR).First();
                                        assetReview.LIFE_YEARS = assetsCategoryInfo.LIFE_YEARS;
                                        assetReview.LIFE_MONTHS = assetsCategoryInfo.LIFE_MONTHS;
                                        //残值类型 残值金额 摊销标记
                                        //assetReview.SALVAGE_TYPE = assetsCategoryInfo.SALVAGE_TYPE;
                                        //assetReview.SALVAGE_VALUE = assetsCategoryInfo.SALVAGE_VALUE;
                                        //assetReview.SALVAGE_PERCENT = assetsCategoryInfo.SALVAGE_PERCENT;
                                        assetReview.AMORTIZATION_FLAG = "N";
                                        assetReview.METHOD = assetsCategoryInfo.METHOD;
                                        assetReview.BOOK_TYPE_CODE = assetsCategoryInfo.BOOK_TYPE_CODE;
                                        assetReview.ASSET_COST_ACCOUNT = assetsCategoryInfo.ASSET_COST_ACCOUNT;
                                        assetReview.ASSET_SETTLEMENT_ACCOUNT = assetsCategoryInfo.ASSET_SETTLEMENT_ACCOUNT;
                                        assetReview.DEPRECIATION_EXPENSE_SEGMENT = assetsCategoryInfo.DEPRECIATION_EXPENSE_SEGMENT;
                                        assetReview.ACCT_DEPRECIATION_ACCOUNT = assetsCategoryInfo.ACCT_DEPRECIATION_ACCOUNT;
                                        assetReview.ISVERIFY = false;
                                        //var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                                        //     x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                                        //assetReview.MANAGEMENT_COMPANY = item.AssetManagementCompany;
                                        //assetReview.BELONGTO_COMPANY = item.BelongToCompany;
                                        //assetReview.MANAGEMENT_COMPANY_CODE = ssList.First(x => x.Abbreviation == item.AssetManagementCompany).OrgID;
                                        //assetReview.BELONGTO_COMPANY_CODE = ssList.First(x => x.Descrption == item.BelongToCompany).OrgID;
                                        ////总账帐簿 根据主信息中的资产所属公司获得后通过映射表转换
                                        //var accountModeCode = ssList.First(x => x.Descrption == assetReview.BELONGTO_COMPANY).AccountModeCode;
                                        //var accountModel = db.Queryable<Business_SevenSection>().Where(x =>
                                        //    x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == accountModeCode).First();
                                        //assetReview.EXP_ACCOUNT_SEGMENT = accountModel.Descrption;
                                        assetReview.YTD_DEPRECIATION = 0;
                                        assetReview.ACCT_DEPRECIATION = 0;
                                        assetReview.FIXED_ASSETS_ORDERID = vguid;
                                        assetReview.CREATE_USER = cache[PubGet.GetUserKey].UserName;
                                        assetReview.CREATE_DATE = DateTime.Now;
                                        assetReviewList.Add(assetReview);
                                    }
                                    db.Insertable<Business_AssetReview>(assetReviewList).ExecuteCommand();
                                    purchaseAssign.ChangeUser = cache[PubGet.GetUserKey].UserName;
                                    purchaseAssign.ChangeDate = DateTime.Now;
                                    purchaseAssign.SubmitStatus = 1;
                                    db.Updateable(purchaseAssign)
                                        .UpdateColumns(it => new { it.ChangeUser, it.ChangeDate, it.SubmitStatus }).ExecuteCommand();
                                resultModel.IsSuccess = true;
                                resultModel.Status = "1";
                            }
                            });
                        });
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(string.Format("Data:{0},result:{1}", filePath, ex.ToString()));
                    }
                }
                return Json(resultModel);
        }
        public JsonResult SubmitAssign(Guid vguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var purchaseAssign = db.Queryable<Business_PurchaseAssign>()
                        .Where(c => c.FixedAssetsOrderVguid == vguid).First();
                    //获取固定资产信息
                    var fixedAssetsOrderInfo =
                        db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == vguid).First();
                    //获取资产订单批次所有的车辆税费数据
                    var orderNumList = db.Queryable<Business_PurchaseOrderNum>().Where(x => x.FixedAssetOrderVguid == vguid).ToList();
                    if (orderNumList.Count > 0)
                    {
                        var isSubmit = true;
                        //根据税费订单获取车型
                        var faxVguid = orderNumList.First().FaxOrderVguid;
                        var vehicleModelCode = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == faxVguid).First().VehicleModelCode;
                        //判断是否已提交全部税费订单
                        if (!orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030104"))
                        {
                            //出库费  如果没有则需要根据车型到配置表中查看是否为0
                            if(!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x => x.Fee == 0 && x.VehicleModelCode == vehicleModelCode && x.BusinessSubItem == "cz|03|0301|030104" && x.Status == 1))
                            {
                                isSubmit = false;
                            }
                        }
                        if (!orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030103"))
                        {
                            //上牌费
                            if (!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x => x.Fee == 0 && x.VehicleModelCode == vehicleModelCode && x.BusinessSubItem == "cz|03|0301|030103" && x.Status == 1))
                            {
                                isSubmit = false;
                            }
                        }
                        if (!orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030107"))
                        {
                            //装车费
                            if (!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x => x.Fee == 0 && x.VehicleModelCode == vehicleModelCode && x.BusinessSubItem == "cz|03|0301|030107" && x.Status == 1))
                            {
                                isSubmit = false;
                            }
                        }
                        if (!orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030108"))
                        {
                            //内顶费
                            if (!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x => x.Fee == 0 && x.VehicleModelCode == vehicleModelCode && x.BusinessSubItem == "cz|03|0301|030108" && x.Status == 1))
                            {
                                isSubmit = false;
                            }
                        }
                        if (!orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030109"))
                        {
                            //计价器
                            if (!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x => x.Fee == 0 && x.VehicleModelCode == vehicleModelCode && x.BusinessSubItem == "cz|03|0301|030109" && x.Status == 1))
                            {
                                isSubmit = false;
                            }
                        }
                        if (!orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030106"))
                        {
                            //防劫板
                            if (!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x => x.Fee == 0 && x.VehicleModelCode == vehicleModelCode && x.BusinessSubItem == "cz|03|0301|030106" && x.Status == 1))
                            {
                                isSubmit = false;
                            }
                        }
                        if (!orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030105"))
                        {
                            //顶灯费
                            if (!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x => x.Fee == 0 && x.VehicleModelCode == vehicleModelCode && x.BusinessSubItem == "cz|03|0301|030105" && x.Status == 1))
                            {
                                isSubmit = false;
                            }
                        }
                        if (!orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030102"))
                        {
                            //购置税
                            if (!db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x => x.Fee == 0 && x.VehicleModelCode == vehicleModelCode && x.BusinessSubItem == "cz|03|0301|030102" && x.Status == 1))
                            {
                                isSubmit = false;
                            }
                        }
                        //如果各项费用都提交了
                        if (isSubmit)
                        {
                            //提交写入到资产审核表
                            if (db.Queryable<Business_AssetReview>().Any(x => x.FIXED_ASSETS_ORDERID == vguid))
                            {
                                db.Deleteable<Business_AssetReview>().Where(c => c.FIXED_ASSETS_ORDERID == vguid).ExecuteCommand();
                            }
                            var belongToList = db.Queryable<Business_AssetOrderBelongTo>()
                                .Where(x => x.AssetsOrderVguid == vguid).ToList();
                            var orderNumberLeft = "CZ" + DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                            //查出当前日期数据库中最大的订单号
                            var currentDayAssetReviewList = db.Queryable<Business_AssetReview>()
                                .Where(c => c.ASSET_ID.StartsWith(orderNumberLeft)).Select(c => new { c.ASSET_ID }).ToList();
                            var maxOrderNumRight = 0;
                            if (currentDayAssetReviewList.Any())
                            {
                                maxOrderNumRight = currentDayAssetReviewList.OrderBy(c => c.ASSET_ID.Replace(orderNumberLeft, "").TryToInt()).First().ASSET_ID.Replace(orderNumberLeft, "").TryToInt();
                            }
                            var assetReviewList = new List<Business_AssetReview>();
                            foreach (var item in belongToList)
                            {
                                maxOrderNumRight++;
                                var assetReview = new Business_AssetReview();
                                assetReview.VGUID = Guid.NewGuid();
                                assetReview.ASSET_ID = orderNumberLeft + maxOrderNumRight.ToString().PadLeft(4, '0');
                                assetReview.DESCRIPTION = item.VehicleModel;
                                assetReview.ENGINE_NUMBER = item.EngineNumber;
                                assetReview.CHASSIS_NUMBER = item.ChassisNumber;
                                assetReview.START_VEHICLE_DATE = item.StartVehicleDate;
                                assetReview.PURCHASE_DATE = fixedAssetsOrderInfo.CreateDate;
                                assetReview.QUANTITY = 1;
                                assetReview.NUDE_CAR_FEE = fixedAssetsOrderInfo.PurchasePrices;
                                //车辆税费 根据车型取税费订单中对应的费用
                                assetReview.PURCHASE_TAX = 0;
                                assetReview.LISENSING_FEE = 0;
                                assetReview.OUT_WAREHOUSE_FEE = 0;
                                assetReview.DOME_LIGHT_FEE = 0;
                                assetReview.ANTI_ROBBERY_FEE = 0;
                                assetReview.LOADING_FEE = 0;
                                assetReview.INNER_ROOF_FEE = 0;
                                assetReview.TAXIMETER_FEE = 0;
                                var faxOrderVguid = Guid.NewGuid();
                                if (orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030104"))
                                {
                                    //出库费
                                    faxOrderVguid = orderNumList.First(x => x.PayItemCode == "cz|03|0301|030104").FaxOrderVguid;
                                    assetReview.OUT_WAREHOUSE_FEE = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == faxOrderVguid).First().UnitPrice;
                                }
                                if (orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030103"))
                                {
                                    //上牌费
                                    faxOrderVguid = orderNumList.First(x => x.PayItemCode == "cz|03|0301|030103").FaxOrderVguid;
                                    assetReview.LISENSING_FEE = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == faxOrderVguid).First().UnitPrice;
                                }
                                if (orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030107"))
                                {
                                    //装车费
                                    faxOrderVguid = orderNumList.First(x => x.PayItemCode == "cz|03|0301|030107").FaxOrderVguid;
                                    assetReview.LOADING_FEE = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == faxOrderVguid).First().UnitPrice;
                                }
                                if (orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030108"))
                                {
                                    //内顶费
                                    faxOrderVguid = orderNumList.First(x => x.PayItemCode == "cz|03|0301|030108").FaxOrderVguid;
                                    assetReview.INNER_ROOF_FEE = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == faxOrderVguid).First().UnitPrice;
                                }
                                if (orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030109"))
                                {
                                    //计价器
                                    faxOrderVguid = orderNumList.First(x => x.PayItemCode == "cz|03|0301|030109").FaxOrderVguid;
                                    assetReview.TAXIMETER_FEE = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == faxOrderVguid).First().UnitPrice;
                                }
                                if (orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030106"))
                                {
                                    //防劫板
                                    faxOrderVguid = orderNumList.First(x => x.PayItemCode == "cz|03|0301|030106").FaxOrderVguid;
                                    assetReview.ANTI_ROBBERY_FEE = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == faxOrderVguid).First().UnitPrice;
                                }
                                if (orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030105"))
                                {
                                    //顶灯费
                                    faxOrderVguid = orderNumList.First(x => x.PayItemCode == "cz|03|0301|030105").FaxOrderVguid;
                                    assetReview.DOME_LIGHT_FEE = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == faxOrderVguid).First().UnitPrice;
                                }
                                if (orderNumList.Any(x => x.PayItemCode == "cz|03|0301|030102"))
                                {
                                    //购置税
                                    faxOrderVguid = orderNumList.First(x => x.PayItemCode == "cz|03|0301|030102").FaxOrderVguid;
                                    assetReview.PURCHASE_TAX = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == faxOrderVguid).First().UnitPrice;
                                }
                                assetReview.ASSET_COST = fixedAssetsOrderInfo.PurchasePrices + assetReview.PURCHASE_TAX + assetReview.LISENSING_FEE +
                                                        assetReview.OUT_WAREHOUSE_FEE + assetReview.DOME_LIGHT_FEE +
                                                        assetReview.ANTI_ROBBERY_FEE + assetReview.LOADING_FEE +
                                                        assetReview.INNER_ROOF_FEE + assetReview.TAXIMETER_FEE;
                                //资产主类次类 根据采购物品获取
                                assetReview.ASSET_CATEGORY_MAJOR = db.Queryable<Business_PurchaseOrderSetting>()
                                    .Where(x => x.VGUID == fixedAssetsOrderInfo.PurchaseGoodsVguid).First()
                                    .AssetCategoryMajor;
                                assetReview.ASSET_CATEGORY_MINOR = db.Queryable<Business_PurchaseOrderSetting>()
                                    .Where(x => x.VGUID == fixedAssetsOrderInfo.PurchaseGoodsVguid).First()
                                    .AssetCategoryMinor;
                                //根据主类子类从折旧方法表中获取
                                var assetsCategoryInfo = db.Queryable<Business_AssetsCategory>().Where(x =>
                                    x.ASSET_CATEGORY_MAJOR == assetReview.ASSET_CATEGORY_MAJOR &&
                                    x.ASSET_CATEGORY_MINOR == assetReview.ASSET_CATEGORY_MINOR).First();
                                assetReview.LIFE_YEARS = assetsCategoryInfo.LIFE_YEARS;
                                assetReview.LIFE_MONTHS = assetsCategoryInfo.LIFE_MONTHS;
                                //残值类型 残值金额 摊销标记
                                //assetReview.SALVAGE_TYPE = assetsCategoryInfo.SALVAGE_TYPE;
                                //assetReview.SALVAGE_VALUE = assetsCategoryInfo.SALVAGE_VALUE;
                                //assetReview.SALVAGE_PERCENT = assetsCategoryInfo.SALVAGE_PERCENT;
                                assetReview.AMORTIZATION_FLAG = "N";
                                assetReview.METHOD = assetsCategoryInfo.METHOD;
                                assetReview.BOOK_TYPE_CODE = assetsCategoryInfo.BOOK_TYPE_CODE;
                                assetReview.ASSET_COST_ACCOUNT = assetsCategoryInfo.ASSET_COST_ACCOUNT;
                                assetReview.ASSET_SETTLEMENT_ACCOUNT = assetsCategoryInfo.ASSET_SETTLEMENT_ACCOUNT;
                                assetReview.DEPRECIATION_EXPENSE_SEGMENT = assetsCategoryInfo.DEPRECIATION_EXPENSE_SEGMENT;
                                assetReview.ACCT_DEPRECIATION_ACCOUNT = assetsCategoryInfo.ACCT_DEPRECIATION_ACCOUNT;
                                assetReview.ISVERIFY = false;
                                var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                                     x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                                assetReview.MANAGEMENT_COMPANY = item.AssetManagementCompany;
                                assetReview.BELONGTO_COMPANY = item.BelongToCompany;
                                assetReview.MANAGEMENT_COMPANY_CODE = ssList.First(x => x.Abbreviation == item.AssetManagementCompany).OrgID;
                                assetReview.BELONGTO_COMPANY_CODE = ssList.First(x => x.Descrption == item.BelongToCompany).OrgID;
                                //总账帐簿 根据主信息中的资产所属公司获得后通过映射表转换
                                var accountModeCode = ssList.First(x => x.Descrption == assetReview.BELONGTO_COMPANY).AccountModeCode;
                                var accountModel = db.Queryable<Business_SevenSection>().Where(x =>
                                    x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == accountModeCode).First();
                                assetReview.EXP_ACCOUNT_SEGMENT = accountModel.Descrption;
                                assetReview.YTD_DEPRECIATION = 0;
                                assetReview.ACCT_DEPRECIATION = 0;
                                assetReview.FIXED_ASSETS_ORDERID = vguid;
                                assetReview.CREATE_USER = cache[PubGet.GetUserKey].UserName;
                                assetReview.CREATE_DATE = DateTime.Now;
                                assetReviewList.Add(assetReview);
                            }
                            db.Insertable<Business_AssetReview>(assetReviewList).ExecuteCommand();
                            purchaseAssign.ChangeUser = cache[PubGet.GetUserKey].UserName;
                            purchaseAssign.ChangeDate = DateTime.Now;
                            purchaseAssign.SubmitStatus = 1;
                            db.Updateable(purchaseAssign)
                                .UpdateColumns(it => new { it.ChangeUser, it.ChangeDate, it.SubmitStatus }).ExecuteCommand();
                            resultModel.Status = "1";
                        }
                        else
                        {
                            resultModel.IsSuccess = false;
                            resultModel.ResultInfo = "请检查车辆税费订单是否都已发起支付";
                            resultModel.Status = "2";
                        }
                    }
                    else
                    {
                        resultModel.IsSuccess = false;
                        resultModel.ResultInfo = "车辆税费订单未支付";
                        resultModel.Status = "2";
                    }
                });
                if (resultModel.Status != "2" && resultModel.Status != "1")
                {
                    resultModel.Status = Convert.ToBoolean(result.IsSuccess) ? resultModel.Status : "0";
                }
            });
            return Json(resultModel);
        }
    } 
}