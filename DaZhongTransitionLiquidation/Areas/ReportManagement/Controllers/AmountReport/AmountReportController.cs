using Aspose.Cells;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.ReportManagement.Controllers.AmountReport
{
    public class AmountReportController : BaseController
    {
        public AmountReportController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: ReportManagement/AmountReport
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AmountReport()
        {
            ViewBag.Channel = GetChannel();
            return View();
        }
        public List<T_Channel> GetChannel()
        {
            var result = new List<T_Channel>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<T_Channel>().ToList();

            });
            return result;
        }
        public JsonResult GetAmountReportData(string month, string channel = "")
        {
            var jsonResult = new JsonResultModelExt<usp_RevenueAmountReport>();
            DbBusinessDataService.Command(db =>
                {
                    var outputResult = db.Ado.UseStoredProcedure<dynamic>(() =>
                    {
                        string spName = "usp_RevenueAmountReport";
                        var p1 = new SugarParameter("@Month", month);
                        var p2 = new SugarParameter("@Channel", channel);
                        jsonResult.Rows = db.Ado.SqlQuery<usp_RevenueAmountReport>(spName, new SugarParameter[] { p1, p2 }).ToList();
                        jsonResult.TotalRows = jsonResult.Rows.Count;
                        return "";
                    });
                });
            return Json(jsonResult.Rows, JsonRequestBehavior.AllowGet);
        }

        public void GetAmountReportDataOut(string month, string companyName,string channel = "")
        {
           // var jsonResult = new JsonResultModelExt<usp_RevenueAmountReport>();

            DbBusinessDataService.Command(db =>
                {
                    var outputResult = db.Ado.UseStoredProcedure<dynamic>(() =>
                    {
                        string spName = "usp_RevenueAmountReport_plus";
                        var p1 = new SugarParameter("@Month", month);
                        var p2 = new SugarParameter("@Channel", channel);
                        var data  = db.Ado.GetDataTable(spName, new SugarParameter[] { p1, p2 });
                        ImportExcel(data, companyName, channel);
                        return "";
                    });
                });           
        }

        private void ImportExcel(DataTable data, string companyName,string channel)
        {
            Workbook workbook = new Workbook(); //工作簿
            Worksheet sheet = workbook.Worksheets[0]; //工作表
            Cells cells = sheet.Cells;//单元格
            var columns = data.Columns.Count;
            var cn = companyName.Split(",").ToArray();//公司数
            for (int i = 2; i <= columns - 5; i++)
            {
                var c = data.Columns[i].ToString();
                for (int j = 0; j < cn.Length; j++)
                {
                    if (c.ToString() == cn[j].ToString())
                    {
                        data.Columns[i].SetOrdinal(j + 2);//同步公司列顺序
                        break;
                    }
                }
            }
           
            var amountData = data.AsEnumerable().ToList();
            
            var channelNames = amountData[0].ItemArray[0];
            cells[2, 0].PutValue(channelNames);
            if(channel == "")
            {
                var a = -1;
                var m = 100;
                for (int j = 0; j < amountData.Count; j++)
                {
                    var channelName = amountData[j].ItemArray[0];
                    if (channelNames.ToString() == channelName.ToString())
                    {
                        a++;
                        cells[j + 3, 0].PutValue(amountData[j].ItemArray[1]);
                    }
                    else
                    {
                        m = a + 4;//分隔小计行
                        cells[j + 4, 0].PutValue(amountData[j].ItemArray[1]);
                    }                     
                    setCell(amountData[j], cells, cn, j, channel, amountData.Count, m);
                }
                cells[a + 4, 0].PutValue(amountData[a + 1].ItemArray[0]);
                cells[amountData.Count + 4, 0].PutValue("Total");
            }
            else
            {
                for (int j = 0; j < amountData.Count; j++)
                {
                    cells[j + 3, 0].PutValue(amountData[j].ItemArray[1]);
                    cells[amountData.Count + 3, 0].PutValue("Total");
                    setCell(amountData[j], cells, cn, j, channel, amountData.Count,0);                   
                }
            }
           

            sheet.AutoFitColumns();
            //workbook.Save(System.Web.HttpContext.Current.Response, "AmountReport", ContentDisposition.Attachment, new OoxmlSaveOptions());
            MemoryStream excel = workbook.SaveToStream();
            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", HttpUtility.UrlEncode("营收报表") + DateTime.Now.ToString("yyyy-MM-dd").Trim() + ".xls"));
          
            excel.WriteTo(Response.OutputStream);
            Response.End();
        }

        private void setCell(DataRow dataRow, Cells cells, string[] cn,int j,string channel,int count,int m)
        {
            var k = -1;
            Style s = new Style();
            s.Font.Name = "宋体";
            s.Font.Color = Color.Black;
            s.HorizontalAlignment = TextAlignmentType.Center;
            string[] EN = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
                                        "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ",
                                        "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ"
                    };
            string[] val = null;
            //int index = dataRow.ItemArray.Count() + 3;
            int index = count + 3;
            if (channel == "")
            {
                if (j >= m-3)
                {
                    j = j + 1;
                }
            }
            for (int i = 0; i < cn.Count() * 3 + 1; i++)
            {                              
                if (i % 3 == 1)
                {
                    k++;
                    if (k >= cn.Count())
                    {
                        break;
                    }                 
                    cells.Merge(0, i, 1, 3);
                    cells[0, i].PutValue(cn[k]);
                    cells[0, i].SetStyle(s);
                    cells[1, i].SetStyle(s);
                    if (k == cn.Count() - 1)
                    {
                        k = k + 2;
                    }
                    var value = dataRow.ItemArray[k+2].ToString();//循环几家公司的值                  
                    if (value == "")
                    {
                        val = new string[] { "", "", "" };
                    }
                    else
                    {
                        val = value.Split("|");
                    }
                }
                if (channel == "")
                {
                    cells[2, i + 1].Formula = "=SUM(" + EN[i + 1] + "4:" + EN[i + 1] + m + ")";
                    cells[m, i + 1].Formula = "=SUM(" + EN[i + 1] + (m + 2) + ":" + EN[i + 1] + (count + 4) + ")";
                    cells[count + 4, i + 1].Formula = "=SUM(" + EN[i + 1] + "3," + EN[i + 1] + (m + 1) + ")";
                    cells[100, i + 1].Formula = "";
                    if (i != 0 && i % 3 == 1)
                    {
                        cells[1, i].PutValue("营收缴费");
                        cells[j + 3, i].PutValue(val[0].TryToDecimal());
                    }
                    if (i != 0 && i % 3 == 2)
                    {
                        cells[1, i].PutValue("手续费");
                        cells[j + 3, i].PutValue(val[1].TryToDecimal());
                    }
                    if (i != 0 && i % 3 == 0)
                    {
                        cells[1, i].PutValue("银行收款");
                        cells[j + 3, i].PutValue(val[2].TryToDecimal());
                    }
                }
                else
                {
                    cells[2, i + 1].Formula = "=SUM(" + EN[i + 1] + "4:" + EN[i + 1] + "" + index + ")";
                    cells[index, i + 1].Formula = "=SUM(" + EN[i + 1] + "4:" + EN[i + 1] + "" + index + ")";
                    if (i != 0 && i % 3 == 1)
                    {
                        cells[1, i].PutValue("营收缴费");
                        cells[j + 3, i].PutValue(val[0].TryToDecimal());
                    }
                    if (i != 0 && i % 3 == 2)
                    {
                        cells[1, i].PutValue("手续费");
                        cells[j + 3, i].PutValue(val[1].TryToDecimal());
                    }
                    if (i != 0 && i % 3 == 0)
                    {
                        cells[1, i].PutValue("银行收款");
                        cells[j + 3, i].PutValue(val[2].TryToDecimal());
                    }
                }                             
            }            
        }
    }
}