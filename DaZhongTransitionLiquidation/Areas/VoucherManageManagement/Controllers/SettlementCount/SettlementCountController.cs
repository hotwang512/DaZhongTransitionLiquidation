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
using System.Drawing.Printing;
using System.Text;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;

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
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetSettlementCountData(string year)
        {
            var dataList = new List<SettlementCountList>();
            DbBusinessDataService.Command(db =>
            { 
                var company = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == UserInfo.AccountModeCode && x.Code == UserInfo.CompanyCode).ToList().FirstOrDefault().Abbreviation;
                if (UserInfo.AccountModeCode == "1002" && (UserInfo.CompanyCode == "02" || UserInfo.CompanyCode == "03" || UserInfo.CompanyCode == "04" || UserInfo.CompanyCode == "05"))
                {
                    //按管理公司分类
                    dataList = db.Ado.SqlQuery<SettlementCountList>(@"select BusinessType,YearMonth,MANAGEMENT_COMPANY,SUM(Account) as Account from Business_SettlementCount where
                            MANAGEMENT_COMPANY=@COMPANY and Substring(YearMonth,0,5)=@Year group by BusinessType,YearMonth,BELONGTO_COMPANY ", 
                            new { COMPANY = company, Year = year }).ToList();
                }
                else
                {
                    //按所属公司分类
                    dataList = db.Ado.SqlQuery<SettlementCountList>(@"select BusinessType,YearMonth,BELONGTO_COMPANY,SUM(Account) as Account from Business_SettlementCount where
                            BELONGTO_COMPANY=@COMPANY and Substring(YearMonth,0,5)=@Year group by BusinessType,YearMonth,BELONGTO_COMPANY ", 
                            new { COMPANY = company, Year = year }).ToList();
                }
                foreach (var item in dataList)
                {
                    item.YearMonth = item.YearMonth.Substring(4,2).TryToInt().TryToString();
                    if (item.BusinessType.Contains("-"))
                    {
                        item.Business = item.BusinessType.Split("-")[0];
                        item.BusinessType = item.BusinessType.Split("-")[1];
                    }
                    else
                    {
                        item.Business = item.BusinessType;
                    }
                }
                //string uniqueKey = PubGet.GetUserKey + "SettlementCount";
                //CacheManager<List<Business_SettlementCount>>.GetInstance().Add(uniqueKey, data);
            });
            return Json(
                 dataList,
                 "application/json",
                 Encoding.UTF8,
                 JsonRequestBehavior.AllowGet
             );
        }
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
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
                    //worksheet.PageSetup.PaperSize
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