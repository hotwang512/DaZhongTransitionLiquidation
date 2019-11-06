using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.BankFlowTemplate;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using DaZhongTransitionLiquidation.Models;


namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.FixedAssetsOrder
{
    public class FixedAssetsOrderController : BaseController
    {
        // GET: AssetManagement/FixedAssetsOrder
        public FixedAssetsOrderController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("0c8cd244-a13c-41ff-9b5e-65bf21cd35f6");
            return View();
        }
        public JsonResult GetFixedAssetsOrderListDatas(Business_FixedAssetsOrder searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_FixedAssetsOrder>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_FixedAssetsOrder>()
                    .Where(x => x.OrderType == searchParams.OrderType)
                    .WhereIF(searchParams.PurchaseGoodsVguid != null, i => i.PurchaseGoodsVguid == searchParams.PurchaseGoodsVguid)
                    .WhereIF(searchParams.SubmitStatus != -1, i => i.SubmitStatus == searchParams.SubmitStatus)
                    .WhereIF(searchParams.OrderNumber != null, i => i.OrderNumber.Contains(searchParams.OrderNumber))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteFixedAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //查看是否有提交的订单，如果有提示提交后不可以删除
                var isAnySubmited = db.Queryable<Business_FixedAssetsOrder>().Any(c => vguids.Contains(c.VGUID) && (c.SubmitStatus == FixedAssetsSubmitStatusEnum.Submited.TryToInt() || c.SubmitStatus == FixedAssetsSubmitStatusEnum.UnPay.TryToInt()));
                if (isAnySubmited)
                {
                    resultModel.ResultInfo = "存在已提交的订单，订单提交后不允许删除";
                    resultModel.IsSuccess = false;
                    resultModel.Status = "2";
                }
                else
                {
                    //删除主表信息
                    saveChanges = db.Deleteable<Business_FixedAssetsOrder>(x => vguids.Contains(x.VGUID)).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public JsonResult SubmitFixedAssetsOrder(List<Guid> vguids)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    foreach (var vguid in vguids)
                    {
                        var model = db.Queryable<Business_FixedAssetsOrder>().Where(c => c.VGUID == vguid).First();
                        if (model.SubmitStatus == FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt())
                        {
                            //请求清算平台、待付款请求生成支付凭证接口
                            var pendingPaymentmodel = new PendingPaymentModel();
                            //统计附件信息
                            var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>().Where(x => x.AssetOrderVGUID == vguid).ToList();
                            pendingPaymentmodel.PaymentReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "付款凭证").ToList());
                            pendingPaymentmodel.InvoiceReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList());
                            pendingPaymentmodel.ApprovalReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "OA审批单").ToList());
                            pendingPaymentmodel.Contract = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "合同").ToList());
                            pendingPaymentmodel.DetailList = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "清单、清册").ToList());
                            pendingPaymentmodel.OtherReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "其他").ToList());
                            var goodsData = db.Queryable<Business_PurchaseOrderSetting>()
                                .Where(x => x.VGUID == model.PurchaseGoodsVguid).First();
                            var orderListData = db.Queryable<v_Business_BusinessTypeSet>()
                                .Where(x => x.BusinessSubItem1 == goodsData.BusinessSubItem).First();
                            pendingPaymentmodel.ServiceCategory = orderListData.BusinessProject;
                            pendingPaymentmodel.BusinessProject = orderListData.BusinessSubItem1.Split("|")[0] + "|"
                                                                  + orderListData.BusinessSubItem1.Substring(orderListData.BusinessSubItem1.LastIndexOf("|") + 1, orderListData.BusinessSubItem1.Length - orderListData.BusinessSubItem1.LastIndexOf("|") - 1);
                            //根据供应商账号找到供应商类别
                            pendingPaymentmodel.PaymentCompany = db.Queryable<Business_CustomerBankInfo>()
                                .Where(x => x.BankAccount == model.SupplierBankAccount).First().CompanyOrPerson; ;
                            pendingPaymentmodel.CollectBankAccountName = model.SupplierBankAccountName;
                            pendingPaymentmodel.CollectBankAccouont = model.SupplierBankAccount;
                            pendingPaymentmodel.CollectBankName = model.SupplierBank;
                            pendingPaymentmodel.CollectBankNo = model.SupplierBankNo;
                            pendingPaymentmodel.PaymentMethod = model.PayType;
                            pendingPaymentmodel.IdentityToken = cache[PubGet.GetUserKey].Token;
                            pendingPaymentmodel.FunctionSiteId = "61";
                            pendingPaymentmodel.OperatorIP = GetSystemInfo.GetClientLocalIPv4Address();
                            pendingPaymentmodel.invoiceNumber = assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count().ToString();
                            pendingPaymentmodel.numberOfAttachments = (assetAttachmentList.Count() - assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count()).ToString();
                            pendingPaymentmodel.Amount = model.ContractAmount.ToString();
                            pendingPaymentmodel.Summary = model.AssetDescription;
                            pendingPaymentmodel.AccountSetCode = cache[PubGet.GetUserKey].AccountModeCode + "|" + cache[PubGet.GetUserKey].CompanyCode;
                            var apiReault = PendingPaymentApi(pendingPaymentmodel);
                            var pendingRedult = apiReault.JsonToModel<JsonResultModelApi<Api_PendingPayment>>();
                            if (pendingRedult.success)
                            {
                                var orderModel = db.Queryable<Business_FixedAssetsOrder>()
                                    .Where(x => x.VGUID == model.VGUID).First();
                                orderModel.PaymentVoucherVguid = pendingRedult.data.vguid;
                                orderModel.PaymentVoucherUrl = pendingRedult.data.url;
                                db.Updateable<Business_FixedAssetsOrder>(orderModel).UpdateColumns(x => new { x.PaymentVoucherUrl, x.PaymentVoucherVguid }).ExecuteCommand();
                                model.SubmitStatus = FixedAssetsSubmitStatusEnum.UnPay.TryToInt();
                                model.SubmitDate = DateTime.Now;
                                model.SubmitUser = cache[PubGet.GetUserKey].LoginName;
                                db.Updateable<Business_FixedAssetsOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                                //提交完后写入采购分配表
                                var purchaseAssignmodel = new Business_PurchaseAssign();
                                purchaseAssignmodel.VGUID = Guid.NewGuid();
                                purchaseAssignmodel.CreateDate = DateTime.Now;
                                purchaseAssignmodel.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                purchaseAssignmodel.FixedAssetsOrderVguid = model.VGUID;
                                purchaseAssignmodel.PurchaseGoodsVguid = model.PurchaseGoodsVguid;
                                purchaseAssignmodel.PurchaseGoods = model.PurchaseGoods;
                                purchaseAssignmodel.OrderQuantity = model.OrderQuantity;
                                purchaseAssignmodel.PurchasePrices = model.PurchasePrices;
                                purchaseAssignmodel.ContractAmount = model.ContractAmount;
                                purchaseAssignmodel.AssetDescription = model.AssetDescription;
                                db.Insertable<Business_PurchaseAssign>(purchaseAssignmodel).ExecuteCommand();
                                var fundClearingModel = Mapper.Map<Business_FundClearing>(purchaseAssignmodel);
                                fundClearingModel.VGUID = Guid.NewGuid();
                                db.Insertable<Business_FundClearing>(fundClearingModel).ExecuteCommand();
                                var companys = db.Queryable<Business_PurchaseManagementCompany>()
                                    .Where(x => x.PurchaseOrderSettingVguid == orderModel.PurchaseGoodsVguid && x.IsCheck).ToList();
                                var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                                var liquidationDistributionList = new List<Business_LiquidationDistribution>();
                                foreach (var company in companys)
                                {
                                    var liquidationDistribution = new Business_LiquidationDistribution();
                                    liquidationDistribution.VGUID = Guid.NewGuid();
                                    liquidationDistribution.FundClearingVguid = fundClearingModel.VGUID;
                                    liquidationDistribution.AssetsOrderVguid = fundClearingModel.FixedAssetsOrderVguid;
                                    liquidationDistribution.CompanyVguid = company.ManagementCompanyVguid;
                                    liquidationDistribution.Company = ssList.Where(x => x.Descrption == company.ManagementCompany).First().Abbreviation;
                                    liquidationDistribution.PurchasePrices = fundClearingModel.PurchasePrices;
                                    liquidationDistribution.AssetNum = 0;
                                    liquidationDistribution.ContractAmount = 0;
                                    liquidationDistribution.CreateDate = DateTime.Now;
                                    liquidationDistribution.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                    liquidationDistributionList.Add(liquidationDistribution);
                                }
                                db.Insertable<Business_LiquidationDistribution>(liquidationDistributionList).ExecuteCommand();
                                resultModel.ResultInfo = pendingRedult.data.url;
                                resultModel.IsSuccess = true;
                                resultModel.Status = "1";
                            }
                            else
                            {
                                LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                            }
                        }
                        else
                        {
                            resultModel.ResultInfo = "该状态下不允许发起支付";
                            resultModel.IsSuccess = false;
                            resultModel.Status = "2";
                        }
                    }
                });
            });
            return Json(resultModel);
        }
        public string JoinStr(List<Business_AssetAttachmentList> list)
        {
            var strArr = "";
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    strArr = strArr + item.Attachment + ",";
                }
                strArr = strArr.Substring(0, strArr.Length - 1);
                return strArr;
            }
            else
            {
                return strArr;
            }
        }
        public string PendingPaymentApi(PendingPaymentModel model)
        {
            var url = ConfigSugar.GetAppString("PendingPaymentUrl");
            var data = "{" +
                       "\"IdentityToken\":\"{IdentityToken}\",".Replace("{IdentityToken}", model.IdentityToken) +
                       "\"FunctionSiteId\":\"{FunctionSiteId}\",".Replace("{FunctionSiteId}", "61") +
                       "\"OperatorIP\":\"{OperatorIP}\",".Replace("{OperatorIP}", GetSystemInfo.GetClientLocalIPv4Address()) +
                       "\"AccountSetCode\":\"{AccountSetCode}\",".Replace("{AccountSetCode}", model.AccountSetCode) +
                       "\"ServiceCategory\":\"{ServiceCategory}\",".Replace("{ServiceCategory}", model.ServiceCategory) +
                       "\"BusinessProject\":\"{BusinessProject}\",".Replace("{BusinessProject}", model.BusinessProject) +
                       "\"PaymentCompany\":\"{PaymentCompany}\",".Replace("{PaymentCompany}", model.PaymentCompany) +
                       "\"CollectBankAccountName\":\"{CollectBankAccountName}\",".Replace("{CollectBankAccountName}", model.CollectBankAccountName) +
                       "\"CollectBankAccouont\":\"{CollectBankAccouont}\",".Replace("{CollectBankAccouont}", model.CollectBankAccouont) +
                       "\"CollectBankName\":\"{CollectBankName}\",".Replace("{CollectBankName}", model.CollectBankName) +
                       "\"CollectBankNo\":\"{CollectBankNo}\",".Replace("{CollectBankNo}", model.CollectBankNo) +
                       "\"PaymentMethod\":\"{PaymentMethod}\",".Replace("{PaymentMethod}", model.PaymentMethod) +
                       "\"invoiceNumber\":\"{invoiceNumber}\",".Replace("{invoiceNumber}", model.invoiceNumber) +
                       "\"numberOfAttachments\":\"{numberOfAttachments}\",".Replace("{numberOfAttachments}", model.numberOfAttachments) +
                       "\"Amount\":\"{Amount}\",".Replace("{Amount}", model.Amount) +
                       "\"Summary\":\"{Summary}\",".Replace("{Summary}", model.Summary);
            if (model.PaymentReceipt != "")
            {
                data += "\"PaymentReceipt\":\"{PaymentReceipt}\",".Replace("{PaymentReceipt}", model.PaymentReceipt);
            }
            if (model.InvoiceReceipt != "")
            {
                data += "\"InvoiceReceipt\":\"{InvoiceReceipt}\",".Replace("{InvoiceReceipt}", model.InvoiceReceipt);
            }
            if (model.ApprovalReceipt != "")
            {
                data += "\"ApprovalReceipt\":\"{ApprovalReceipt}\",".Replace("{ApprovalReceipt}", model.ApprovalReceipt);
            }
            if (model.Contract != "")
            {
                data += "\"Contract\":\"{Contract}\",".Replace("{Contract}", model.Contract);
            }
            if (model.DetailList != "")
            {
                data += "\"DetailList\":\"{DetailList}\",".Replace("{DetailList}", model.DetailList);
            }
            if (model.OtherReceipt != "")
            {
                data += "\"OtherReceipt\":\"{OtherReceipt}\"".Replace("{OtherReceipt}", model.OtherReceipt);
            }

            data = data.Substring(0, data.Length - 1);
            data = data + "}";
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                wc.Encoding = System.Text.Encoding.UTF8;
                var resultData = wc.UploadString(new Uri(url), "POST", data);
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
                return resultData;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
                return "";
            }
        }
        public JsonResult ImportOBDAssignFile(Guid vguid, HttpPostedFileBase File)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
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
                            //判断导入数据是否重复
                            var set = new HashSet<string>();
                            list.ForEach(x => {
                                set.Add(x.PlateNumber + x.EquipmentNumber);
                            });
                            if (set.Count != list.Count)
                            {
                                consistent = false;
                                resultModel.ResultInfo = resultModel.ResultInfo + "Excel中有重复数据 ";
                                return;
                            }
                            //判断判断车架号发动机号，系统中没有才可以导入
                            var listAssetInfo = db.Queryable<Business_AssetMaintenanceInfo>().Where(x => x.GROUP_ID == "OBD设备")
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
                            var listReviewInfo = db.Queryable<Business_AssetReview>().Where(x => x.GROUP_ID == "OBD设备")
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
                                if (db.Queryable<Business_AssetReview>().Any(x => x.IMPORT_VGUID == vguid))
                                {
                                    db.Deleteable<Business_AssetReview>().Where(c => c.IMPORT_VGUID == vguid).ExecuteCommand();
                                }
                                var assetReviewList = new List<Business_AssetReview>();
                                var assetsCategoryList = db.Queryable<Business_AssetsCategory>().ToList();
                                var orderSetting = db.Queryable<Business_PurchaseOrderSetting>()
                                    .Where(x => x.PurchaseGoods == "OBD设备").First();
                                var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                                    x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                                foreach (var item in list)
                                {
                                    var assetReview = new Business_AssetReview();
                                    assetReview.VGUID = Guid.NewGuid();
                                    assetReview.IMPORT_VGUID = vguid;
                                    assetReview.OBDSTATUS = false;
                                    var autoID = "FixedAssetID";
                                    var no = CreateNo.GetCreateNo(db, autoID);
                                    assetReview.ASSET_ID = no;
                                    assetReview.GROUP_ID = "OBD设备";
                                    assetReview.PLATE_NUMBER = item.PlateNumber;
                                    assetReview.CHASSIS_NUMBER = item.EquipmentNumber;
                                    assetReview.VEHICLE_SHORTNAME = "OBD";
                                    assetReview.DESCRIPTION = item.PlateNumber;
                                    assetReview.LISENSING_DATE = item.LisensingDate;
                                    assetReview.TAG_NUMBER = assetReview.ASSET_ID.Replace("CZ", "JK");
                                    assetReview.PURCHASE_DATE = DateTime.Now;
                                    assetReview.QUANTITY = 1;
                                    assetReview.ASSET_COST = 0;//fixedAssetsOrder.PurchasePrices;
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
                                    assetReview.OBDSTATUS = false;   
                                    assetReview.YTD_DEPRECIATION = 0;
                                    assetReview.ACCT_DEPRECIATION = 0;
                                    assetReview.FIXED_ASSETS_ORDERID = vguid;
                                    assetReview.CREATE_USER = cache[PubGet.GetUserKey].LoginName;
                                    assetReview.CREATE_DATE = DateTime.Now;
                                    if (assetInfoList.Any(x => x.PLATE_NUMBER == item.PlateNumber))
                                    {
                                        var assetInfo = assetInfoList.Where(x => x.GROUP_ID == "出租车")
                                            .First(x => x.PLATE_NUMBER == item.PlateNumber);
                                        if (assetInfo.BELONGTO_COMPANY.IsNullOrEmpty())
                                        {
                                            resultModel.ResultInfo += "导入的车牌号所属公司为空 ";
                                            return;
                                        }
                                        assetReview.BELONGTO_COMPANY = assetInfo.BELONGTO_COMPANY;
                                        assetReview.MANAGEMENT_COMPANY = assetInfo.MANAGEMENT_COMPANY;
                                        assetReview.MODEL_MAJOR = assetInfo.MODEL_MAJOR;
                                        assetReview.MODEL_MINOR = assetInfo.MODEL_MINOR;
                                        assetReview.EXP_ACCOUNT_SEGMENT = assetInfo.EXP_ACCOUNT_SEGMENT;
                                        assetReview.VEHICLE_STATE = assetInfo.VEHICLE_STATE;
                                        assetReview.OPERATING_STATE = assetInfo.OPERATING_STATE;
                                        assetReview.ORGANIZATION_NUM = assetInfo.ORGANIZATION_NUM;
                                    }
                                    else
                                    {
                                        resultModel.ResultInfo += "导入的车牌号车辆资产不存在 ";
                                        return;
                                    }
                                    assetReviewList.Add(assetReview);
                                }
                                var firstData = assetReviewList.First();
                                //获取订单部门
                                var departmentList = db.SqlQueryable<PurchaseDepartmentModel>(@"SELECT VGUID,Descrption
                                                                                        FROM Business_SevenSection
                                                                                        WHERE SectionVGUID = 'D63BD715-C27D-4C47-AB66-550309794D43'
                                                                                              AND AccountModeCode = '1002'
                                                                                              AND CompanyCode = '01'
                                                                                              AND Status = '1'
                                                                                              AND Code LIKE '10%'").ToList();
                                //根据资产所属公司生成采购订单
                                var companys = assetReviewList.GroupBy(x => new {x.BELONGTO_COMPANY})
                                    .Select(x => new {x.Key.BELONGTO_COMPANY}).ToList();
                                var orderList = new List<Business_FixedAssetsOrder>();
                                foreach (var company in companys)
                                {
                                    var companyName = ssList.Where(x => x.Abbreviation == company.BELONGTO_COMPANY).First().Descrption;
                                    var reviewData = assetReviewList.Where(x => x.BELONGTO_COMPANY == company.BELONGTO_COMPANY).ToList();
                                    var newFixedAssetOrder = new Business_FixedAssetsOrder();
                                    newFixedAssetOrder.VGUID = Guid.NewGuid();
                                    var autoID = "FixedAssetsOrder";
                                    var no = CreateNo.GetCreateNo(db, autoID);
                                    newFixedAssetOrder.OrderNumber = no;
                                    var department = firstData.ORGANIZATION_NUM.Replace("财务共享-", "");
                                    newFixedAssetOrder.PurchaseDepartmentIDs = departmentList.Where(x => x.Descrption == department).First().VGUID.ToString();
                                    newFixedAssetOrder.PurchaseGoods = firstData.GROUP_ID;
                                    newFixedAssetOrder.PurchaseGoodsVguid = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.PurchaseGoods == firstData.GROUP_ID).First().VGUID;
                                    var BusinessSubItem = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.VGUID == newFixedAssetOrder.PurchaseGoodsVguid).First().BusinessSubItem;
                                    var OrderVguid = db.Queryable<v_Business_BusinessTypeSet>().Where(x => x.BusinessSubItem1 == BusinessSubItem).First().VGUID.ToString();
                                    var dataSetting = db.Queryable<Business_CustomerBankSetting>().Where(x => x.OrderVGUID == OrderVguid && x.Isable).ToList();
                                    if (dataSetting.Count > 0)
                                    {
                                        var resultData = db.SqlQueryable<v_Business_CustomerBankInfo>(@"select a.*,b.Isable,b.OrderVGUID from Business_CustomerBankInfo as a 
                                        left join Business_CustomerBankSetting as b on a.VGUID = b.CustomerID
                                        left join v_Business_BusinessTypeSet as c on c.VGUID = b.OrderVGUID where b.Isable = '1'").Where(x => x.OrderVGUID == OrderVguid)
                                            .OrderBy(i => i.CreateTime, OrderByType.Desc).First();
                                        newFixedAssetOrder.PaymentInformationVguid = resultData.VGUID;
                                        newFixedAssetOrder.PaymentInformation = resultData.BankAccountName;
                                        newFixedAssetOrder.SupplierBankAccountName = resultData.BankAccountName;
                                        newFixedAssetOrder.SupplierBankAccount = resultData.BankAccount;
                                        newFixedAssetOrder.SupplierBank = resultData.Bank;
                                        newFixedAssetOrder.SupplierBankNo = resultData.BankNo;
                                        //newFixedAssetOrder.OrderFromVguid = "";
                                    }
                                    newFixedAssetOrder.OrderQuantity = reviewData.Count;
                                    newFixedAssetOrder.PurchasePrices = 0;
                                    newFixedAssetOrder.ContractAmount = 0;
                                    newFixedAssetOrder.AssetDescription = "OBD设备采购";
                                    newFixedAssetOrder.PayType = "转账";
                                    newFixedAssetOrder.OrderType = "OBD";
                                    newFixedAssetOrder.OrderFrom = "清算提交";
                                    var companylist = db.Queryable<Business_UserCompanySetDetail>().Where(x => x.OrderVGUID == OrderVguid && x.Isable)
                                        .OrderBy(i => i.CompanyCode).ToList();
                                    if (companylist.Count > 0)
                                    {
                                        var companyInfo = companylist.First(x => x.OrderVGUID == OrderVguid && x.CompanyName == companyName);
                                        newFixedAssetOrder.PayCompanyVguid = companyInfo.VGUID;
                                        newFixedAssetOrder.PayCompany = companyInfo.CompanyName;
                                        newFixedAssetOrder.CompanyBankName = companyInfo.PayBank;
                                        newFixedAssetOrder.CompanyBankAccount = companyInfo.PayAccount;
                                        newFixedAssetOrder.CompanyBankAccountName = companyInfo.PayBankAccountName;
                                        newFixedAssetOrder.AccountType = companyInfo.AccountType;
                                    }
                                    newFixedAssetOrder.CreateDate = DateTime.Now;
                                    newFixedAssetOrder.CreateUser = cache[PubGet.GetUserKey].LoginName;
                                    newFixedAssetOrder.SubmitStatus = FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt();
                                    orderList.Add(newFixedAssetOrder);
                                    foreach (var item in assetReviewList)
                                    {
                                        if (company.BELONGTO_COMPANY == item.BELONGTO_COMPANY)
                                        {
                                            item.FIXED_ASSETS_ORDERID = newFixedAssetOrder.VGUID;
                                        }
                                    }
                                }
                                db.Insertable<Business_FixedAssetsOrder>(orderList).ExecuteCommand();
                                db.Insertable<Business_AssetReview>(assetReviewList).ExecuteCommand();
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
    }
}