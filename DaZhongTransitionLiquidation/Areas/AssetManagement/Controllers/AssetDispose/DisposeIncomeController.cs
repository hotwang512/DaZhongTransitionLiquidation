using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Controllers.AssetDisposeIncome
{
    public class DisposeIncomeController : BaseController
    {
        // GET: AssetManagement/DisposeIncome
        public DisposeIncomeController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: AssetManagement/DisposeIncome
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("f0ed636a-5001-4393-9bbc-5c9dd195f64b");
            return View();
        }
        public JsonResult GetAssetsDisposeIncomeListDatas(string PlateNumber, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_DisposeIncome>();

            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_DisposeIncome>()
                    .WhereIF(PlateNumber != null, i => i.DepartmentVehiclePlateNumber.Contains(PlateNumber) || i.OraclePlateNumber.Contains(PlateNumber) || i.ImportPlateNumber.Contains(PlateNumber))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="File"></param>
        /// <param name="ImportType">Auction 拍卖, Sale 出售,Scrap 报废</param>
        /// <returns></returns>
        public JsonResult ImportDisposeIncomeFile(HttpPostedFileBase File, string ImportType)
        {
            var resultModel = new ResultModel<string, string>() { IsSuccess = false, Status = "0" };
            var cache = CacheManager<Sys_User>.GetInstance();
            var AccountModeCode = cache[PubGet.GetUserKey].AccountModeCode;
            if (File != null)
            {
                var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + File.FileName.Substring(File.FileName.LastIndexOf("."), File.FileName.Length - File.FileName.LastIndexOf("."));
                var uploadPath = "\\" + ConfigSugar.GetAppString("UploadPath") + "\\" + "DisposeIncome\\";
                var filePath = AppDomain.CurrentDomain.BaseDirectory + uploadPath + newFileName;
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + uploadPath))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + uploadPath);
                }
                try
                {
                    File.SaveAs(filePath);
                    DbBusinessDataService.Command(db =>
                    {
                        var result = db.Ado.UseTran(() =>
                        {
                            var disposeIncomeList = db.Queryable<Business_DisposeIncome>().ToList();
                            var updateDisposeIncomeList = new List<Business_DisposeIncome>();
                            if (ImportType == "Auction")
                            {
                                var disposeAuctionImportList = new List<Excel_DisposeIncomeAuction>();
                                var dt = ExcelHelper.ExportToDataTable(filePath, true);
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    var dispose = new Excel_DisposeIncomeAuction();
                                    dispose.ImportPlateNumber = dt.Rows[i][1].ToString();
                                    dispose.VehicleModel = dt.Rows[i][2].ToString();
                                    dispose.CommissioningDate = dt.Rows[i][3].ToString();
                                    dispose.TransactionPrice = dt.Rows[i][4].ToString();
                                    dispose.ProcedureFee = dt.Rows[i][5].ToString();
                                    dispose.SettlementPrice = dt.Rows[i][6].ToString();
                                    dispose.UseDepartment = dt.Rows[i][7].ToString();
                                    dispose.VehicleOwner = dt.Rows[i][8].ToString();
                                    dispose.BackCarDate = dt.Rows[i][9].ToString();
                                    dispose.Remark = dt.Rows[i][10].ToString();
                                    dispose.BusinessModel = dt.Rows[i][11].ToString();
                                    disposeAuctionImportList.Add(dispose);
                                }
                                foreach (var item in disposeAuctionImportList)
                                {
                                    if (disposeIncomeList.Any(x =>
                                            x.DepartmentVehiclePlateNumber.Contains(item.ImportPlateNumber)) ||
                                        disposeIncomeList.Any(x => x.OraclePlateNumber.Contains(item.ImportPlateNumber)))
                                    {
                                        var updateModel = disposeIncomeList.First(x => x.DepartmentVehiclePlateNumber.Contains(item.ImportPlateNumber) ||
                                                                                       x.OraclePlateNumber.Contains(item.ImportPlateNumber));
                                        updateModel.ImportPlateNumber = item.ImportPlateNumber;
                                        //updateModel.VehicleModel = item.VehicleModel;
                                        updateModel.CommissioningDate = item.CommissioningDate.TryToDate();
                                        updateModel.TransactionPrice = item.TransactionPrice.ObjToDecimal();
                                        updateModel.ProcedureFee = item.ProcedureFee.ObjToDecimal();
                                        updateModel.SettlementPrice = item.SettlementPrice.ObjToDecimal();
                                        updateModel.UseDepartment = item.UseDepartment;
                                        updateModel.VehicleOwner = item.VehicleOwner;
                                        updateModel.BackCarDate = item.BackCarDate.TryToDate();
                                        updateModel.Remark = item.VehicleModel;
                                        updateModel.BusinessModel = item.BusinessModel;
                                        updateModel.ChangeDate = DateTime.Now;
                                        updateModel.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                                        updateDisposeIncomeList.Add(updateModel);
                                    }
                                }
                                if (updateDisposeIncomeList.Count > 0)
                                {
                                    db.Updateable<Business_DisposeIncome>(updateDisposeIncomeList).ExecuteCommand();
                                    resultModel.IsSuccess = true;
                                    resultModel.Status = "1";
                                }
                                else
                                {
                                    resultModel.Status = "2";
                                    resultModel.ResultInfo = "没有匹配到车牌号";
                                }
                            }else if (ImportType == "Sale")
                            {
                                var disposeSaleImportList = new List<Excel_DisposeIncomeSale>();
                                var dt = ExcelHelper.ExportToDataTable(filePath, true);
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    var dispose = new Excel_DisposeIncomeSale();
                                    dispose.ImportPlateNumber = dt.Rows[i][1].ToString();
                                    dispose.VehicleModel = dt.Rows[i][2].ToString();
                                    dispose.TransactionPrice = dt.Rows[i][3].ToString();
                                    dispose.ProcedureFee = dt.Rows[i][4].ToString();
                                    dispose.SettlementPrice = dt.Rows[i][5].ToString();
                                    dispose.CommissioningDate = dt.Rows[i][6].ToString();
                                    dispose.UseDepartment = dt.Rows[i][7].ToString();
                                    dispose.VehicleOwner = dt.Rows[i][8].ToString();
                                    dispose.BackCarDate = dt.Rows[i][9].ToString();
                                    dispose.Remark = dt.Rows[i][10].ToString();
                                    dispose.BusinessModel = dt.Rows[i][11].ToString();
                                    disposeSaleImportList.Add(dispose);
                                }
                                foreach (var item in disposeSaleImportList)
                                {
                                    if (disposeIncomeList.Any(x =>
                                            x.DepartmentVehiclePlateNumber.Contains(item.ImportPlateNumber)) ||
                                        disposeIncomeList.Any(x => x.OraclePlateNumber.Contains(item.ImportPlateNumber)))
                                    {
                                        var updateModel = disposeIncomeList.First(x => x.DepartmentVehiclePlateNumber.Contains(item.ImportPlateNumber) ||
                                                                                       x.OraclePlateNumber.Contains(item.ImportPlateNumber));
                                        updateModel.ImportPlateNumber = item.ImportPlateNumber;
                                        //updateModel.VehicleModel = item.VehicleModel;
                                        updateModel.CommissioningDate = item.CommissioningDate.TryToDate();
                                        updateModel.TransactionPrice = item.TransactionPrice.ObjToDecimal();
                                        updateModel.ProcedureFee = item.ProcedureFee.ObjToDecimal();
                                        updateModel.SettlementPrice = updateModel.TransactionPrice - updateModel.ProcedureFee;
                                        updateModel.UseDepartment = item.UseDepartment;
                                        updateModel.VehicleOwner = item.VehicleOwner;
                                        updateModel.BackCarDate = item.BackCarDate.TryToDate();
                                        updateModel.Remark = item.VehicleModel;
                                        updateModel.BusinessModel = item.BusinessModel;
                                        updateModel.ChangeDate = DateTime.Now;
                                        updateModel.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                                        updateDisposeIncomeList.Add(updateModel);
                                    }
                                }
                                if (updateDisposeIncomeList.Count > 0)
                                {
                                    db.Updateable<Business_DisposeIncome>(updateDisposeIncomeList).ExecuteCommand();
                                    resultModel.IsSuccess = true;
                                    resultModel.Status = "1";
                                }
                                else
                                {
                                    resultModel.Status = "2";
                                    resultModel.ResultInfo = "没有匹配到车牌号";
                                }
                            }
                            else if (ImportType == "Scrap")
                            {
                                var disposeScrapImportList = new List<Excel_DisposeIncomeScrap>();
                                var dt = ExcelHelper.ExportToDataTable(filePath, true);
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    var dispose = new Excel_DisposeIncomeScrap();
                                    dispose.ImportPlateNumber = dt.Rows[i][1].ToString();
                                    dispose.VehicleOwner = dt.Rows[i][2].ToString();
                                    dispose.VehicleModel = dt.Rows[i][3].ToString();
                                    dispose.VehicleType = dt.Rows[i][4].ToString();
                                    dispose.CurbWeight = dt.Rows[i][5].ToString();
                                    dispose.DeductTonnage = dt.Rows[i][6].ToString();
                                    dispose.ActualTonnage = dt.Rows[i][7].ToString();
                                    dispose.SalvageUnitPrice = dt.Rows[i][8].ToString();
                                    dispose.SalvageValue = dt.Rows[i][9].ToString();
                                    dispose.ServiceUnitFee = dt.Rows[i][10].ToString();
                                    dispose.ServiceFee = dt.Rows[i][11].ToString();
                                    dispose.TowageFee = dt.Rows[i][12].ToString();
                                    dispose.SettlementPrice = dt.Rows[i][13].ToString();
                                    dispose.UseDepartment = dt.Rows[i][14].ToString();
                                    dispose.BusinessModel = dt.Rows[i][15].ToString();
                                    disposeScrapImportList.Add(dispose);
                                }
                                foreach (var item in disposeScrapImportList)
                                {
                                    if (disposeIncomeList.Any(x =>
                                            x.DepartmentVehiclePlateNumber.Contains(item.ImportPlateNumber)) ||
                                        disposeIncomeList.Any(x => x.OraclePlateNumber.Contains(item.ImportPlateNumber)))
                                    {
                                        var updateModel = disposeIncomeList.First(x => x.DepartmentVehiclePlateNumber.Contains(item.ImportPlateNumber) ||
                                                                                       x.OraclePlateNumber.Contains(item.ImportPlateNumber));
                                        updateModel.ImportPlateNumber = item.ImportPlateNumber;
                                        updateModel.VehicleOwner = item.VehicleOwner;
                                        updateModel.VehicleModel = item.VehicleModel;
                                        updateModel.VehicleType = item.VehicleType;
                                        updateModel.CurbWeight = item.CurbWeight.ObjToDecimal();
                                        updateModel.DeductTonnage = item.DeductTonnage.ObjToDecimal();
                                        updateModel.ActualTonnage = item.ActualTonnage.ObjToDecimal();
                                        updateModel.SalvageUnitPrice = item.SalvageUnitPrice.ObjToDecimal();
                                        updateModel.SalvageValue = item.SalvageValue.ObjToDecimal();
                                        updateModel.ServiceUnitFee = item.ServiceUnitFee.ObjToDecimal();
                                        updateModel.ServiceFee = item.ServiceFee.ObjToDecimal();
                                        updateModel.TowageFee = item.TowageFee.ObjToDecimal();
                                        updateModel.SettlementPrice = updateModel.TransactionPrice - updateModel.ProcedureFee;
                                        updateModel.UseDepartment = item.UseDepartment;
                                        updateModel.BusinessModel = item.BusinessModel;
                                        updateModel.TransactionPrice = item.TransactionPrice.ObjToDecimal();
                                        updateModel.ProcedureFee = item.ProcedureFee.ObjToDecimal();
                                        updateModel.VehicleOwner = item.VehicleOwner;
                                        updateModel.BackCarDate = item.BackCarDate.TryToDate();
                                        updateModel.Remark = item.VehicleModel;
                                        updateModel.ChangeDate = DateTime.Now;
                                        updateModel.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                                        updateDisposeIncomeList.Add(updateModel);
                                    }
                                }
                                if (updateDisposeIncomeList.Count > 0)
                                {
                                    db.Updateable<Business_DisposeIncome>(updateDisposeIncomeList).ExecuteCommand();
                                    resultModel.IsSuccess = true;
                                    resultModel.Status = "1";
                                }
                                else
                                {
                                    resultModel.Status = "2";
                                    resultModel.ResultInfo = "没有匹配到车牌号";
                                }
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