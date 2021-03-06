﻿using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
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
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("a69dd619-91f4-4e10-b5f0-5dab64ec1465");
            return View();
        }
        public JsonResult GetBusiness_PurchaseAssignListDatas(Business_PurchaseAssign searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_PurchaseAssign>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.SqlQueryable<Business_PurchaseAssign>("SELECT pa.* FROM Business_PurchaseAssign pa INNER JOIN (select * from Business_FixedAssetsOrder where SubmitStatus = 2) fao ON pa.FixedAssetsOrderVguid = fao.VGUID")//WHERE fao.SubmitStatus = 1
                    .Where(x => x.OrderType == searchParams.OrderType)
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
                    listFixedAssetsOrder = db.Queryable<Business_AssetOrderDetails>().Where(x => x.AssetsOrderVguid == AssetsOrderVguid).OrderBy(x => x.AssetManagementCompany).ToList();
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
                var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                foreach (var item in listManagementCompany)
                {
                    if (!db.Queryable<Business_AssetOrderDetails>()
                        .Any(c => c.AssetsOrderVguid == AssetsOrderVguid && c.KeyData == item.KeyData))
                    {
                        var model = new Business_AssetOrderDetails();
                        model.VGUID = Guid.NewGuid();
                        model.AssetsOrderVguid = AssetsOrderVguid;
                        model.CreateDate = DateTime.Now;
                        model.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        model.KeyData = item.KeyData;
                        model.AssetManagementCompany = ssList
                            .Where(x => x.AccountModeCode == model.KeyData.Substring(0, 4).ToString() && x.Code == model.KeyData.Substring(4, 2).ToString()).First()
                            .Abbreviation;
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
            DbBusinessDataService.Command(db =>
            {
                list = db.Queryable<Business_SevenSection>().Where(x =>
                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").Select(x => new BelongToCompanyModel { BelongToCompany = x.Descrption }).ToList();
            });
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
                        saveObj.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                        saveObj.AssetNum = AssetNum;
                        saveObj.BelongToCompany = BelongToCompany;
                        db.Updateable(saveObj).IgnoreColumns(it => new { it.CreateDate,it.CreateUser }).ExecuteCommand();
                    }
                    else
                    {
                        var saveObj = new Business_AssetOrderBelongTo();
                        saveObj.VGUID = Guid.NewGuid();
                        saveObj.CreateDate = DateTime.Now;
                        saveObj.CreateUser = cache[PubGet.GetUserKey].LoginName;
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
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
            if (File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "PurchaseAssign\\";
                var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath + newFileName;
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + uploadPath))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + uploadPath);
                }
                try
                {
                    File.SaveAs(filePath);
                    //校验总数是否一致，校验管理公司是否一致
                    DbBusinessDataService.Command(db =>
                    {
                        var consistent = true;
                        var result = db.Ado.UseTran(() =>
                        {
                            //判断是否有已经发起支付的数据，如果有不允许导入
                            var fixedAssetsOrder = db.Queryable<Business_FixedAssetsOrder>()
                                .Where(c => c.VGUID == vguid).First();
                            var list = new List<Excel_PurchaseAssignModel>();
                            var dt = ExcelHelper.ExportToDataTable(filePath, true);
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                var assign = new Excel_PurchaseAssignModel();
                                assign.VehicleModel = fixedAssetsOrder.GoodsModel;
                                assign.ChassisNumber = dt.Rows[i][0].ToString();
                                assign.EngineNumber = dt.Rows[i][1].ToString();
                                list.Add(assign);
                            }
                            var taxFeeOrderList = db.Queryable<Business_PurchaseOrderNum, Business_TaxFeeOrder>(
                                (pon, tfo) => new object[]
                                {
                                    JoinType.Left, pon.FaxOrderVguid == tfo.VGUID
                                }).Select((pon, tfo) => new
                                {
                                    FixedAssetOrderVguid = pon.FixedAssetOrderVguid,
                                    FaxOrderVguid = pon.FaxOrderVguid,
                                    SubmitStatus = tfo.SubmitStatus
                                }).ToList();
                            taxFeeOrderList = taxFeeOrderList.Where(x => x.FixedAssetOrderVguid == vguid).ToList();
                            if (taxFeeOrderList.Any(x => x.SubmitStatus >= 2))
                            {
                                consistent = false;
                                resultModel.ResultInfo = "已经有数据支付，不能重新导入 ";
                                return;
                            }
                            if (list.Any(x => x.ChassisNumber.Length != 17))
                            {
                                consistent = false;
                                resultModel.ResultInfo = resultModel.ResultInfo + "车架号不正确 ";
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
                            if (set.Count != list.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo = resultModel.ResultInfo + "Excel中有重复数据 ";
                            }

                            var purchaseAssign = db.Queryable<Business_PurchaseAssign>()
                                .Where(c => c.FixedAssetsOrderVguid == vguid).First();
                            //判断判断车架号发动机号，系统中没有才可以导入
                            var listAssetInfo = db.Queryable<Business_AssetMaintenanceInfo>().Where(x => x.GROUP_ID == "出租车")
                            .Select(x => new { EngineNumber_ChassisNumber = x.ENGINE_NUMBER + x.CHASSIS_NUMBER }).ToList();
                            var listExcel = list.Select(x => new
                            { EngineNumber_ChassisNumber = x.EngineNumber + x.ChassisNumber }).ToList();
                            if (listAssetInfo.Union(listExcel).ToList().Count < listAssetInfo.Count + listExcel.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo += "导入的车架号和发动机号已存在 ";
                                return;
                            }
                            //判断已导入未审核的车架号发动机号，没有才可以导入
                            var listReviewInfo = db.Queryable<Business_AssetReview>()
                                .Select(x => new { EngineNumber_ChassisNumber = x.ENGINE_NUMBER + x.CHASSIS_NUMBER }).ToList();
                            if (listReviewInfo.Union(listExcel).ToList().Count < listReviewInfo.Count + listExcel.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo += "导入的车架号和发动机号已存在 ";
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
                                    belongTo.CreateUser = cache[PubGet.GetUserKey].LoginName;
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

                                var purchaseOrderSettingList = db.Queryable<Business_PurchaseOrderSetting>().ToList();
                                //车型
                                var vehicleModelList = sevenSectionList.Select(x => new Vehicle_Model { VehicleModel = x.VehicleModel }).ToList();
                                var vehicleModels = vehicleModelList.GroupBy(c => new { c.VehicleModel }).Select(c => c.Key).ToList();
                                //根据车型获取各项费用单价并生成订单
                                foreach (var vehicleModel in vehicleModels)
                                {
                                    //根据车型获取各项费用的单价
                                    var feeList = db.Queryable<Business_VehicleExtrasFeeSetting>()
                                        .Where(x => x.VehicleModel == vehicleModel.VehicleModel && x.Status && x.Fee != 0).ToList();
                                    //采购数量
                                    var orderNum = sevenSectionList.Where(x => x.VehicleModel == vehicleModel.VehicleModel).Sum(x => x.AssetNum);
                                    //出库费
                                    var maxOrderNumRight = 0;
                                    var orderNumberLeft = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                                    //供应商信息
                                    var bankInfoList = db.SqlQueryable<v_Business_CustomerBankInfo>(
                                            @"select a.*,b.Isable,b.OrderVGUID from Business_CustomerBankInfo as a 
                                                left join Business_CustomerBankSetting as b on a.VGUID = b.CustomerID
                                                left join v_Business_BusinessTypeSet as c on c.VGUID = b.OrderVGUID where b.Isable = '1'")
                                        .OrderBy(i => i.CreateTime, OrderByType.Desc).ToList();
                                    //付款信息
                                    var companylist = db.Queryable<Business_UserCompanySetDetail>().Where(x => x.AccountModeCode == AccountModeCode && x.Isable)
                                        .OrderBy(i => i.CompanyCode).ToList();
                                    foreach (var itemFee in feeList)
                                    {
                                        var feeOrder = new Business_TaxFeeOrder();
                                        feeOrder.PurchaseDepartmentIDs = "a8b4f808-33f3-454a-9a19-0b90d40aabea";//默认营运部
                                        feeOrder.PayItem = itemFee.BusinessProject.Split("|")[itemFee.BusinessProject.Split("|").Length - 1];
                                        feeOrder.PayItemCode = itemFee.BusinessSubItem;
                                        feeOrder.VehicleModel = vehicleModel.VehicleModel;
                                        feeOrder.VehicleModelCode = itemFee.VehicleModelCode;
                                        feeOrder.OrderQuantity = orderNum;
                                        feeOrder.UnitPrice = itemFee.Fee;
                                        feeOrder.SumPayment = orderNum * itemFee.Fee;
                                        feeOrder.SubmitStatus = 0;
                                        feeOrder.PayType = "转账";
                                        feeOrder.VGUID = Guid.NewGuid();
                                        feeOrder.CreateDate = DateTime.Now;
                                        feeOrder.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                        var autoID = "TaxFeeOrder";
                                        var no = CreateNo.GetCreateNo(db, autoID);
                                        feeOrder.OrderNumber = no;
                                        var purchaseOrderNum = new Business_PurchaseOrderNum();
                                        purchaseOrderNum.VGUID = Guid.NewGuid();
                                        purchaseOrderNum.PayItemCode = feeOrder.PayItemCode;
                                        purchaseOrderNum.PayItem = feeOrder.PayItem;
                                        purchaseOrderNum.OrderQuantity = feeOrder.OrderQuantity;
                                        purchaseOrderNum.FaxOrderVguid = feeOrder.VGUID;
                                        purchaseOrderNum.FixedAssetOrderVguid = vguid;
                                        purchaseOrderNum.CreateDate = DateTime.Now;
                                        purchaseOrderNum.CreateUser = "System";
                                        //根据付款项目填充供应商信息和付款信息
                                        var OrderVguid = db.Queryable<v_Business_BusinessTypeSet>().Where(x => x.BusinessSubItem1 == itemFee.BusinessSubItem).First().VGUID.ToString();
                                        if (bankInfoList.Any(x => x.OrderVGUID == OrderVguid) && bankInfoList.Count(x => x.OrderVGUID == OrderVguid) == 1)
                                        {
                                            var bankInfo = bankInfoList.First(x => x.OrderVGUID == OrderVguid);
                                            feeOrder.PaymentInformationVguid = bankInfo.VGUID;
                                            feeOrder.PaymentInformation = bankInfo.BankAccountName;
                                            feeOrder.SupplierBankAccountName = bankInfo.BankAccountName;
                                            feeOrder.SupplierBankAccount = bankInfo.BankAccount;
                                            feeOrder.SupplierBankNo = bankInfo.BankNo;
                                            feeOrder.SupplierBank = bankInfo.Bank;
                                        }
                                        if (companylist.Any(x => x.OrderVGUID == OrderVguid) && companylist.Count(x => x.OrderVGUID == OrderVguid) == 1)
                                        {
                                            var companyInfo = companylist.First(x => x.OrderVGUID == OrderVguid);
                                            feeOrder.PayCompanyVguid = companyInfo.VGUID;
                                            feeOrder.PayCompany = companyInfo.PayBankAccountName;
                                            feeOrder.CompanyBankName = companyInfo.PayBank;
                                            feeOrder.CompanyBankAccountName = companyInfo.PayBankAccountName;
                                            feeOrder.CompanyBankAccount = companyInfo.PayAccount;
                                            feeOrder.AccountType = companyInfo.AccountType;
                                        }
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
                                //获取订单部门
                                var departmentList = db.SqlQueryable<PurchaseDepartmentModel>(@"SELECT VGUID,Descrption
                                                                                        FROM Business_SevenSection
                                                                                        WHERE SectionVGUID = 'D63BD715-C27D-4C47-AB66-550309794D43'
                                                                                              AND AccountModeCode = '1002'
                                                                                              AND CompanyCode = '01'
                                                                                              AND Status = '1'
                                                                                              AND Code LIKE '10%'").ToList();
                                var departmentIDsArr = db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == fixedAssetsOrderInfo.VGUID).First().PurchaseDepartmentIDs.Split(",");
                                var strList = new List<Guid>();
                                foreach (var departmentID in departmentIDsArr)
                                {
                                    strList.Add(departmentID.TryToGuid());
                                }
                                var departments = departmentList.Where(x => strList.Contains(x.VGUID));
                                var departmentStr = "";
                                foreach (var ditem in departments)
                                {
                                    departmentStr = departmentStr + ditem.Descrption + ",";
                                }
                                departmentStr = departmentStr.Substring(0, departmentStr.Length - 1);
                                var assetReviewList = new List<Business_AssetReview>();
                                foreach (var item in belongToList)
                                {
                                    //根据车型获取各项费用的单价
                                    var feeList = db.Queryable<Business_VehicleExtrasFeeSetting>()
                                        .Where(x => x.VehicleModel == item.VehicleModel && x.Status).ToList();
                                    var assetReview = new Business_AssetReview();
                                    assetReview.VGUID = Guid.NewGuid();
                                    //var autoID = "FixedAssetID";
                                    //var no = CreateNo.GetCreateNo(db, autoID);
                                    //assetReview.ASSET_ID = no;
                                    assetReview.GROUP_ID = fixedAssetsOrder.PurchaseGoods;
                                    assetReview.DESCRIPTION = fixedAssetsOrder.GoodsModel;//item.VehicleModel;
                                    assetReview.ENGINE_NUMBER = item.EngineNumber;
                                    assetReview.CHASSIS_NUMBER = item.ChassisNumber;
                                    assetReview.VEHICLE_SHORTNAME = fixedAssetsOrder.GoodsModel;
                                    assetReview.DESCRIPTION = fixedAssetsOrder.GoodsModel;
                                    //assetReview.START_VEHICLE_DATE = fixedAssetsOrderInfo.LISENSING_DATE;
                                    //assetReview.PURCHASE_DATE = fixedAssetsOrderInfo.CreateDate;
                                    assetReview.QUANTITY = 1;
                                    assetReview.NUDE_CAR_FEE = fixedAssetsOrderInfo.PurchasePrices;
                                    //车辆税费 根据车型取税费订单中对应的费用
                                    assetReview.PURCHASE_TAX = feeList.First(x => x.BusinessSubItem == "cz|03|0301|030102").Fee;
                                    assetReview.LISENSING_FEE = feeList.First(x => x.BusinessSubItem == "cz|03|0301|030103").Fee;
                                    assetReview.OUT_WAREHOUSE_FEE = feeList.First(x => x.BusinessSubItem == "cz|03|0301|030104").Fee;
                                    assetReview.DOME_LIGHT_FEE = feeList.First(x => x.BusinessSubItem == "cz|03|0301|030105").Fee;
                                    assetReview.ANTI_ROBBERY_FEE = feeList.First(x => x.BusinessSubItem == "cz|03|0301|030106").Fee;
                                    assetReview.LOADING_FEE = feeList.First(x => x.BusinessSubItem == "cz|03|0301|030107").Fee;
                                    assetReview.INNER_ROOF_FEE = feeList.First(x => x.BusinessSubItem == "cz|03|0301|030108").Fee;
                                    assetReview.TAXIMETER_FEE = feeList.First(x => x.BusinessSubItem == "cz|03|0301|030109").Fee;
                                    assetReview.ASSET_COST = assetReview.NUDE_CAR_FEE +
                                                             assetReview.PURCHASE_TAX +
                                                             assetReview.LISENSING_FEE +
                                                             assetReview.OUT_WAREHOUSE_FEE +
                                                             assetReview.DOME_LIGHT_FEE +
                                                             assetReview.ANTI_ROBBERY_FEE +
                                                             assetReview.LOADING_FEE +
                                                             assetReview.INNER_ROOF_FEE +
                                                             assetReview.TAXIMETER_FEE;
                                    //资产主类次类 根据采购物品获取
                                    assetReview.ASSET_CATEGORY_MAJOR = purchaseOrderSettingList
                                        .Where(x => x.VGUID == fixedAssetsOrderInfo.PurchaseGoodsVguid).First()
                                        .AssetCategoryMajor;
                                    assetReview.ASSET_CATEGORY_MINOR = purchaseOrderSettingList
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
                                    assetReview.OBDSTATUS = false;
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
                                    assetReview.CREATE_USER = cache[PubGet.GetUserKey].LoginName;
                                    assetReview.CREATE_DATE = DateTime.Now;
                                    
                                    assetReview.ORGANIZATION_NUM = "财务共享-" + departmentStr;
                                    assetReviewList.Add(assetReview);
                                }
                                db.Insertable<Business_AssetReview>(assetReviewList).ExecuteCommand();
                                purchaseAssign.ChangeUser = cache[PubGet.GetUserKey].LoginName;
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
        public JsonResult ImportOBDAssignFile(Guid vguid, HttpPostedFileBase File)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            if (File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "PurchaseOBDAssign\\";
                var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath + newFileName;
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + uploadPath))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + uploadPath);
                }
                try
                {
                    File.SaveAs(filePath);
                    //校验总数是否一致，校验管理公司是否一致
                    DbBusinessDataService.Command(db =>
                    {
                        var consistent = true;
                        var result = db.Ado.UseTran(() =>
                        {
                            var fixedAssetsOrder = db.Queryable<Business_FixedAssetsOrder>()
                                .Where(c => c.VGUID == vguid).First();
                            var list = new List<Excel_PurchaseOBDAssignModel>();
                            var dt = ExcelHelper.ExportToDataTable(filePath, true);
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                var assign = new Excel_PurchaseOBDAssignModel();
                                assign.EquipmentNumber = dt.Rows[i][0].ToString();
                                assign.PlateNumber = dt.Rows[i][1].ToString();
                                assign.LisensingDate = dt.Rows[i][2].TryToDate();
                                list.Add(assign);
                            }
                            if (list.Any(x => x.EquipmentNumber.Length != 15))
                            {
                                consistent = false;
                                resultModel.ResultInfo = resultModel.ResultInfo + "设备号不正确 ";
                            }
                            if (fixedAssetsOrder.OrderQuantity != list.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo += "订单总数不一致 ";
                            }
                            //判断导入数据是否重复
                            var set = new HashSet<string>();
                            list.ForEach(x => {
                                set.Add(x.PlateNumber + x.EquipmentNumber);
                            });
                            if (set.Count != list.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo = resultModel.ResultInfo + "Excel中有重复数据 ";
                            }

                            var purchaseAssign = db.Queryable<Business_PurchaseAssign>()
                                .Where(c => c.FixedAssetsOrderVguid == vguid).First();
                            //判断判断车架号发动机号，系统中没有才可以导入
                            var listAssetInfo = db.Queryable<Business_AssetMaintenanceInfo>()
                            .Select(x => new { PlateNumber_EquipmentNumber = x.PLATE_NUMBER + x.CHASSIS_NUMBER }).ToList();
                            var listExcel = list.Select(x => new
                            { PlateNumber_EquipmentNumber = x.PlateNumber + x.EquipmentNumber }).ToList();
                            if (listAssetInfo.Union(listExcel).ToList().Count < listAssetInfo.Count + listExcel.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo += "导入的车牌号和设备号已存在 ";
                                return;
                            }
                            //判断已导入未审核的车架号发动机号，没有才可以导入
                            var listReviewInfo = db.Queryable<Business_AssetReview>()
                                .Select(x => new { PlateNumber_EquipmentNumber = x.PLATE_NUMBER + x.CHASSIS_NUMBER }).ToList();
                            if (listReviewInfo.Union(listExcel).ToList().Count < listReviewInfo.Count + listExcel.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo += "导入的车牌号和设备号已存在 ";
                                return;
                            }
                            if (consistent)
                            {
                                var assetInfoList = db.Queryable<Business_AssetMaintenanceInfo>().ToList();
                                //写入资产审核表
                                //获取固定资产信息
                                var fixedAssetsOrderInfo =
                                    db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == vguid).First();
                                if (db.Queryable<Business_AssetReview>().Any(x => x.FIXED_ASSETS_ORDERID == vguid))
                                {
                                    db.Deleteable<Business_AssetReview>().Where(c => c.FIXED_ASSETS_ORDERID == vguid).ExecuteCommand();
                                }
                                var orderNumberLeftAsset = "CZ" + DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                                //获取订单部门
                                var departmentList = db.SqlQueryable<PurchaseDepartmentModel>(@"SELECT VGUID,Descrption
                                                                                        FROM Business_SevenSection
                                                                                        WHERE SectionVGUID = 'D63BD715-C27D-4C47-AB66-550309794D43'
                                                                                              AND AccountModeCode = '1002'
                                                                                              AND CompanyCode = '01'
                                                                                              AND Status = '1'
                                                                                              AND Code LIKE '10%'").ToList();
                                var assetReviewList = new List<Business_AssetReview>();
                                var orderSetting = db.Queryable<Business_PurchaseOrderSetting>()
                                    .Where(x => x.VGUID == fixedAssetsOrderInfo.PurchaseGoodsVguid).First();
                                var assetsCategoryList = db.Queryable<Business_AssetsCategory>().ToList();
                                var departmentIDsArr = db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == fixedAssetsOrderInfo.VGUID).First().PurchaseDepartmentIDs.Split(",");
                                var strList = new List<Guid>();
                                foreach (var departmentID in departmentIDsArr)
                                {
                                    strList.Add(departmentID.TryToGuid());
                                }
                                var departments = departmentList.Where(x => strList.Contains(x.VGUID));
                                var departmentStr = "";
                                foreach (var ditem in departments)
                                {
                                    departmentStr = departmentStr + ditem.Descrption + ",";
                                }
                                departmentStr = departmentStr.Substring(0, departmentStr.Length - 1);
                                foreach (var item in list)
                                {
                                    var assetReview = new Business_AssetReview();
                                    assetReview.VGUID = Guid.NewGuid();
                                    var autoID = "FixedAssetID";
                                    var no = CreateNo.GetCreateNo(db, autoID);
                                    assetReview.ASSET_ID = no;
                                    assetReview.GROUP_ID = fixedAssetsOrder.PurchaseGoods;
                                    assetReview.PLATE_NUMBER = item.PlateNumber;
                                    assetReview.CHASSIS_NUMBER = item.EquipmentNumber;
                                    assetReview.VEHICLE_SHORTNAME = "OBD";
                                    assetReview.DESCRIPTION = item.PlateNumber;
                                    assetReview.LISENSING_DATE = item.LisensingDate;
                                    assetReview.TAG_NUMBER = assetReview.ASSET_ID.Replace("CZ","JK");
                                    //assetReview.START_VEHICLE_DATE = fixedAssetsOrderInfo.LISENSING_DATE;
                                    //assetReview.PURCHASE_DATE = fixedAssetsOrderInfo.CreateDate;
                                    assetReview.QUANTITY = 1;
                                    assetReview.ASSET_COST = fixedAssetsOrder.PurchasePrices;
                                    //资产主类次类 根据采购物品获取
                                    assetReview.ASSET_CATEGORY_MAJOR = orderSetting.AssetCategoryMajor;
                                    assetReview.ASSET_CATEGORY_MINOR = orderSetting.AssetCategoryMinor;
                                    //根据主类子类从折旧方法表中获取
                                    var assetsCategoryInfo = assetsCategoryList.First(x => x.ASSET_CATEGORY_MAJOR == assetReview.ASSET_CATEGORY_MAJOR &&
                                                                                           x.ASSET_CATEGORY_MINOR == assetReview.ASSET_CATEGORY_MINOR);
                                    assetReview.LIFE_YEARS = assetsCategoryInfo.LIFE_YEARS;
                                    assetReview.LIFE_MONTHS = assetsCategoryInfo.LIFE_MONTHS;
                                    assetReview.AMORTIZATION_FLAG = "N";
                                    assetReview.METHOD = assetsCategoryInfo.METHOD;
                                    assetReview.BOOK_TYPE_CODE = assetsCategoryInfo.BOOK_TYPE_CODE;
                                    assetReview.ASSET_COST_ACCOUNT = assetsCategoryInfo.ASSET_COST_ACCOUNT;
                                    assetReview.ASSET_SETTLEMENT_ACCOUNT = assetsCategoryInfo.ASSET_SETTLEMENT_ACCOUNT;
                                    assetReview.DEPRECIATION_EXPENSE_SEGMENT = assetsCategoryInfo.DEPRECIATION_EXPENSE_SEGMENT;
                                    assetReview.ACCT_DEPRECIATION_ACCOUNT = assetsCategoryInfo.ACCT_DEPRECIATION_ACCOUNT;
                                    assetReview.SALVAGE_PERCENT = assetsCategoryInfo.SALVAGE_PERCENT;
                                    assetReview.ISVERIFY = false;
                                    assetReview.YTD_DEPRECIATION = 0;
                                    assetReview.ACCT_DEPRECIATION = 0;
                                    assetReview.FIXED_ASSETS_ORDERID = vguid;
                                    assetReview.CREATE_USER = cache[PubGet.GetUserKey].LoginName;
                                    assetReview.CREATE_DATE = DateTime.Now;
                                    assetReview.ORGANIZATION_NUM = "财务共享-" + departmentStr;
                                    if (assetInfoList.Any(x => x.PLATE_NUMBER == item.PlateNumber))
                                    {
                                        var assetInfo = assetInfoList
                                            .First(x => x.PLATE_NUMBER == item.PlateNumber);
                                        assetReview.BELONGTO_COMPANY = assetInfo.BELONGTO_COMPANY;
                                        assetReview.MANAGEMENT_COMPANY = assetInfo.MANAGEMENT_COMPANY;
                                        assetReview.MODEL_MAJOR = assetInfo.MODEL_MAJOR;
                                        assetReview.MODEL_MINOR = assetInfo.MODEL_MINOR;
                                        assetReview.EXP_ACCOUNT_SEGMENT = assetInfo.EXP_ACCOUNT_SEGMENT;
                                        assetReview.VEHICLE_STATE = assetInfo.VEHICLE_STATE;
                                        assetReview.OPERATING_STATE = assetInfo.OPERATING_STATE;
                                    }
                                    assetReviewList.Add(assetReview);
                                }
                                db.Insertable<Business_AssetReview>(assetReviewList).ExecuteCommand();
                                purchaseAssign.ChangeUser = cache[PubGet.GetUserKey].LoginName;
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
                    var purchaseOrderSettingList = db.Queryable<Business_PurchaseOrderSetting>().ToList();
                    var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                        x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                    var purchaseAssign = db.Queryable<Business_PurchaseAssign>()
                        .Where(c => c.FixedAssetsOrderVguid == vguid).First();
                    //获取固定资产信息 写入资产审核表
                    var fixedAssetsOrderInfo =
                        db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == vguid).First();
                    if (db.Queryable<Business_AssetReview>().Any(x => x.FIXED_ASSETS_ORDERID == vguid))
                    {
                        db.Deleteable<Business_AssetReview>().Where(c => c.FIXED_ASSETS_ORDERID == vguid).ExecuteCommand();
                    }
                    var manageCompanyList = db.Queryable<Business_AssetOrderDetails>()
                        .Where(x => x.AssetsOrderVguid == vguid).ToList();
                    var orderNumberLeftAsset = "CZ" + DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                    //查出当前日期数据库中最大的订单号
                    var currentDayAssetReviewList = db.Queryable<Business_AssetReview>()
                        .Where(c => c.ASSET_ID.StartsWith(orderNumberLeftAsset)).Select(c => new { c.ASSET_ID }).ToList();
                    var maxOrderNumRightAsset = 0;
                    if (currentDayAssetReviewList.Any())
                    {
                        maxOrderNumRightAsset = currentDayAssetReviewList.OrderByDescending(c => c.ASSET_ID.Replace(orderNumberLeftAsset, "").TryToInt()).First().ASSET_ID.Replace(orderNumberLeftAsset, "").TryToInt();
                    }
                    //获取订单部门
                    var departmentList = db.SqlQueryable<PurchaseDepartmentModel>(@"SELECT VGUID,Descrption
                                                                                        FROM Business_SevenSection
                                                                                        WHERE SectionVGUID = 'D63BD715-C27D-4C47-AB66-550309794D43'
                                                                                              AND AccountModeCode = '1002'
                                                                                              AND CompanyCode = '01'
                                                                                              AND Status = '1'
                                                                                              AND Code LIKE '10%'").ToList();
                    var departmentIDsArr = db.Queryable<Business_FixedAssetsOrder>().Where(x => x.VGUID == fixedAssetsOrderInfo.VGUID).First().PurchaseDepartmentIDs.Split(",");
                    var strList = new List<Guid>();
                    foreach (var departmentID in departmentIDsArr)
                    {
                        strList.Add(departmentID.TryToGuid());
                    }
                    var departments = departmentList.Where(x => strList.Contains(x.VGUID));
                    var departmentStr = "";
                    foreach (var ditem in departments)
                    {
                        departmentStr = departmentStr + ditem.Descrption + ",";
                    }
                    departmentStr = departmentStr.Substring(0, departmentStr.Length - 1);
                    var assetReviewList = new List<Business_AssetReview>();
                    foreach (var item in manageCompanyList)
                    {
                        if (!item.AssetNum.IsNullOrEmpty())
                        {
                            for (int i = 0; i < item.AssetNum; i++)
                            {
                                maxOrderNumRightAsset++;
                                var assetReview = new Business_AssetReview();
                                assetReview.VGUID = Guid.NewGuid();
                                assetReview.ASSET_ID = orderNumberLeftAsset + maxOrderNumRightAsset.ToString().PadLeft(4, '0');
                                assetReview.GROUP_ID = fixedAssetsOrderInfo.PurchaseGoods;
                                assetReview.DESCRIPTION = fixedAssetsOrderInfo.AssetDescription;
                                //assetReview.START_VEHICLE_DATE = fixedAssetsOrderInfo.LISENSING_DATE;
                                //assetReview.PURCHASE_DATE = fixedAssetsOrderInfo.CreateDate;
                                assetReview.QUANTITY = 1;
                                assetReview.ASSET_COST = fixedAssetsOrderInfo.PurchasePrices;
                                //资产主类次类 根据采购物品获取
                                assetReview.ASSET_CATEGORY_MAJOR = purchaseOrderSettingList.First(x => x.VGUID == fixedAssetsOrderInfo.PurchaseGoodsVguid).AssetCategoryMajor;
                                assetReview.ASSET_CATEGORY_MINOR = purchaseOrderSettingList.First(x => x.VGUID == fixedAssetsOrderInfo.PurchaseGoodsVguid).AssetCategoryMinor;
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
                                assetReview.MANAGEMENT_COMPANY = item.AssetManagementCompany;
                                assetReview.MANAGEMENT_COMPANY_CODE = ssList.First(x => x.Abbreviation == item.AssetManagementCompany).OrgID;
                                //assetReview.BELONGTO_COMPANY_CODE = ssList.First(x => x.Descrption == item.BelongToCompany).OrgID;
                                //总账帐簿 根据主信息中的资产所属公司获得后通过映射表转换
                                //var accountModeCode = ssList.First(x => x.Descrption == assetReview.BELONGTO_COMPANY).AccountModeCode;
                                //var accountModel = db.Queryable<Business_SevenSection>().Where(x =>
                                //    x.SectionVGUID == "H63BD715-C27D-4C47-AB66-550309794D43" && x.Code == accountModeCode).First();
                                //assetReview.EXP_ACCOUNT_SEGMENT = accountModel.Descrption;
                                assetReview.YTD_DEPRECIATION = 0;
                                assetReview.ACCT_DEPRECIATION = 0;
                                assetReview.FIXED_ASSETS_ORDERID = vguid;
                                assetReview.OBDSTATUS = false;
                                assetReview.CREATE_USER = cache[PubGet.GetUserKey].LoginName;
                                assetReview.CREATE_DATE = DateTime.Now;
                                assetReview.ORGANIZATION_NUM = "财务共享-" + departmentStr;
                                assetReviewList.Add(assetReview);
                            }
                        }
                    }
                    db.Insertable<Business_AssetReview>(assetReviewList).ExecuteCommand();
                    purchaseAssign.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                    purchaseAssign.ChangeDate = DateTime.Now;
                    purchaseAssign.SubmitStatus = 1;
                    db.Updateable(purchaseAssign)
                        .UpdateColumns(it => new { it.ChangeUser, it.ChangeDate, it.SubmitStatus }).ExecuteCommand();
                    resultModel.IsSuccess = true;
                    resultModel.Status = "1";
                });
            });
            return Json(resultModel);
        }
    } 
}