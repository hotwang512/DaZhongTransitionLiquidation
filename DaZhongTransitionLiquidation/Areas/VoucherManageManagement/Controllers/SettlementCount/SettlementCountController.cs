using Aspose.Cells;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spire.Xls;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.SettlementCount
{
    public class SettlementCountController : BaseController
    {
        public SettlementCountController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/SettlementCount
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetSettlementCountData(string yearMonth)
        {
            var jsonResult = new JsonResultModel<SettlementCountList>();
            DbBusinessDataService.Command(db =>
            {
                var dataList = new List<SettlementCountList>();
                var data = db.Queryable<Business_SettlementCount>().Where(x => x.YearMonth == yearMonth && x.BusinessType != "小计").OrderBy("Model,ClassType,CarType,BusinessKey").ToList();
                foreach (var item in data)
                {
                    var carAccount = data.Where(x => x.Model == item.Model && x.ClassType == item.ClassType && x.BusinessKey == item.BusinessKey
                                                && x.BusinessType == item.BusinessType).ToList();
                    var isAny = dataList.Any(x => x.Model == item.Model && x.ClassType == item.ClassType && x.BusinessKey == item.BusinessKey
                                                && x.BusinessType == item.BusinessType);
                    if (isAny)
                    {
                        continue;
                    }
                    //for (int i = 0; i < carAccount.Count; i++)
                    //{

                    //}
                    SettlementCountList sc = new SettlementCountList();
                    sc.VGUID = item.VGUID;
                    sc.Model = item.Model;
                    sc.ClassType = item.ClassType;
                    sc.BusinessKey = item.BusinessKey == null ? item.BusinessType : item.BusinessKey;
                    sc.BusinessType = item.BusinessType;
                    sc.CarAccount1 = carAccount[0].Account;
                    sc.CarAccount3 = carAccount[1].Account;
                    dataList.Add(sc);
                }
                jsonResult.Rows = dataList;
                jsonResult.TotalRows = jsonResult.Rows.Count;
                string uniqueKey = PubGet.GetUserKey + "SettlementCount";
                CacheManager<List<Business_SettlementCount>>.GetInstance().Add(uniqueKey, data);
            });
            return Json(jsonResult.Rows, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSettlementCountJson(string yearMonth)
        {
            var jsonResult = new JsonResultModel<DataTable>();
            DbBusinessDataService.Command(db =>
            {
                var data1 = db.Ado.GetDataTable(@"select * from Business_SettlementCount where YearMonth=@YearMonth and BusinessType != '小计'", new { YearMonth = yearMonth });
                for (int i = 0; i < data1.Rows.Count; i++)
                {
                    var rowData = data1.Rows[i];
                    
                }
                var row1 = data1.Rows[0];
                var row2 = data1.Rows[1];
                var row3 = data1.Rows[2];
                jsonResult.Rows = null;
                jsonResult.TotalRows = jsonResult.Rows.Count;
            });
            return Json(jsonResult.Rows, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CountSettlementData(string YearMonth)
        {
            ResultModel<string> Result = new ResultModel<string>{IsSuccess = true,Status = "0" };
            DbBusinessDataService.Command(db =>
            {
                try
                {
                    var data = db.Ado.SqlQuery<Business_SettlementCount>(@"select c.Model,c.ClassType, c.CarType,c.BusinessType ,a.YearMonth,a.MODEL_DAYS as DAYS,c.Money,(a.MODEL_DAYS*c.Money) as Account,c.MoneyRow,c.MoneyColumns from Business_VehicleList as a
                            left join Business_AssetMaintenanceInfo as b on a.PLATE_NUMBER = b.PLATE_NUMBER
                            left join Business_SettlementImport as c on c.Model=b.MODEL_MAJOR and c.ClassType=b.MODEL_MINOR and
                            c.CarType = b.DESCRIPTION where b.OPERATING_STATE='在运' and c.Model is not null  and a.YearMonth=@YearMonth
                            order by c.Model,c.ClassType, c.CarType,c.BusinessType asc", new { YearMonth = YearMonth }).ToList();
                    foreach (var item in data)
                    {
                        item.VGUID = Guid.NewGuid();
                        if (item.BusinessType.Contains("-"))
                        {
                            item.BusinessKey = item.BusinessType.Split("-")[0].TryToString();
                            item.BusinessType = item.BusinessType.Split("-")[1].TryToString();
                        }
                        item.Founder = UserInfo.LoginName;
                        item.CreatTime = DateTime.Now;
                    }
                    if (data.Count > 0)
                    {
                        db.Deleteable<Business_SettlementCount>().Where(x => x.YearMonth == YearMonth).ExecuteCommand();
                        db.Insertable(data).ExecuteCommand();
                        Result.Status = "1";
                    }
                    else
                    {
                        Result.Status = "2";//当前月份下没有数据
                    }
                }
                catch (Exception ex)
                {
                    Result.ResultInfo = ex.Message;
                    Result.Status = "3";
                }
            });
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        public void ExportSettlementDatas()
        {
            DbBusinessDataService.Command(db =>
            {
                string uniqueKey = PubGet.GetUserKey + "SettlementCount";
                var data = CacheManager<List<Business_SettlementCount>>.GetInstance()[uniqueKey];
                if (data.Count > 0)
                {
                    Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook(Path.Combine(Server.MapPath("/Template"), "SettlementCount.xlsx"));
                    Aspose.Cells.Worksheet worksheet = workbook.Worksheets[0];
                    Cells cells = worksheet.Cells;
                    foreach (var item in data)
                    {
                        cells[item.MoneyRow, item.MoneyColumns].PutValue(item.Account);
                    }
                    worksheet.AutoFitColumns();
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    
                    //workbook.Save(System.Web.HttpContext.Current.Response, "SettlementCount", ContentDisposition.Attachment, new OoxmlSaveOptions());
                    MemoryStream excel = workbook.SaveToStream();
                    //Response.Clear();
                    //Response.ClearContent();
                    //Response.ClearHeaders();
                    //Response.ContentType = "application/vnd.ms-excel";
                    //Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", HttpUtility.UrlEncode("各种车型各种模式结算标准汇总") + DateTime.Now.ToString("yyyy-MM-dd").Trim() + ".xls"));

                    //excel.WriteTo(Response.OutputStream);
                    //Response.End();


                    //var ms = ExcelHelper.OutModelFileToStream(dt, "/Template/AssetsRetiremen.xlsx", "资产报废");
                    byte[] fileContents = excel.ToArray();
                    File(fileContents, "application/ms-excel", "各种车型各种模式结算标准汇总" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");


                    //workbook.Save(Path.Combine(dir, "SettlementCount.pdf"), SaveFormat.Pdf);
                }
                //var data1 = db.Queryable<Business_SettlementCount>().ToList();
                //var data2 = db.Queryable<Business_SettlementImport>().ToList();
                //foreach (var item in data2)
                //{
                //    if(item.BusinessType != "" && item.BusinessType != null)
                //    {
                //        if(item.BusinessType == "经济补偿金")
                //        {
                //            db.Updateable<Business_SettlementCount>(new { MoneyRow = item.MoneyRow, MoneyColumns = item.MoneyColumns }).Where(x => x.Model == item.Model && x.ClassType == item.ClassType && x.CarType == item.CarType
                //                               && x.BusinessKey == null && x.BusinessType == "经济补偿金").ExecuteCommand();
                //        }
                //        else
                //        {
                //            var businessKey = item.BusinessType.Split("-")[0];
                //            var businessType = item.BusinessType.Split("-")[1];
                //            var isAny = data1.Any(x => x.Model == item.Model && x.ClassType == item.ClassType && x.CarType == item.CarType
                //                                && x.BusinessKey == businessKey && x.BusinessType == businessType);
                //            if (isAny)
                //            {
                //                db.Updateable<Business_SettlementCount>(new { MoneyRow = item.MoneyRow, MoneyColumns = item.MoneyColumns }).Where(x => x.Model == item.Model && x.ClassType == item.ClassType && x.CarType == item.CarType
                //                                && x.BusinessKey == businessKey && x.BusinessType == businessType).ExecuteCommand();
                //            }
                //        }

                //    }                   
                //}
            });
            //return Json(Result, JsonRequestBehavior.AllowGet);
        }

        public void ExportSettlementData()
        {
            DbBusinessDataService.Command(db =>
            {
                string uniqueKey = PubGet.GetUserKey + "SettlementCount";
                var data = CacheManager<List<Business_SettlementCount>>.GetInstance()[uniqueKey];
                if (data.Count > 0)
                {
                    Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook(Path.Combine(Server.MapPath("/Template"), "SettlementCount.xlsx"));
                    Aspose.Cells.Worksheet worksheet = workbook.Worksheets[0];
                    Cells cells = worksheet.Cells;
                    foreach (var item in data)
                    {
                        cells[item.MoneyRow, item.MoneyColumns].PutValue(item.Account);
                    }
                    worksheet.AutoFitColumns();
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    //workbook.Save(System.Web.HttpContext.Current.Response, "SettlementCount", ContentDisposition.Attachment, new OoxmlSaveOptions());
                    workbook.Save(Path.Combine(dir, "SettlementCount.pdf"), SaveFormat.Pdf);

                    MemoryStream excel = workbook.SaveToStream();
                    Response.Clear();
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", HttpUtility.UrlEncode("车辆经营汇总") + DateTime.Now.ToString("yyyy-MM-dd").Trim() + ".xls"));

                    excel.WriteTo(Response.OutputStream);
                    Response.End();
                }
            });
        }
    }
}