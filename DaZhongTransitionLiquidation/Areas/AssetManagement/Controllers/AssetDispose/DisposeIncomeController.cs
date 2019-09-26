using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
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
                    .Where(x => x.SubmitStatus == 0)
                    .WhereIF(PlateNumber != null, i => i.DepartmentVehiclePlateNumber.Contains(PlateNumber) || i.OraclePlateNumber.Contains(PlateNumber) || i.ImportPlateNumber.Contains(PlateNumber))
                    .OrderBy(i => i.CreateDate, OrderByType.Desc).ToList();
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
                            var maxTaxes =
                                db.SqlQueryable<v_TaxesInfo>(
                                        @"SELECT top 1 * FROM Business_TaxesInfo info order by convert(int,info.Year),convert(int,info.Month) desc")
                                    .First();
                            var TaxesList = db.Ado.SqlQuery<v_TaxesInfo>(@"select b.CompanyCode,b.AccountModeCode, a.Code,a.ParentCode,a.Descrption,b.TaxesType,b.TaxRate,a.VGUID as KeyVGUID,b.VGUID from Business_SevenSection as a
                                    left join Business_TaxesInfo as b on a.VGUID = b.SubjectVGUID and b.Year='2019'and b.Month='9'
                                    where a.SectionVGUID = 'B63BD715-C27D-4C47-AB66-550309794D43'
                                    and (a.Code like '%6403%' or a.Code like '%2221%') order by Code", new { Year = maxTaxes.Year, Month = maxTaxes.Month }).ToList();
                            var disposeIncomeList = db.Queryable<Business_DisposeIncome>().ToList();
                            var ssList = db.Queryable<Business_SevenSection>().Where(x =>
                                     x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43").ToList();
                            var updateDisposeIncomeList = new List<Business_DisposeIncome>();
                            var disposeAuctionImportList = new List<Excel_DisposeIncomeAuction>();
                            var dt = ExcelHelper.ExportToDataTable(filePath, true);
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                var dispose = new Excel_DisposeIncomeAuction();
                                dispose.ImportPlateNumber = dt.Rows[i][0].ToString();
                                dispose.DisposeIncomeValue = dt.Rows[i][1].ToString();
                                dispose.ServiceFee = dt.Rows[i][2].ToString();
                                dispose.ConsignFee = dt.Rows[i][3].ToString();
                                dispose.SaleType = dt.Rows[i][4].ToString();
                                dispose.SaleMonth = dt.Rows[i][5].ToString();
                                disposeAuctionImportList.Add(dispose);
                            }
                            foreach (var item in disposeAuctionImportList)
                            {
                                if (disposeIncomeList.Any(x =>
                                        x.DepartmentVehiclePlateNumber.Contains(item.ImportPlateNumber)) ||
                                    disposeIncomeList.Any(x => x.OraclePlateNumber.Contains(item.ImportPlateNumber)))
                                {
                                    var updateModel = disposeIncomeList.First(x => x.DepartmentVehiclePlateNumber.Contains(item.ImportPlateNumber));
                                    updateModel.ImportPlateNumber = item.ImportPlateNumber;
                                    //updateModel.VehicleModel = item.VehicleModel;
                                    //updateModel.BackCarDate = item.BackCarDate.TryToDate();
                                    //updateModel.BusinessModel = item.BusinessModel;
                                    updateModel.ServiceFee = item.ServiceFee.TryToDecimal();
                                    updateModel.ConsignFee = item.ConsignFee.TryToDecimal();
                                    updateModel.DisposeIncomeValue = item.DisposeIncomeValue.TryToDecimal();
                                    updateModel.SaleType = item.SaleType;
                                    updateModel.SaleMonth = item.SaleMonth;
                                    updateModel.ChangeDate = DateTime.Now;
                                    updateModel.ChangeUser = cache[PubGet.GetUserKey].LoginName;
                                    //计算税金，收入
                                    var companyInfo = ssList.First(x => x.Abbreviation == updateModel.ManageCompany);
                                    //companyInfo.Code = "01";
                                    var AddedValueTax = TaxesList.First(x => x.AccountModeCode == companyInfo.AccountModeCode &&
                                                                             x.CompanyCode == companyInfo.Code && x.TaxesType.StartsWith("旧车处置增值税"));
                                    var ConstructionTax = TaxesList.First(x => x.AccountModeCode == companyInfo.AccountModeCode &&
                                                                               x.CompanyCode == companyInfo.Code && x.TaxesType == "城建税");
                                    var AdditionalEducationTax = TaxesList.First(x => x.AccountModeCode == companyInfo.AccountModeCode &&
                                                                               x.CompanyCode == companyInfo.Code && x.TaxesType == "教育费附加");
                                    var LocalAdditionalEducationTax = TaxesList.First(x => x.AccountModeCode == companyInfo.AccountModeCode &&
                                                                               x.CompanyCode == companyInfo.Code && x.TaxesType == "地方教育费附加");
                                    updateModel.AddedValueTax = updateModel.DisposeIncomeValue.TryToDecimal() * decimal.Parse(AddedValueTax.TaxRate.Split("|")[0].Replace("%","")) / 100;
                                    updateModel.ConstructionTax = updateModel.AddedValueTax.TryToDecimal() * decimal.Parse(ConstructionTax.TaxRate.Replace("%", "")) / 100;
                                    updateModel.AdditionalEducationTax = updateModel.AddedValueTax.TryToDecimal() * decimal.Parse(AdditionalEducationTax.TaxRate.Replace("%", "")) / 100;
                                    updateModel.LocalAdditionalEducationTax = updateModel.AddedValueTax.TryToDecimal() * decimal.Parse(LocalAdditionalEducationTax.TaxRate.Replace("%", "")) / 100;
                                    if (updateModel.BusinessModel == "租赁模式(轻资产轻人员)-长租车")
                                    {
                                        updateModel.ReturnToPilot =
                                            updateModel.DisposeIncomeValue - updateModel.AddedValueTax -
                                            updateModel.ConstructionTax - updateModel.AdditionalEducationTax -
                                            updateModel.LocalAdditionalEducationTax;
                                        updateModel.NetIncomeValue = 0;
                                    }
                                    else
                                    {
                                        updateModel.NetIncomeValue =
                                            updateModel.DisposeIncomeValue - updateModel.AddedValueTax -
                                            updateModel.ConstructionTax - updateModel.AdditionalEducationTax -
                                            updateModel.LocalAdditionalEducationTax;
                                        updateModel.ReturnToPilot = 0;
                                    }
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
        public JsonResult SubmitDisposeIncome(List<Guid> guids)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                var DisposeProfitLossList = new List<Business_DisposeProfitLoss>();
                var result = db.Ado.UseTran(() =>
                {
                    var IncomeList = db.Queryable<Business_DisposeIncome>().Where(x => x.SubmitStatus == 0 && guids.Contains(x.VGUID)).ToList();
                    foreach (var item in IncomeList)
                    {
                        if (!item.DisposeIncomeValue.IsNullOrEmpty() && !item.ImportPlateNumber.IsNullOrEmpty())
                        {
                            var DisposeProfitLoss = db.Queryable<Business_DisposeProfitLoss>().First(x => x.AssetID == item.AssetID);
                            DisposeProfitLoss.ImportPlateNumber = item.ImportPlateNumber;
                            DisposeProfitLoss.VehicleModel = item.VehicleModel;
                            DisposeProfitLoss.Price = item.DisposeIncomeValue;
                            DisposeProfitLoss.Taxes = item.AddedValueTax + item.ConstructionTax + item.AdditionalEducationTax + item.LocalAdditionalEducationTax;
                            DisposeProfitLoss.DriverRentCarFee = item.ReturnToPilot;
                            DisposeProfitLoss.RealizedProfitLoss = DisposeProfitLoss.Price - DisposeProfitLoss.Taxes - DisposeProfitLoss.DriverRentCarFee;
                            DisposeProfitLoss.ManageCompany = item.ManageCompany;
                            DisposeProfitLoss.BelongToCompany = item.BelongToCompany;
                            DisposeProfitLoss.SaleMonth = item.SaleMonth;
                            DisposeProfitLoss.SaleType = item.SaleType;
                            DisposeProfitLoss.BusinessModel = item.BusinessModel;
                            DisposeProfitLoss.BackCarDate = item.BackCarDate;
                            DisposeProfitLoss.BackCarAge = item.BackCarAge;
                            DisposeProfitLossList.Add(DisposeProfitLoss);
                            item.SubmitStatus = 1;
                        }
                    }
                    db.Updateable<Business_DisposeProfitLoss>(DisposeProfitLossList).ExecuteCommand();
                    db.Updateable<Business_DisposeIncome>(IncomeList).UpdateColumns(x => new { x.SubmitStatus}).ExecuteCommand();
                });
                resultModel.IsSuccess = result.IsSuccess;
                resultModel.ResultInfo = "已提交数量：" + DisposeProfitLossList.Count;
                resultModel.Status = resultModel.IsSuccess ? "1" : "0";
            });
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
    }
}