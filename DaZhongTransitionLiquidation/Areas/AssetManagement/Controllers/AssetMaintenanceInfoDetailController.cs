using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using SqlSugar;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Common;
using AutoMapper;
using System.Net;
using System.Web.Script.Serialization;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetsMaintenance
{
    public class AssetMaintenanceInfoDetailController : BaseController
    {
        // GET: AssetManagement/AssetMaintenanceInfoDetail
        public AssetMaintenanceInfoDetailController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/AssetBasicInfoMaintenanceDetail
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult SaveAssetMaintenanceInfo(Business_AssetMaintenanceInfo sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (sevenSection.VGUID == Guid.Empty)
                    {
                        if(!db.Queryable<Business_AssetMaintenanceInfo>().Any(c => c.PLATE_NUMBER == sevenSection.PLATE_NUMBER))
                        {
                            sevenSection.VGUID = Guid.NewGuid();
                            sevenSection.CREATE_DATE = DateTime.Now;
                            sevenSection.CREATE_USER = cache[PubGet.GetUserKey].UserName;
                            //获取资产类别基础信息，填充Business_AssetMaintenanceInfo
                            var assetMintor = GetASSET_CATEGORY_MINOR(sevenSection.ORGANIZATION_NUM, sevenSection.GROUP_ID, sevenSection.ENGINE_NUMBER, sevenSection.CHASSIS_NUMBER);
                            if (!assetMintor.IsNullOrEmpty())
                            {
                                var assetCategory = GetBusiness_AssetsCategory(assetMintor);
                                sevenSection.LIFE_MONTHS = assetCategory.LIFE_YEARS * 12 + assetCategory.LIFE_MONTHS;
                                sevenSection.BOOK_TYPE_CODE = assetCategory.BOOK_TYPE_CODE;
                                sevenSection.METHOD = assetCategory.METHOD;
                                sevenSection.ASSET_CATEGORY_MAJOR = assetCategory.ASSET_CATEGORY_MAJOR;
                                sevenSection.ASSET_CATEGORY_MINOR = assetCategory.ASSET_CATEGORY_MINOR;
                                var account = assetCategory.ASSET_COST_ACCOUNT;
                                var arr = account.Split(".");
                                sevenSection.EXP_ACCOUNT_SEGMENT1 = Convert.ToDouble(arr[0]);
                                sevenSection.EXP_ACCOUNT_SEGMENT2 = Convert.ToDouble(arr[1]);
                                sevenSection.EXP_ACCOUNT_SEGMENT3 = Convert.ToDouble(arr[2]);
                                sevenSection.EXP_ACCOUNT_SEGMENT4 = Convert.ToDouble(arr[3]);
                                sevenSection.EXP_ACCOUNT_SEGMENT5 = Convert.ToDouble(arr[4]);
                                sevenSection.EXP_ACCOUNT_SEGMENT6 = Convert.ToDouble(arr[5]);
                                sevenSection.EXP_ACCOUNT_SEGMENT7 = Convert.ToDouble(arr[6]);
                            }
                            sevenSection.STATUS = AcceptStatus.UnAccept;
                            db.Insertable<Business_AssetMaintenanceInfo>(sevenSection).ExecuteCommand();                       
                            //中间表数据
                            var resultSwap = SaveAssetMaintenanceInfo_Swap(sevenSection);
                        }
                        else
                        {
                            resultModel.IsSuccess = false;
                            resultModel.ResultInfo = "车牌号已存在";
                            resultModel.Status = "2";
                        }
                    }
                    else
                    {
                        sevenSection.CHANGE_DATE = DateTime.Now;
                        sevenSection.CHANGE_USER = cache[PubGet.GetUserKey].UserName;
                        var assetMintor = GetASSET_CATEGORY_MINOR(sevenSection.ORGANIZATION_NUM, sevenSection.GROUP_ID, sevenSection.ENGINE_NUMBER, sevenSection.CHASSIS_NUMBER);
                        if (!assetMintor.IsNullOrEmpty() && assetMintor != sevenSection.ASSET_CATEGORY_MAJOR)
                        {
                            var assetCategory = GetBusiness_AssetsCategory(assetMintor);
                            sevenSection.LIFE_MONTHS = assetCategory.LIFE_YEARS * 12 + assetCategory.LIFE_MONTHS;
                            sevenSection.BOOK_TYPE_CODE = assetCategory.BOOK_TYPE_CODE;
                            sevenSection.METHOD = assetCategory.METHOD;
                            sevenSection.ASSET_CATEGORY_MAJOR = assetCategory.ASSET_CATEGORY_MAJOR;
                            sevenSection.ASSET_CATEGORY_MINOR = assetCategory.ASSET_CATEGORY_MINOR;
                            var account = assetCategory.ASSET_COST_ACCOUNT;
                            var arr = account.Split(".");
                            sevenSection.EXP_ACCOUNT_SEGMENT1 = Convert.ToDouble(arr[0]);
                            sevenSection.EXP_ACCOUNT_SEGMENT2 = Convert.ToDouble(arr[1]);
                            sevenSection.EXP_ACCOUNT_SEGMENT3 = Convert.ToDouble(arr[2]);
                            sevenSection.EXP_ACCOUNT_SEGMENT4 = Convert.ToDouble(arr[3]);
                            sevenSection.EXP_ACCOUNT_SEGMENT5 = Convert.ToDouble(arr[4]);
                            sevenSection.EXP_ACCOUNT_SEGMENT6 = Convert.ToDouble(arr[5]);
                            sevenSection.EXP_ACCOUNT_SEGMENT7 = Convert.ToDouble(arr[6]);
                        }
                        db.Updateable<Business_AssetMaintenanceInfo>(sevenSection).IgnoreColumns(x => new { x.CREATE_DATE, x.CREATE_USER, x.ACCEPTANCE_CERTIFICATE, x.STATUS }).ExecuteCommand();
                        //中间表数据
                        var resultSwap = SaveAssetMaintenanceInfo_Swap(sevenSection);
                    }
                });
                if (resultModel.Status != "2")
                {
                    resultModel.IsSuccess = result.IsSuccess;
                    resultModel.ResultInfo = result.ErrorMessage;
                    resultModel.Status = resultModel.IsSuccess ? "1" : "0";
                }
            });
            return Json(resultModel);
        }
        public bool SaveAssetMaintenanceInfo_Swap(Business_AssetMaintenanceInfo sevenSection)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            //var sevenSwapSection = new Business_AssetMaintenanceInfo_Swap();

            var sevenSwapSection = Mapper.Map<AssetMaintenanceInfo_Swap>(sevenSection);
            DbBusinessDataService.Command(db =>
            {
                
                if (!db.Queryable<AssetMaintenanceInfo_Swap>().Any(c => c.ASSET_ID == sevenSection.VGUID))
                {
                    db.Insertable<AssetMaintenanceInfo_Swap>(sevenSwapSection).ExecuteCommand();
                }
                else
                {
                    sevenSection.CHANGE_DATE = DateTime.Now;
                    db.Updateable<AssetMaintenanceInfo_Swap>(sevenSwapSection).ExecuteCommand();
                }
            });
            return true;
        }
        public JsonResult GetAssetInfoDetail(Guid vguid)
        {
            Business_AssetMaintenanceInfo model = new Business_AssetMaintenanceInfo();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                model = db.Queryable<Business_AssetMaintenanceInfo>().Single(x => x.VGUID == vguid);
            });
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取资产基础信息
        /// </summary>
        public Business_AssetsCategory GetBusiness_AssetsCategory(string ASSET_CATEGORY_MINOR)
        {
            var model = new Business_AssetsCategory();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                model = db.Queryable<Business_AssetsCategory>().Single(x => x.ASSET_CATEGORY_MINOR == ASSET_CATEGORY_MINOR);
            });
            return model;
        }
        /// <summary>
        /// 获取资产次类
        /// </summary>
        /// <param name="GROUP_ID"></param>
        /// <param name="ENGINE_NUMBER"></param>
        /// <param name="ORGANIZATION_NUM"></param>
        /// <param name="CHASSIS_NUMBER"></param>
        /// <returns></returns>
        public string GetASSET_CATEGORY_MINOR(string ORGANIZATION_NUM, string GROUP_ID, string ENGINE_NUMBER, string CHASSIS_NUMBER)
        {
            return AssetMaintenanceAPI.GetASSET_CATEGORY_MINOR(ORGANIZATION_NUM, GROUP_ID, ENGINE_NUMBER, CHASSIS_NUMBER);
        }

        public JsonResult UploadImg(Guid Vguid, string ImageBase64Str)
        {
            var sevenSection = new Business_AssetMaintenanceInfo();
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            int len = ImageBase64Str.IndexOf("base64,") + 7;
            int len1 = ImageBase64Str.IndexOf("data:") + 5;
            string ext = ImageBase64Str.Substring(len1, len - len1 - 8);
            var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "AcceptFile\\" +
                DateTime.Now.ToString("yyyyMMddHHmmssfff.") +
                (ext.ToLower().Contains("png") ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg);
            var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath;
            var savePath = FileHelper.Base64ToImg(filePath, ImageBase64Str);
            if (!savePath.IsNullOrEmpty())
            {
                DbBusinessDataService.Command(db =>
                {
                    var result = db.Ado.UseTran(() =>
                    {
                        sevenSection = db.Queryable<Business_AssetMaintenanceInfo>().Where(c => c.VGUID == Vguid).First();
                        sevenSection.ACCEPTANCE_CERTIFICATE = uploadPath;
                        sevenSection.CHANGE_DATE = DateTime.Now;
                        sevenSection.CHANGE_USER = cache[PubGet.GetUserKey].UserName;
                        db.Updateable(sevenSection).UpdateColumns(x => new {
                            x.CHANGE_DATE,
                            x.CHANGE_USER,
                            x.ACCEPTANCE_CERTIFICATE
                        }).ExecuteCommand();
                    });
                    resultModel.IsSuccess = result.IsSuccess;
                    resultModel.ResultInfo = uploadPath;
                    resultModel.ResultInfo2 = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                    resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
                });
            }
            return Json(resultModel);
        }
        public JsonResult UploadToImageServer(Guid Vguid, string ImageBase64Str)
        {
            var sevenSection = new Business_AssetMaintenanceInfo();
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            var imageServerUrl = ConfigSugar.GetAppString("ImageServerUrl");
            var resultData = FileUploadHelper.UploadToImageServer(ImageBase64Str);
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
                                sevenSection = db.Queryable<Business_AssetMaintenanceInfo>().Where(c => c.VGUID == Vguid).First();
                                sevenSection.ACCEPTANCE_CERTIFICATE = imageServerUrl + fileData.fileName;
                                sevenSection.CHANGE_DATE = DateTime.Now;
                                sevenSection.CHANGE_USER = cache[PubGet.GetUserKey].UserName;
                                db.Updateable(sevenSection).UpdateColumns(x => new
                                {
                                    x.CHANGE_DATE,
                                    x.CHANGE_USER,
                                    x.ACCEPTANCE_CERTIFICATE
                                }).ExecuteCommand();
                            });
                            resultModel.IsSuccess = result.IsSuccess;
                            resultModel.ResultInfo = sevenSection.ACCEPTANCE_CERTIFICATE;
                            resultModel.ResultInfo2 = fileData.fileName;
                            resultModel.Status = Convert.ToBoolean(resultModel.IsSuccess) ? "1" : "0";
                        });
                    }
                }
            }
            return Json(resultModel);
        }
        public JsonResult UploadLocalFile(Guid Vguid, HttpPostedFileBase File)
        {
            var sevenSection = new Business_AssetMaintenanceInfo();
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            if(File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "AcceptFile\\" + newFileName;
                var filePath = System.AppDomain.CurrentDomain.BaseDirectory + uploadPath;
                try
                {
                    File.SaveAs(filePath);
                    DbBusinessDataService.Command(db =>
                    {
                        var result = db.Ado.UseTran(() =>
                        {
                            sevenSection = db.Queryable<Business_AssetMaintenanceInfo>().Where(c => c.VGUID == Vguid).First();
                            sevenSection.ACCEPTANCE_CERTIFICATE = uploadPath;
                            sevenSection.CHANGE_DATE = DateTime.Now;
                            sevenSection.CHANGE_USER = cache[PubGet.GetUserKey].UserName;
                            db.Updateable(sevenSection).UpdateColumns(x => new {
                                x.CHANGE_DATE,
                                x.CHANGE_USER,
                                x.ACCEPTANCE_CERTIFICATE
                            }).ExecuteCommand();
                        });
                        resultModel.IsSuccess = result.IsSuccess;
                        resultModel.ResultInfo = uploadPath;
                        resultModel.ResultInfo2 = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
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
        public JsonResult InserIntoSwapTable(Guid Vguid)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var isSuccess = false;
            DbBusinessDataService.Command(db =>
            {
                //var assetMaintenanceSwapInfo = new Business_AssetMaintenanceInfo_Swap();
                var result = db.Ado.UseTran(() =>
                {
                    //写入中间表
                    var assetMaintenanceInfo = db.Queryable<Business_AssetMaintenanceInfo>().Where(c => c.VGUID == Vguid).First();
                    var assetMaintenanceSwapInfo = Mapper.Map<AssetMaintenanceInfo_Swap>(assetMaintenanceInfo);
                    
                    assetMaintenanceSwapInfo.STATUS = AcceptStatus.Accepted;
                    db.Updateable<AssetMaintenanceInfo_Swap>(assetMaintenanceSwapInfo).IgnoreColumns(x => new { x.CREATE_DATE,x.ASSET_ID,x.TRANSACTION_ID }).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
        /// <summary>
        /// 调用接口
        /// </summary>
        /// <param name="Vguid"></param>
        /// <returns></returns>
        public JsonResult SendAssetInfo(Guid Vguid)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var sevenSection = new Business_AssetMaintenanceInfo();
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            var isSuccess = false;
            DbBusinessDataService.Command(db =>
            {
                var assetMaintenanceInfo = db.Queryable<Business_AssetMaintenanceInfo>().Where(c => c.VGUID == Vguid).First();
                var assetData = new AssetInfoData();
                assetData.VGUID = sevenSection.VGUID;
                assetData.ORGANATION_NUM = sevenSection.ORGANIZATION_NUM;
                assetData.ENGINE_NUMBER = sevenSection.ENGINE_NUMBER;
                assetData.CHASSIS_NUMBER = sevenSection.CHASSIS_NUMBER;
                assetData.BOOK_TYPE_CODE = sevenSection.BOOK_TYPE_CODE;
                assetData.TAG_NUMBER = sevenSection.TAG_NUMBER;
                assetData.DESCRIPTION = sevenSection.DESCRIPTION;
                assetData.QUANITIY = sevenSection.QUANTITY;
                assetMaintenanceInfo.STATUS = AcceptStatus.Accepted;
                assetMaintenanceInfo.CHANGE_DATE = DateTime.Now;
                assetMaintenanceInfo.CHANGE_USER = cache[PubGet.GetUserKey].UserName;
                db.Updateable<Business_AssetMaintenanceInfo>(assetMaintenanceInfo).UpdateColumns(x => new { x.STATUS, x.CHANGE_DATE}).ExecuteCommand();
                resultModel.IsSuccess = AssetMaintenanceAPI.SendAssetInfo(assetData);
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel);
        }
    }
}