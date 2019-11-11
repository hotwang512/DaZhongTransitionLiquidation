using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SyntacticSugar;
using SqlSugar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using Business_AssetOrderDetails = DaZhongTransitionLiquidation.Areas.AssetPurchase.Models.Business_AssetOrderDetails;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.FixedAssetsOrder
{

    public class FixedAssetsOrderDetailController : BaseController
    {
        // GET: AssetManagement/FixedAssetsOrderDetail
        public FixedAssetsOrderDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            var vguid = Request["VGUID"].TryToGuid();
            if (vguid != Guid.Empty)
            {
                ViewBag.SubmitStatus = GetSubmitStatus(Request["VGUID"].TryToGuid());
            }
            return View();
        }
        public JsonResult SaveFixedAssetsOrder(Business_FixedAssetsOrder sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var model = db.Queryable<Business_FixedAssetsOrder>().Where(c => c.VGUID == sevenSection.VGUID);
                    if (model.Count() == 0)
                    {
                        var autoID = "FixedAssetsOrder";
                        var no = CreateNo.GetCreateNo(db, autoID);
                        sevenSection.OrderNumber = no;
                        sevenSection.OrderFrom = "手动新增";
                        sevenSection.CreateDate = DateTime.Now;
                        sevenSection.CreateUser = cache[PubGet.GetUserKey].LoginName;
                        sevenSection.SubmitStatus = FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt();
                        db.Insertable<Business_FixedAssetsOrder>(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.ChangeDate = DateTime.Now;
                        sevenSection.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                        db.Updateable<Business_FixedAssetsOrder>(sevenSection).IgnoreColumns(x => new { x.CreateDate, x.CreateUser, x.SubmitStatus,x.OrderNumber }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });

            return Json(resultModel);
        }
        public JsonResult ObsoleteFixedAssetsOrder(Business_FixedAssetsOrder sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var isAnySubmited = db.Queryable<Business_FixedAssetsOrder>().Any(c => c.VGUID == sevenSection.VGUID && c.SubmitStatus > FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt());
                    if (isAnySubmited)
                    {
                        resultModel.ResultInfo = "此状态下不允许作废！";
                        resultModel.IsSuccess = false;
                        resultModel.Status = "2";
                    }
                    else
                    {
                        db.Updateable<Business_FixedAssetsOrder>()
                            .UpdateColumns(it => new Business_FixedAssetsOrder() { SubmitStatus = 4 })
                            .Where(it => it.VGUID == sevenSection.VGUID).ExecuteCommand();
                        resultModel.IsSuccess = true;
                        resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                    }
                });
            });
            return Json(resultModel);
        }
        
        public JsonResult GetFixedAssetsOrder(Guid vguid)
        {
            Business_FixedAssetsOrder model = new Business_FixedAssetsOrder();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                model = db.Queryable<Business_FixedAssetsOrder>().Single(x => x.VGUID == vguid);
            });
            return Json(model, JsonRequestBehavior.AllowGet); ;
        }

        public JsonResult GetAssetOrderDetails(Guid AssetsOrderVguid,Guid PurchaseOrderSettingVguid)
        {
            var listFixedAssetsOrder = new List<Business_AssetOrderDetails>();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                if (db.Queryable<Business_AssetOrderDetails>().Any(x => x.AssetsOrderVguid == AssetsOrderVguid))
                {
                    listFixedAssetsOrder = db.Queryable<Business_AssetOrderDetails>().Where(x => x.AssetsOrderVguid == AssetsOrderVguid).OrderBy(c => c.AssetManagementCompany).ToList();
                }
            });
            if (listFixedAssetsOrder.Count == 0)
            {
                listFixedAssetsOrder = GetDefaultAssetOrderDetails(AssetsOrderVguid, PurchaseOrderSettingVguid);
            }
            return Json(listFixedAssetsOrder, JsonRequestBehavior.AllowGet); ;
        }

        public List<Business_AssetOrderDetails> GetDefaultAssetOrderDetails(Guid AssetsOrderVguid,Guid PurchaseOrderSettingVguid)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var list = new List<Business_AssetOrderDetails>();
            var listCompany = new List<string>();
            //获取采购物品配置的资产管理公司
            DbBusinessDataService.Command(db =>
            {
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
                        model.CreateUser = cache[PubGet.GetUserKey].LoginName;
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

        public JsonResult DeleteApprovalFile(Guid vguid)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var updateObj = db.Queryable<Business_AssetOrderDetails>().Where(c => c.VGUID == vguid).First();
                    updateObj.ApprovalFormFileName = "";
                    updateObj.ApprovalFormFilePath = "";
                    db.Updateable(updateObj).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult UpdateAssetNum(Guid vguid, int AssetNum)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var updateObj = db.Queryable<Business_AssetOrderDetails>().Where(c => c.VGUID == vguid).First();
                    db.Updateable(updateObj).UpdateColumns(it => new { it.AssetNum }).ReSetValue(it => it.AssetNum == AssetNum).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
            });
            return Json(resultModel);
        }
        public JsonResult UploadLocalFile(Guid Vguid, HttpPostedFileBase File)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            if (File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "ApprovalFormFile\\" + newFileName;
                var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath;
                try
                {
                    File.SaveAs(filePath);
                    DbBusinessDataService.Command(db =>
                    {
                        var result = db.Ado.UseTran(() =>
                        {
                            var sevenSection = db.Queryable<Business_AssetOrderDetails>().Where(c => c.VGUID == Vguid).First();
                            sevenSection.ApprovalFormFilePath = uploadPath;
                            sevenSection.ApprovalFormFileName = File.FileName;
                            sevenSection.ChangeDate = DateTime.Now;
                            sevenSection.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                            db.Updateable(sevenSection).UpdateColumns(x => new {
                                x.ChangeDate,
                                x.ChangeUser,
                                x.ApprovalFormFilePath,
                                x.ApprovalFormFileName
                            }).ExecuteCommand();
                        });
                        resultModel.IsSuccess = result.IsSuccess;
                        resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", filePath, ex.ToString()));
                }
            }
            return Json(resultModel);
        }
        public JsonResult UploadContractFile(Guid Vguid, HttpPostedFileBase File)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            if (File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "PurchaseContract\\" + newFileName;
                var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath;
                try
                {
                    File.SaveAs(filePath);
                    DbBusinessDataService.Command(db =>
                    {
                        var result = db.Ado.UseTran(() =>
                        {
                            var sevenSection = db.Queryable<Business_FixedAssetsOrder>().Where(c => c.VGUID == Vguid).First();
                            if (sevenSection != null)
                            {
                                sevenSection.ContractFilePath = uploadPath;
                                sevenSection.ContractName = File.FileName;
                                sevenSection.ChangeDate = DateTime.Now;
                                sevenSection.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                                db.Updateable(sevenSection).UpdateColumns(x => new {
                                    x.ChangeDate,
                                    x.ChangeUser,
                                    x.ContractFilePath,
                                    x.ContractName
                                }).ExecuteCommand();
                            }
                        });
                        resultModel.IsSuccess = result.IsSuccess;
                        resultModel.ResultInfo = uploadPath;
                        resultModel.ResultInfo2 = File.FileName;
                        resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", filePath, ex.ToString()));
                }
            }
            return Json(resultModel);
        }
        public JsonResult GetUseDepartment()
        {
            var departmentData = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                departmentData = db.Queryable<Business_SevenSection>().Where(x =>
                    x.SectionVGUID == "D63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == "1002" &&
                    x.CompanyCode == "01" && x.Status == "1" && x.Code.StartsWith("10")).ToList();
            });
            return Json(departmentData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPurchaseGoods(int OrderCategory,Guid[] PurchaseDepartment,string OrderType)
        {
            var PurchaseDepartmentStr = "";
            var orderTypeData = new List<Business_PurchaseOrderSetting>();
            DbBusinessDataService.Command(db =>
            {
                if (PurchaseDepartment == null)
                {
                    orderTypeData = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.OrderCategory == OrderCategory).ToList();
                }
                else
                {
                    foreach (var str in PurchaseDepartment)
                    {
                        PurchaseDepartmentStr = PurchaseDepartmentStr + str + "','";
                    }
                    PurchaseDepartmentStr = PurchaseDepartmentStr.Substring(0,PurchaseDepartmentStr.Length - 3);
                    if (OrderType == "Vehicle")
                    {
                        orderTypeData = db.SqlQueryable<Business_PurchaseOrderSetting>(@"SELECT DISTINCT bpos.* FROM  Business_PurchaseOrderSetting bpos INNER JOIN
                    Business_PurchaseDepartment bpd ON bpos.VGUID = bpd.PurchaseOrderSettingVguid
                    WHERE OrderCategory = '0' and bpos.PurchaseGoods like '%出租车%' And bpd.DepartmentVguid IN ('" + PurchaseDepartmentStr + "')").ToList();
                    }else if (OrderType == "OBD")
                    {
                        orderTypeData = db.SqlQueryable<Business_PurchaseOrderSetting>(@"SELECT DISTINCT bpos.* FROM  Business_PurchaseOrderSetting bpos INNER JOIN
                    Business_PurchaseDepartment bpd ON bpos.VGUID = bpd.PurchaseOrderSettingVguid
                    WHERE OrderCategory = '0' and bpos.PurchaseGoods like '%OBD%' And bpd.DepartmentVguid IN ('" + PurchaseDepartmentStr + "')").ToList();
                    }
                    else
                    {
                        orderTypeData = db.SqlQueryable<Business_PurchaseOrderSetting>(@"SELECT DISTINCT bpos.* FROM  Business_PurchaseOrderSetting bpos INNER JOIN
                    Business_PurchaseDepartment bpd ON bpos.VGUID = bpd.PurchaseOrderSettingVguid
                    WHERE OrderCategory = '0'and bpos.PurchaseGoods not in ('出租车','OBD设备') And bpd.DepartmentVguid IN ('" + PurchaseDepartmentStr + "')").ToList();
                    }
                }
            });
            return Json(orderTypeData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPurchaseDepartmentList(string OrderType)
        {
            var list = new List<Business_PurchaseDepartment>();
            DbBusinessDataService.Command(db =>
            {
                if (OrderType == "Vehicle")
                {
                    if (db.Queryable<Business_PurchaseOrderSetting>().Any(x => x.PurchaseGoods.Contains("出租车")))
                    {
                        var orderSetting = db.Queryable<Business_PurchaseOrderSetting>()
                            .Where(x => x.PurchaseGoods.Contains("出租车")).First();
                        list = db.SqlQueryable<Business_PurchaseDepartment>(@"SELECT * FROM Business_PurchaseDepartment where PurchaseOrderSettingVguid ='"+ orderSetting.VGUID +"'").ToList();
                    }
                }
                else if(OrderType == "OBD")
                {
                    if (db.Queryable<Business_PurchaseOrderSetting>().Any(x => x.PurchaseGoods.Contains("OBD")))
                    {
                        var orderSetting = db.Queryable<Business_PurchaseOrderSetting>()
                            .Where(x => x.PurchaseGoods.Contains("OBD")).First();
                        list = db.SqlQueryable<Business_PurchaseDepartment>(@"SELECT * FROM Business_PurchaseDepartment where PurchaseOrderSettingVguid ='" + orderSetting.VGUID + "'").ToList();
                    }
                }
                else
                {
                    if (db.Queryable<Business_PurchaseOrderSetting>().Any(x => !x.PurchaseGoods.Contains("OBD") && !x.PurchaseGoods.Contains("出租车") && x.OrderCategory == 0))
                    {
                        var orderSettingList = db.Queryable<Business_PurchaseOrderSetting>().Where(x => !x.PurchaseGoods.Contains("OBD") && !x.PurchaseGoods.Contains("出租车") && x.OrderCategory == 0).Select(x => x.VGUID).ToList();
                        list = db.Queryable<Business_PurchaseDepartment>().Where(x => orderSettingList.Contains(x.PurchaseOrderSettingVguid)).GroupBy(x => x.DepartmentVguid).GroupBy(x => x.DepartmentName).Select(x => new Business_PurchaseDepartment() {DepartmentVguid = x.DepartmentVguid,DepartmentName = x.DepartmentName}).ToList();
                    }
                }
            });

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SubmitFixedAssetsOrder(Guid vguid,string OrderType)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
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
                            .Where(x => x.BankAccount == model.SupplierBankAccount).First().CompanyOrPerson; 
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
                            if (model.OrderFrom != "清算提交" && OrderType != "OBD")
                            {
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
                                purchaseAssignmodel.OrderType = model.OrderType;
                                if (OrderType != "Office")
                                {
                                    db.Insertable<Business_PurchaseAssign>(purchaseAssignmodel).ExecuteCommand();
                                    var fundClearingModel = Mapper.Map<Business_FundClearing>(purchaseAssignmodel);
                                    fundClearingModel.VGUID = Guid.NewGuid();
                                    var companys = db.Queryable<Business_PurchaseManagementCompany>()
                                        .Where(x => x.PurchaseOrderSettingVguid == orderModel.PurchaseGoodsVguid && x.IsCheck).ToList();
                                    var liquidationDistributionList = new List<Business_LiquidationDistribution>();
                                    var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                                        x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
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
                                    db.Insertable<Business_FundClearing>(fundClearingModel).ExecuteCommand();
                                    db.Insertable<Business_LiquidationDistribution>(liquidationDistributionList).ExecuteCommand();
                                }
                                else
                                {
                                    var fundClearingModel = Mapper.Map<Business_FundClearing>(purchaseAssignmodel);
                                    fundClearingModel.VGUID = Guid.NewGuid();
                                    db.Insertable<Business_FundClearing>(fundClearingModel).ExecuteCommand();
                                }
                                
                            }
                            db.Updateable<Business_FixedAssetsOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                            if (OrderType == "OBD")
                            {
                                db.Updateable<Business_AssetReview>().UpdateColumns(it => it.OBDSTATUS == true)
                                    .Where(x => x.FIXED_ASSETS_ORDERID == orderModel.VGUID).ExecuteCommand();
                            }
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
        public int GetSubmitStatus(Guid vguid)
        {
            var model = new Business_FixedAssetsOrder();
            DbBusinessDataService.Command(db =>
            {
                model = db.Queryable<Business_FixedAssetsOrder>().Single(x => x.VGUID == vguid);
            });
            return model.SubmitStatus.TryToInt();
        }
        public JsonResult GetCustomerBankInfoList(string PurchaseOrderSetting)
        {
            var PurchaseOrderSettingGuid = PurchaseOrderSetting.TryToGuid();
            if (PurchaseOrderSettingGuid != Guid.Empty)
            {
                var jsonResult = new JsonResultModel<v_Business_CustomerBankInfo>();
                DbBusinessDataService.Command(db =>
                {
                    var BusinessSubItem = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.VGUID == PurchaseOrderSettingGuid).First().BusinessSubItem;
                    var OrderVguid = db.Queryable<v_Business_BusinessTypeSet>().Where(x => x.BusinessSubItem1 == BusinessSubItem).First().VGUID.ToString();
                    var data = db.Queryable<Business_CustomerBankSetting>().Where(x => x.OrderVGUID == OrderVguid && x.Isable).ToList();
                    if (data.Count > 0)
                    {
                        jsonResult.Rows = db.SqlQueryable<v_Business_CustomerBankInfo>(@"select a.*,b.Isable,b.OrderVGUID from Business_CustomerBankInfo as a 
left join Business_CustomerBankSetting as b on a.VGUID = b.CustomerID
left join v_Business_BusinessTypeSet as c on c.VGUID = b.OrderVGUID where b.Isable = '1'").Where(x => x.OrderVGUID == OrderVguid)
                            .OrderBy(i => i.CreateTime, OrderByType.Desc).ToList();
                    }
                    else
                    {
                        jsonResult.Rows = db.SqlQueryable<v_Business_CustomerBankInfo>(@"select * from Business_CustomerBankInfo").ToList();
                    }
                });
                return Json(jsonResult, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var jsonResult = new JsonResultModel<Business_CustomerBankInfo>();
                DbBusinessDataService.Command(db =>
                {
                    jsonResult.Rows = db.Queryable<Business_CustomerBankInfo>()
                        .OrderBy(i => i.CreateTime, OrderByType.Desc).ToList();
                });
                return Json(jsonResult, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetCustomerBankInfo(Guid vguid)
        {
            var model = new Business_CustomerBankInfo();
            DbBusinessDataService.Command(db =>
            {
                model = db.Queryable<Business_CustomerBankInfo>().Where(c => c.VGUID == vguid).First();
            });
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCompanyBankInfoDropdown()
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
            var list = new List<SevenSectionDropdown>();
            DbBusinessDataService.Command(db =>
            {
                if (db.Queryable<Business_SevenSection>().Where(c => c.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && c.Status == "1" && c.AccountModeCode == AccountModeCode).Count() > 0)
                {
                    list = db.Queryable<Business_SevenSection>().Where(c => c.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && c.Status == "1" && c.AccountModeCode == AccountModeCode).Select(c => new SevenSectionDropdown { VGUID = c.VGUID, Descrption = c.Descrption }).ToList();
                }
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCompanyBankInfoDropdownByCode(Guid PurchaseOrderSetting)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
            var list = new List<Business_UserCompanySetDetail>();
            DbBusinessDataService.Command(db =>
            {
                var BusinessSubItem = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.VGUID == PurchaseOrderSetting).First().BusinessSubItem;
                var OrderVguid = db.Queryable<v_Business_BusinessTypeSet>().Where(x => x.BusinessSubItem1 == BusinessSubItem).First().VGUID.ToString();
                var data = db.Queryable<Business_UserCompanySetDetail>().Where(x => x.OrderVGUID == OrderVguid && x.Isable).ToList();
                if (data.Count > 0)
                {
                    list = db.Queryable<Business_UserCompanySetDetail>().Where(x => x.OrderVGUID == OrderVguid && x.AccountModeCode == AccountModeCode && x.Isable)
                        .OrderBy(i => i.CompanyCode).ToList();
                }
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCompanyBankInfo(Guid Vguid)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var model = new Business_UserCompanySetDetail();
            DbBusinessDataService.Command(db =>
            {
                model = db.Queryable<Business_UserCompanySetDetail>().Where(c => c.VGUID == Vguid).First();
            });
            return Json(model, JsonRequestBehavior.AllowGet);
        }


        public JsonResult UploadToImageServer(Guid Vguid, string ImageBase64Str, string AttachmentType)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            var imageServerUrl = ConfigSugar.GetAppString("ImageServerUrl");
            var resultData = FileUploadHelper.UploadToImageServer(ImageBase64Str);
            var saveModel = new Business_AssetAttachmentList();
            if (resultData != "")
            {
                var modelData = resultData.JsonToModel<JsonResultFileModelApi<Api_FileInfo>>();
                if (modelData.code == 0)
                {
                    var fileData = modelData.data[0];
                    if (!fileData.fileName.IsNullOrEmpty())
                    {
                        DbBusinessDataService.Command(db =>
                        {
                            var result = db.Ado.UseTran(() =>
                            {
                                saveModel.VGUID = Guid.NewGuid();
                                saveModel.Attachment = imageServerUrl + fileData.fileName;
                                saveModel.AttachmentType = AttachmentType;
                                saveModel.CreatePerson = cache[PubGet.GetUserKey].LoginName;
                                saveModel.AssetOrderVGUID = Vguid;
                                saveModel.CreateTime = DateTime.Now;
                                db.Insertable<Business_AssetAttachmentList>(saveModel).ExecuteCommand();
                            });
                            resultModel.IsSuccess = result.IsSuccess;
                            resultModel.ResultInfo = saveModel.Attachment;
                            resultModel.ResultInfo2 = fileData.fileName;
                            resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
                        });
                    }
                }
            }
            return Json(resultModel);
        }
        public JsonResult AllUploadLocalFile(Guid Vguid, string AttachmentType, HttpPostedFileBase File)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            var imageServerUrl = ConfigSugar.GetAppString("ImageServerUrl");
            if (File != null)
            {
                var uploadPath = ConfigSugar.GetAppString("UploadPath") + "\\" + "AssetAttachFile\\";
                var fileName = File.FileName;
                var isExits = true;
                var i = 1;
                while (isExits)
                {
                    if (FileHelper.Contains(System.AppDomain.CurrentDomain.BaseDirectory + uploadPath, fileName, false))
                    {
                        fileName = File.FileName.Split(".")[0] + "(" + i + ")." + File.FileName.Split(".")[1];
                    }
                    else
                    {
                        isExits = false;
                    }
                    i++;
                }
                var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath + fileName;
                try
                {
                    File.SaveAs(filePath);
                    var FileBase64Str = FileHelper.FileToBase64(filePath);
                    var resultData = FileUploadHelper.UploadToFileServer(fileName, FileBase64Str);

                    if (resultData != "")
                    {
                        var modelData = resultData.JsonToModel<JsonResultFileModelApi<Api_FileInfo>>();
                        if (modelData.code == 0)
                        {
                            var fileData = modelData.data[0];
                            if (!fileData.fileName.IsNullOrEmpty())
                            {
                                DbBusinessDataService.Command(db =>
                                {
                                    var result = db.Ado.UseTran(() =>
                                    {
                                        var sevenSection = new Business_AssetAttachmentList();
                                        sevenSection.VGUID = Guid.NewGuid();
                                        sevenSection.AssetOrderVGUID = Vguid;
                                        sevenSection.Attachment = fileData.fileId;//"\\" + uploadPath + fileName;
                                        sevenSection.AttachmentType = AttachmentType;
                                        sevenSection.CreateTime = DateTime.Now;
                                        sevenSection.CreatePerson = cache[PubGet.GetUserKey].LoginName;
                                        db.Insertable<Business_AssetAttachmentList>(sevenSection).ExecuteCommand();
                                    });
                                    resultModel.IsSuccess = result.IsSuccess;
                                    resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", filePath, ex.ToString()));
                }
            }
            return Json(resultModel);
        }
        public JsonResult GetAttachmentInfo(Guid VGUID)
        {
            var imageServerUrl = ConfigSugar.GetAppString("ImageServerUrl");
            List<Business_AssetAttachmentList> BAList = new List<Business_AssetAttachmentList>();
            DbBusinessDataService.Command(db =>
                {
                    BAList = db.Queryable<Business_AssetAttachmentList>().Where(x => x.AssetOrderVGUID == VGUID)
                        .ToList();
                });
            BAList.ForEach(x => { x.Attachment = imageServerUrl + x.Attachment; });
            return Json(BAList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPaymentInformationByBusinessSubItem(Guid PurchaseGoodsVguid)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
            var result = new Business_UserCompanySetDetail();
            DbBusinessDataService.Command(db =>
            {
                var goodsData = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.VGUID == PurchaseGoodsVguid)
                    .First();
                var businessTypeSetData = db.Queryable<v_Business_BusinessTypeSet>()
                    .Where(x => x.BusinessSubItem1 == goodsData.BusinessSubItem).First();
                var businessTypeSetDataVguid = businessTypeSetData.VGUID.ToString();
                if (db.Queryable<Business_UserCompanySetDetail>()//Business_CustomerBankSetting
                    .Any(x => x.OrderVGUID == businessTypeSetDataVguid && x.Isable &&
                              x.AccountModeCode == AccountModeCode))
                {
                    result = db.Queryable<Business_UserCompanySetDetail>()
                        .Where(x => x.OrderVGUID == businessTypeSetDataVguid && x.Isable && x.AccountModeCode == AccountModeCode).First();
                    result.VGUID = db.Queryable<Business_SevenSection>()
                        .Where(x => x.Descrption == result.CompanyName).First().VGUID;
                }
            });
            return Json(result);
        }
        public JsonResult DeleteAttachment(Guid VGUID)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                int saveChanges = 1;
                //删除主表信息
                saveChanges = db.Deleteable<Business_AssetAttachmentList>(x => x.VGUID == VGUID).ExecuteCommand();
                resultModel.IsSuccess = saveChanges == 1;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }

        public JsonResult PendingPaymentAttachmentUpload(Guid PaymentVoucherVguid, Guid Vguid)
        {
            var resultModel = new ResultModel<string,string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                //请求清算平台、待付款请求生成支付凭证接口
                var pendingPaymentmodel = new PendingPaymentModel();
                pendingPaymentmodel.IdentityToken = cache[PubGet.GetUserKey].Token;
                pendingPaymentmodel.FunctionSiteId = "61";
                pendingPaymentmodel.OperatorIP = GetSystemInfo.GetClientLocalIPv4Address();
                //统计附件信息
                var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>()
                    .Where(x => x.AssetOrderVGUID == Vguid).ToList();
                pendingPaymentmodel.PaymentReceipt =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "付款凭证").ToList());
                pendingPaymentmodel.InvoiceReceipt =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList());
                pendingPaymentmodel.ApprovalReceipt =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "OA审批单").ToList());
                pendingPaymentmodel.Contract =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "合同").ToList());
                pendingPaymentmodel.DetailList =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "清单、清册").ToList());
                pendingPaymentmodel.OtherReceipt =
                    JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "其他").ToList());

                var apiReault = PendingPaymentAttachmentApi(pendingPaymentmodel, PaymentVoucherVguid);
                var pendingRedult = apiReault.JsonToModel<PendingResultModel>();
                resultModel.IsSuccess = pendingRedult.success;
                resultModel.Status = pendingRedult.success ? "1":"0";
                resultModel.ResultInfo = pendingRedult.code;
                resultModel.ResultInfo2 = pendingRedult.message;
                if (!pendingRedult.success)
                {
                    LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public string PendingPaymentAttachmentApi(PendingPaymentModel model,Guid PaymentVoucherVguid)
        {
            var url = ConfigSugar.GetAppString("PendingPaymentAttachmentUrl");
            var data = "{" +
                       "\"IdentityToken\":\"{IdentityToken}\",".Replace("{IdentityToken}", model.IdentityToken) +
                       "\"FunctionSiteId\":\"{FunctionSiteId}\",".Replace("{FunctionSiteId}", "61") +
                       "\"OperatorIP\":\"{OperatorIP}\",".Replace("{OperatorIP}",
                           GetSystemInfo.GetClientLocalIPv4Address()) +
                       "\"vguid\":\"{vguid}\",".Replace("{vguid}", PaymentVoucherVguid.ToString());
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
        /// <summary>
        /// 获取物品类型
        /// </summary>
        /// <returns></returns>
        public JsonResult GetGoodsModelDropDown(string Goods)
        {
            var list = new List<Business_SevenSection>();
            DbBusinessDataService.Command(db =>
            {
                if (Goods == "出租车")
                {
                    list = db.SqlQueryable<Business_SevenSection>(@"SELECT * FROM Business_SevenSection WHERE SectionVGUID = 'F63BD715-C27D-4C47-AB66-550309794D43'
AND AccountModeCode = '1002' AND status = 1 AND CompanyCode = '01' AND code LIKE '10%'").ToList();
                }
            });
            return Json(list, JsonRequestBehavior.AllowGet);
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
                    //删除订单数量关联表信息
                    db.Deleteable<Business_PurchaseOrderNum>(x => vguids.Contains(x.FaxOrderVguid)).ExecuteCommand();
                    resultModel.IsSuccess = saveChanges == vguids.Count;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
    }
}