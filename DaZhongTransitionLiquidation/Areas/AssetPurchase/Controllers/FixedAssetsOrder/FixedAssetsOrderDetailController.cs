using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SyntacticSugar;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Common;

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
                        sevenSection.CreateDate = DateTime.Now;
                        sevenSection.CreateUser = cache[PubGet.GetUserKey].UserName;
                        sevenSection.SubmitStatus = FixedAssetsSubmitStatusEnum.UnSubmit.TryToInt();
                        db.Insertable<Business_FixedAssetsOrder>(sevenSection).ExecuteCommand();
                    }
                    else
                    {
                        sevenSection.ChangeDate = DateTime.Now;
                        sevenSection.ChangeUser = cache[PubGet.GetUserKey].UserName;
                        db.Updateable<Business_FixedAssetsOrder>(sevenSection).IgnoreColumns(x => new { x.CreateDate, x.CreateUser, x.SubmitStatus }).ExecuteCommand();
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
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
            });
            if (listFixedAssetsOrder.Count == 0)
            {
                listFixedAssetsOrder = GetDefaultAssetOrderDetails(AssetType, AssetsOrderVguid);
            }
            return Json(listFixedAssetsOrder, JsonRequestBehavior.AllowGet); ;
        }

        public List<Business_AssetOrderDetails> GetDefaultAssetOrderDetails(string AssetType, Guid AssetsOrderVguid)
        {
            var cache = CacheManager<Sys_User>.GetInstance();
            var list = new List<Business_AssetOrderDetails>();
            var listCompany = new List<string>();
            if (AssetType == "vehicle")
            {
                listCompany.Add("浦东");
                listCompany.Add("市南");
                listCompany.Add("九分");
            }
            else
            {
                listCompany.Add("集团");
                listCompany.Add("虹口");
                listCompany.Add("奉贤");
            }
            listCompany.Add("新亚");
            listCompany.Add("交运");
            listCompany.Add("万祥");
            listCompany.Add("营管部");
            for (int i = 0; i < listCompany.Count; i++)
            {
                var model = new Business_AssetOrderDetails();
                model.VGUID = Guid.NewGuid();
                model.AssetsOrderVguid = AssetsOrderVguid;
                model.CreateDate = DateTime.Now;
                model.CreateUser = cache[PubGet.GetUserKey].UserName;
                model.AssetManagementCompany = listCompany[i];
                list.Add(model);
            }
            DbBusinessDataService.Command(db =>
            {
                db.Insertable<Business_AssetOrderDetails>(list).ExecuteCommand();
            });
            return list;
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
    }
}