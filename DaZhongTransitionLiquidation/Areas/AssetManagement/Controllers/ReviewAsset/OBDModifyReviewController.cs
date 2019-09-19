using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Controllers;
using DaZhongTransitionLiquidation.Infrastructure.ApiResultEntity;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.ReviewAsset
{
    public class OBDModifyReviewController : BaseController
    {
        // GET: AssetManagement/OBDModifyReview
        public OBDModifyReviewController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/ReviewAsset
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("eaf2caac-98f9-459c-939b-faf6468c5c64");
            return View();
        }
        public JsonResult GetReviewOBDListDatas(GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_ModifyOBD>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_ModifyOBD>().OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ImportModifyOBDReview(HttpPostedFileBase File)
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
                    DbBusinessDataService.Command(db =>
                    {
                        var consistent = true;
                        var result = db.Ado.UseTran(() =>
                        {
                            var list = new List<OBDModifyModel>();
                            var dt = ExcelHelper.ExportToDataTable(filePath, true);
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                var assign = new OBDModifyModel();
                                assign.EquipmentNumber = dt.Rows[i][0].ToString();
                                assign.PlateNumber = dt.Rows[i][1].ToString();
                                assign.LisensingDate = dt.Rows[i][2].TryToDate();
                                list.Add(assign);
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
                            //判断数据，系统中没有才可以导入
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
                            if (consistent)
                            {
                                //写入OBD资产变更审核表
                                var obdReviewList = new List<Business_ModifyOBD>();
                                foreach (var item in list)
                                {
                                    var obdReview = new Business_ModifyOBD();
                                    obdReview.VGUID = Guid.NewGuid();
                                    obdReview.PlateNumber = item.PlateNumber;
                                    obdReview.EquipmentNumber = item.EquipmentNumber;
                                    obdReview.ISVerify = false;
                                    obdReview.CreateDate = DateTime.Now;
                                    obdReview.CreateUser = "";
                                    obdReviewList.Add(obdReview);
                                }
                                db.Insertable<Business_ModifyOBD>(obdReviewList).ExecuteCommand();
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
        public JsonResult SubmitModifyOBDReview(List<Guid> guids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var result = db.Ado.UseTran(() =>
                {
                    var modifyVehicleList = db.Queryable<Business_ModifyOBD>().Where(x => guids.Contains(x.VGUID)).ToList();
                    foreach (var item in modifyVehicleList)
                    {
                        if (db.Queryable<Business_AssetMaintenanceInfo>()
                            .Any(x => x.PLATE_NUMBER == item.PlateNumber))
                        {
                            var asset = db.Queryable<Business_AssetMaintenanceInfo>()
                                .Where(x => x.PLATE_NUMBER == item.PlateNumber).First();
                            asset.CHASSIS_NUMBER = item.EquipmentNumber;
                            db.Updateable<Business_AssetMaintenanceInfo>().UpdateColumns(x => new {x.CHASSIS_NUMBER})
                                .ExecuteCommand();
                        }
                    }
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = result.ErrorMessage;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}