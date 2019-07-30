using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;
using Business_AssetOrderDetails = DaZhongTransitionLiquidation.Areas.AssetPurchase.Models.Business_AssetOrderDetails;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Controllers.TaxFeeOrder
{
    public class TaxFeeOrderDetailController : BaseController
    {
        // GET: AssetPurchase/TaxFeeOrderDetail
        public TaxFeeOrderDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult SaveTaxFeeOrder(SaveBusiness_TaxFeeOrderModel sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var model = db.Queryable<Business_TaxFeeOrder>().Where(c => c.VGUID == sevenSection.VGUID);
                    if (model.Count() == 0)
                    {
                        var orderNumberLeft = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
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
                        sevenSection.OrderNumber = orderNumberLeft + maxOrderNumRight.ToString().PadLeft(4, '0');
                        var OrderNumList = sevenSection.OrderNumData.Split(",");
                        foreach (var item in OrderNumList)
                        {
                            var orderNumModel = new Business_PurchaseOrderNum();
                            orderNumModel.VGUID = Guid.NewGuid();
                            orderNumModel.FaxOrderVguid = sevenSection.VGUID;
                            orderNumModel.PayItemCode = sevenSection.PayItemCode;
                            orderNumModel.PayItem = sevenSection.PayItem;
                            orderNumModel.FixedAssetOrderVguid = item.TryToGuid();
                            orderNumModel.OrderQuantity = db.Queryable<Business_FixedAssetsOrder>()
                                .Where(x => x.VGUID == orderNumModel.FixedAssetOrderVguid).First().OrderQuantity;
                            orderNumModel.CreateDate = DateTime.Now;
                            sevenSection.CreateUser = cache[PubGet.GetUserKey].UserName;
                            db.Insertable<Business_PurchaseOrderNum>(orderNumModel).ExecuteCommand();
                        }
                        sevenSection.CreateDate = DateTime.Now;
                        sevenSection.CreateUser = cache[PubGet.GetUserKey].UserName;
                        sevenSection.SubmitStatus = FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt();
                        db.Insertable<Business_TaxFeeOrder>(sevenSection).ExecuteCommand();

                        //请求清算平台、待付款请求生成支付凭证接口
                        var pendingPaymentmodel = new PendingPaymentModel();
                        //统计附件信息
                        var assetAttachmentList = db.Queryable<Business_AssetAttachmentList>().Where(x => x.AssetOrderVGUID == sevenSection.VGUID).ToList();
                        pendingPaymentmodel.PaymentReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "付款凭证").ToList());
                        pendingPaymentmodel.InvoiceReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList());
                        pendingPaymentmodel.ApprovalReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "OA审批单").ToList());
                        pendingPaymentmodel.Contract = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "合同").ToList());
                        pendingPaymentmodel.DetailList = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "清单、清册").ToList());
                        pendingPaymentmodel.OtherReceipt = JoinStr(assetAttachmentList.Where(x => x.AttachmentType == "其他").ToList());
                        var orderListData = db.Queryable<v_Business_BusinessTypeSet>()
                            .Where(x => x.BusinessSubItem1 == sevenSection.PayItemCode).First();

                        pendingPaymentmodel.ServiceCategory = orderListData.BusinessProject;
                        pendingPaymentmodel.BusinessProject = orderListData.BusinessSubItem1.Split("|")[0] + "|"
                                                              + orderListData.BusinessSubItem1.Substring(orderListData.BusinessSubItem1.LastIndexOf("|") + 1, orderListData.BusinessSubItem1.Length - orderListData.BusinessSubItem1.LastIndexOf("|") - 1);
                        //根据供应商账号找到供应商类别
                        pendingPaymentmodel.PaymentCompany = db.Queryable<Business_CustomerBankInfo>()
                            .Where(x => x.BankAccount == sevenSection.SupplierBankAccount).First().CompanyOrPerson; ;
                        pendingPaymentmodel.CollectBankAccountName = sevenSection.SupplierBankAccountName;
                        pendingPaymentmodel.CollectBankAccouont = sevenSection.SupplierBankAccount;
                        pendingPaymentmodel.CollectBankName = sevenSection.SupplierBank;
                        pendingPaymentmodel.CollectBankNo = sevenSection.SupplierBankNo;
                        pendingPaymentmodel.PaymentMethod = sevenSection.PayType;

                        pendingPaymentmodel.IdentityToken = cache[PubGet.GetUserKey].Token;
                        pendingPaymentmodel.FunctionSiteId = "61";
                        pendingPaymentmodel.OperatorIP = GetSystemInfo.GetClientLocalIPv4Address();
                        pendingPaymentmodel.invoiceNumber = assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count().ToString();
                        pendingPaymentmodel.numberOfAttachments = (assetAttachmentList.Count() - assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count()).ToString();
                        pendingPaymentmodel.Amount = sevenSection.SumPayment.ToString();
                        pendingPaymentmodel.Summary = sevenSection.PurchaseDescription;
                        pendingPaymentmodel.AccountSetCode = cache[PubGet.GetUserKey].AccountModeCode + "|" + cache[PubGet.GetUserKey].CompanyCode;

                        var apiReault = PendingPaymentApi(pendingPaymentmodel);
                        var pendingRedult = apiReault.JsonToModel<JsonResultModelApi<Api_PendingPayment>>();
                        if (pendingRedult.success)
                        {
                            var orderModel = db.Queryable<Business_TaxFeeOrder>()
                                .Where(x => x.VGUID == sevenSection.VGUID).First();
                            orderModel.PaymentVoucherVguid = pendingRedult.data.vguid;
                            orderModel.PaymentVoucherUrl = pendingRedult.data.url;
                            db.Updateable<Business_TaxFeeOrder>(orderModel).UpdateColumns(x => new { x.PaymentVoucherUrl, x.PaymentVoucherVguid }).ExecuteCommand();
                        }
                        else
                        {
                            LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                        }
                    }
                    else
                    {
                        sevenSection.ChangeDate = DateTime.Now;
                        sevenSection.ChangeUser = cache[PubGet.GetUserKey].UserName;
                        
                        db.Updateable<Business_TaxFeeOrder>(sevenSection).UpdateColumns(x => new { x.PurchaseDescription, x.PaymentInformationVguid, x.PaymentInformation,x.SupplierBankAccountName,x.SupplierBankAccount,x.SupplierBank,x.SupplierBankNo,x.PayType,x.PayCompanyVguid,x.PayCompany,x.CompanyBankName,x.CompanyBankAccount,x.CompanyBankAccountName,x.AccountType,x.ChangeDate,x.ChangeUser }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
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
        public JsonResult GetTaxFeeOrder(Guid vguid)
        {
            Business_TaxFeeOrder model = new Business_TaxFeeOrder();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                model = db.Queryable<Business_TaxFeeOrder>().Single(x => x.VGUID == vguid);
            });
            return Json(model, JsonRequestBehavior.AllowGet); ;
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
                            sevenSection.ChangeUser = cache[PubGet.GetUserKey].UserName;
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
        public JsonResult SubmitTaxFeeOrder(Guid vguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var model = db.Queryable<Business_TaxFeeOrder>().Where(c => c.VGUID == vguid).First();
                    model.SubmitStatus = FixedAssetsSubmitStatusEnum.Submited.TryToInt();
                    model.SubmitDate = DateTime.Now;
                    model.SubmitUser = cache[PubGet.GetUserKey].UserName;
                    db.Updateable<Business_TaxFeeOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess.ObjToBool() ? "1" : "0";
            });
            return Json(resultModel);
        }
        public int GetSubmitStatus(Guid vguid)
        {
            var model = new Business_TaxFeeOrder();
            DbBusinessDataService.Command(db =>
            {
                model = db.Queryable<Business_TaxFeeOrder>().Single(x => x.VGUID == vguid);
            });
            return model.SubmitStatus.TryToInt();
        }
        public JsonResult GetCustomerBankInfoList(string PayItem)
        {
            if (PayItem != "")
            {
                var jsonResult = new JsonResultModel<v_Business_CustomerBankInfo>();
                DbBusinessDataService.Command(db =>
                {
                    var OrderVguid = db.Queryable<v_Business_BusinessTypeSet>().Where(x => x.BusinessSubItem1 == PayItem).First().VGUID.ToString();
                    var data = db.Queryable<Business_CustomerBankSetting>().Where(x => x.OrderVGUID == OrderVguid).ToList();
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
        public JsonResult GetCompanyBankInfoDropdownByCode(string PayItem)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
            var list = new List<Business_UserCompanySetDetail>();
            if (!PayItem.IsNullOrEmpty())
            {
                DbBusinessDataService.Command(db =>
                {
                    var OrderVguid = db.Queryable<v_Business_BusinessTypeSet>().Where(x => x.BusinessSubItem1 == PayItem).First().VGUID.ToString();
                    var data = db.Queryable<Business_UserCompanySetDetail>().Where(x => x.OrderVGUID == OrderVguid).ToList();
                    if (data.Count > 0)
                    {
                        list = db.Queryable<Business_UserCompanySetDetail>().Where(x => x.OrderVGUID == OrderVguid && x.AccountModeCode == AccountModeCode && x.Isable)
                            .OrderBy(i => i.CompanyCode).ToList();
                    }
                });
            }
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
                                saveModel.CreatePerson = cache[PubGet.GetUserKey].UserName;
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
                                        sevenSection.CreatePerson = cache[PubGet.GetUserKey].UserName;
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
        public JsonResult GetPaymentInformationByBusinessSubItem(Guid PayItemVguid)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
            var result = new Business_UserCompanySetDetail();
            DbBusinessDataService.Command(db =>
            {
                var goodsData = db.Queryable<Business_PurchaseOrderSetting>().Where(x => x.VGUID == PayItemVguid)
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
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
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
                resultModel.Status = pendingRedult.success ? "1" : "0";
                resultModel.ResultInfo = pendingRedult.code;
                resultModel.ResultInfo2 = pendingRedult.message;
                if (!pendingRedult.success)
                {
                    LogHelper.WriteLog(string.Format("result:{0}", pendingRedult.message));
                }
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        public string PendingPaymentAttachmentApi(PendingPaymentModel model, Guid PaymentVoucherVguid)
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

        public JsonResult GetPayItem()
        {
            var list = new List<BusinessProjectModel>();
            DbBusinessDataService.Command(db =>
            {
                list = db.SqlQueryable<BusinessProjectModel>(@"SELECT BusinessSubItem1,BusinessProject FROM v_Business_BusinessTypeSet WHERE BusinessSubItem1 LIKE 'cz|03|0301|%' AND BusinessSubItem1 != 'cz|03|0301|030101'").ToList();
                foreach (var item in list)
                {
                    item.BusinessProject = item.BusinessProject.Substring(item.BusinessProject.LastIndexOf("|")+1,
                        item.BusinessProject.Length - item.BusinessProject.LastIndexOf("|")-1);
                }
            });

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFeeByVehicleModel(string PayItem, string VehicleModel)
        {
            var list = new List<Business_VehicleExtrasFeeSetting>();
            DbBusinessDataService.Command(db =>
                {
                    if (db.Queryable<Business_VehicleExtrasFeeSetting>().Any(x =>
                        x.BusinessSubItem == PayItem && x.VehicleModelCode == VehicleModel && x.Status == 1))
                    {
                        list = db.Queryable<Business_VehicleExtrasFeeSetting>().Where(x =>
                            x.BusinessSubItem == PayItem && x.VehicleModelCode == VehicleModel && x.Status == 1).ToList();
                    }
                });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPurchaseOrderNum(string PayItemCode)
        {
            var list = new List<PurchaseOrderSelectNum>();
            DbBusinessDataService.Command(db =>
            {
                var submitedList = db.Queryable<Business_FixedAssetsOrder>().Where(x =>
                    x.SubmitStatus == FixedAssetsSubmitStatusEnum.Submited.TryToInt()).ToList();
                if (submitedList.Count > 0)
                {
                    var sql =
                        "SELECT fao.VGUID AS FixedAssetsOrderVguid,fao.OrderQuantity,fao.OrderNumber, pon.PayItemCode FROM (SELECT * FROM dbo.Business_FixedAssetsOrder WHERE SubmitStatus = 2 ) fao LEFT JOIN (SELECT * FROM Business_PurchaseOrderNum WHERE PayItemCode = '" +
                        PayItemCode + "') AS  pon ON fao.VGUID = pon.FixedAssetOrderVguid";
                    list = db.SqlQueryable<PurchaseOrderSelectNum>(sql).Where(x => x.PayItemCode == null).ToList();
                }
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPurchaseOrderNumDetail(string PayItemCode,Guid TaxFeeOrderVguid)
        {
            var list = new List<PurchaseOrderSelectNum>();
            DbBusinessDataService.Command(db =>
            {
                var FaxOrderVguid = db.Queryable<Business_TaxFeeOrder>().Where(x => x.VGUID == TaxFeeOrderVguid).First().VGUID;
                var FixedAssetsOrderVguid = db.Queryable<Business_PurchaseOrderNum>()
                    .Where(x => x.FaxOrderVguid == FaxOrderVguid).First().FixedAssetOrderVguid;
                    
                var submitedList = db.Queryable<Business_FixedAssetsOrder>().Where(x =>
                    x.SubmitStatus == FixedAssetsSubmitStatusEnum.Submited.TryToInt()).ToList();
                if (submitedList.Count > 0)
                {
                    var sql =
                        @"SELECT fao.VGUID AS FixedAssetsOrderVguid,fao.OrderQuantity,fao.OrderNumber, pon.PayItemCode,CASE	WHEN pon.PayItemCode IS NULL THEN 1 ELSE 0 END AS IsChecked FROM (SELECT * FROM dbo.Business_FixedAssetsOrder where vguid = '"+ FixedAssetsOrderVguid  +"' ) fao LEFT JOIN (SELECT * FROM Business_PurchaseOrderNum WHERE PayItemCode = '" +
                        PayItemCode + "') AS  pon ON fao.VGUID = pon.FixedAssetOrderVguid";
                    list = db.SqlQueryable<PurchaseOrderSelectNum>(sql).Where(x => x.PayItemCode != null).ToList();
                }
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}