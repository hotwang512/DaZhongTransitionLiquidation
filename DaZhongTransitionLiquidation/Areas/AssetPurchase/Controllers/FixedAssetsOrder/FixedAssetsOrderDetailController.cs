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
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CustomerBankInfo;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderList;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail;
using DaZhongTransitionLiquidation.Common;
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
                        var orderNumberLeft = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                        //查出当前日期数据库中最大的订单号

                        var currentDayFixedAssetOrderList = db.Queryable<Business_FixedAssetsOrder>()
                            .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new { c.OrderNumber }).ToList();
                        var currentDayIntangibleAssetsOrderList = db.Queryable<Business_IntangibleAssetsOrder>()
                            .Where(c => c.OrderNumber.StartsWith(orderNumberLeft)).Select(c => new {c.OrderNumber}).ToList();
                        var currentDayList = currentDayFixedAssetOrderList.Union(currentDayIntangibleAssetsOrderList).ToList();
                        var maxOrderNumRight = 0;
                        if (currentDayList.Any())
                        {
                            maxOrderNumRight = currentDayList.OrderBy(c => c.OrderNumber.Replace(orderNumberLeft, "").TryToInt()).First().OrderNumber.Replace(orderNumberLeft, "").TryToInt();
                        }
                        maxOrderNumRight = maxOrderNumRight + 1;
                        sevenSection.OrderNumber = orderNumberLeft + maxOrderNumRight.ToString().PadLeft(4,'0');
                        sevenSection.CreateDate = DateTime.Now;
                        sevenSection.CreateUser = cache[PubGet.GetUserKey].UserName;
                        sevenSection.SubmitStatus = FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt();
                        db.Insertable<Business_FixedAssetsOrder>(sevenSection).ExecuteCommand();

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
                        var goodsData = db.Queryable<Business_PurchaseOrderSetting>()
                            .Where(x => x.VGUID == sevenSection.PurchaseGoodsVguid).First();
                        var orderListData = db.Queryable<Business_OrderList>()
                            .Where(x => x.BusinessSubItem1 == goodsData.BusinessSubItem).First();

                        pendingPaymentmodel.ServiceCategory = orderListData.BusinessProject;
                        pendingPaymentmodel.BusinessProject = orderListData.BusinessSubItem1;
                        //根据供应商账号找到供应商类别
                        pendingPaymentmodel.PaymentCompany = db.Queryable<Business_CustomerBankInfo>()
                            .Where(x => x.BankAccount == sevenSection.SupplierBankAccount).First().CompanyOrPerson; ;
                        pendingPaymentmodel.CollectBankAccountName = sevenSection.SupplierBankAccountName;
                        pendingPaymentmodel.CollectBankAccouont = sevenSection.SupplierBankAccount;
                        pendingPaymentmodel.CollectBankName = sevenSection.SupplierBank;
                        pendingPaymentmodel.CollectBankNo = sevenSection.SupplierBankNo;

                        pendingPaymentmodel.IdentityToken = cache[PubGet.GetUserKey].Token;
                        pendingPaymentmodel.FunctionSiteId = "61";
                        pendingPaymentmodel.OperatorIP = GetSystemInfo.GetClientLocalIPv4Address();
                        pendingPaymentmodel.invoiceNumber = assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count().ToString();
                        pendingPaymentmodel.numberOfAttachments = (assetAttachmentList.Count() - assetAttachmentList.Where(x => x.AttachmentType == "发票").ToList().Count()).ToString();
                        pendingPaymentmodel.Amount = sevenSection.ContractAmount.ToString();
                        pendingPaymentmodel.Summary = sevenSection.AssetDescription;

                        var apiReault = PendingPaymentApi(pendingPaymentmodel);
                        var pendingRedult = apiReault.JsonToModel<JsonResultModelApi<Api_PendingPayment>>();
                        if (pendingRedult.success)
                        {
                            var orderModel = db.Queryable<Business_FixedAssetsOrder>()
                                .Where(x => x.VGUID == sevenSection.VGUID).First();
                            orderModel.PaymentVoucherVguid = pendingRedult.data.vguid;
                            orderModel.PaymentVoucherUrl = pendingRedult.data.url;
                            db.Updateable<Business_FixedAssetsOrder>(orderModel).UpdateColumns(x => new { x.PaymentVoucherUrl,x.PaymentVoucherVguid }).ExecuteCommand();
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
                        db.Updateable<Business_FixedAssetsOrder>(sevenSection).IgnoreColumns(x => new { x.CreateDate, x.CreateUser, x.SubmitStatus,x.OrderNumber }).ExecuteCommand();
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
                                sevenSection.ChangeUser = cache[PubGet.GetUserKey].UserName;
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
        public JsonResult GetPurchaseGoods(int OrderCategory,Guid[] PurchaseDepartment)
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
                    orderTypeData = db.SqlQueryable<Business_PurchaseOrderSetting>(@"SELECT DISTINCT bpos.* FROM  Business_PurchaseOrderSetting bpos INNER JOIN
                    Business_PurchaseDepartment bpd ON bpos.VGUID = bpd.PurchaseOrderSettingVguid
                    WHERE OrderCategory = '0' And bpd.DepartmentVguid IN ('" + PurchaseDepartmentStr + "')").ToList();
                }
            });
            return Json(orderTypeData, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult SubmitFixedAssetsOrder(Guid vguid)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var model = db.Queryable<Business_FixedAssetsOrder>().Where(c => c.VGUID == vguid).First();
                    model.SubmitStatus = FixedAssetsSubmitStatusEnum.Submited.TryToInt();
                    model.SubmitDate = DateTime.Now;
                    model.SubmitUser = cache[PubGet.GetUserKey].UserName;
                    db.Updateable<Business_FixedAssetsOrder>(model).UpdateColumns(x => new { x.SubmitStatus, x.SubmitDate, x.SubmitUser }).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess.ObjToBool() ? "1" : "0";
            });
            return Json(resultModel);
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
                var jsonResult = new JsonResultModel<v_BankInfoSetting>();
                DbBusinessDataService.Command(db =>
                {
                    jsonResult.Rows = db.Queryable<v_BankInfoSetting>().Where(i => i.IsCheck == "Checked")
                        .WhereIF(PurchaseOrderSettingGuid != Guid.Empty,
                            i => i.PurchaseOrderSettingVguid == PurchaseOrderSettingGuid)
                        .OrderBy(i => i.CreateTime, OrderByType.Desc).ToList();
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
        public JsonResult GetCompanyBankInfo(Guid Vguid)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
            var model = new Business_CompanyBankInfo();
            DbBusinessDataService.Command(db =>
            {
                var sevenSection = db.Queryable<Business_SevenSection>().Where(c => c.VGUID == Vguid).First();
                model = db.Queryable<Business_CompanyBankInfo>().Where(c => c.CompanyCode == sevenSection.Code && c.AccountModeCode == AccountModeCode && c.BankStatus).First();
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
                if (db.Queryable<Business_UserCompanySetDetail>()
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
        public string PendingPaymentApi(PendingPaymentModel model)
        {
            var url = ConfigSugar.GetAppString("PendingPaymentUrl");
            var data = "{" +
                       "\"IdentityToken\":\"{IdentityToken}\",".Replace("{IdentityToken}", model.IdentityToken) +
                       "\"FunctionSiteId\":\"{FunctionSiteId}\",".Replace("{FunctionSiteId}", "61") +
                       "\"OperatorIP\":\"{OperatorIP}\",".Replace("{OperatorIP}", GetSystemInfo.GetClientLocalIPv4Address()) +
                       "\"ServiceCategory\":\"{ServiceCategory}\",".Replace("{ServiceCategory}", model.ServiceCategory) +
                       "\"BusinessProject\":\"{BusinessProject}\",".Replace("{BusinessProject}", model.BusinessProject) +
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
    }
}