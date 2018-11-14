using System.Data;
using System.Web;
using Aspose.Cells;
using System.Drawing;

namespace DaZhongTransitionLiquidation.Common
{
    public class ExcelHelper
    {
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="templatePath">模板名称</param>
        /// <param name="fileName">导出文件名称</param>
        /// <param name="dataSource">数据源</param>
        /// <param name="isSpecialStyle">是否有特殊样式</param>
        public static void ExportExcel(string templatePath, string fileName, DataTable dataSource, bool isSpecialStyle = false)
        {
            string rootPath = HttpContext.Current.Server.MapPath(templatePath);
            Workbook wk = new Workbook(rootPath);
            WorkbookDesigner designer = new WorkbookDesigner(wk);
            designer.SetDataSource(dataSource);
            designer.Process();
            Worksheet sheet = wk.Worksheets[0]; //工作表 
            Cells cells = sheet.Cells;//单元格  

            #region 设置特殊样式
            if (isSpecialStyle)
            {
                for (int i = 0; i < dataSource.Rows.Count; i++)
                {
                    if (dataSource.Rows[i]["ReasonStatus"].ToString() == "True")
                    {
                        for (int j = 0; j < 8; j++)  //Excel总共有8列
                        {
                            cells[i + 1, j].SetStyle(GetStyle(wk));  //这一行全部标红
                            var style = cells[i + 1, 3].GetStyle();
                            style.Custom = "yyyy-MM-dd hh:mm:ss";
                            cells[i + 1, 3].SetStyle(style);
                            var style2 = cells[i + 1, 6].GetStyle();
                            style2.Custom = "yyyy-MM-dd hh:mm:ss";
                            cells[i + 1,6].SetStyle(style);
                        }
                    }
                }
              
            }
            #endregion
            designer.Workbook.Save(HttpContext.Current.Response, fileName, ContentDisposition.Inline, new XlsSaveOptions());
        }

        public static Style GetStyle(Workbook workbook)
        {
            Style style = workbook.CreateStyle();
            style.Font.Color = Color.Red;//红色字体
            style.Font.Name = "Calibri";
            style.Font.Size = 9;
            style.HorizontalAlignment = TextAlignmentType.Center;//文字居中 
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            return style;
        }

        //public static Style GetStyle2(Workbook workbook)
        //{
        //    Style style = workbook.CreateStyle();
        //    style.Font.Color = Color.Red;//红色字体
        //    style.Font.Name = "Calibri";
        //    style.Font.Size = 9;
        //    style.Custom = "yyyy-MM-dd hh:mm:ss";
        //    style.HorizontalAlignment = TextAlignmentType.Center;//文字居中 
        //    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
        //    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
        //    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
        //    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
        //    return style;
        //}


        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="templateFileName">模板名称</param>
        /// <param name="fileName">导出文件名称</param>
        /// <param name="dataSource">数据源</param>
        public static void ExportExcel(string templateFileName, string fileName)
        {
            string rootPath = HttpContext.Current.Server.MapPath(string.Format("~/Template/{0}", templateFileName));
            Workbook wk = new Workbook(rootPath);
            WorkbookDesigner designer = new WorkbookDesigner(wk);
            designer.Process();
            designer.Workbook.Save(HttpContext.Current.Response, fileName, ContentDisposition.Inline, designer.Workbook.SaveOptions);
        }
    }
}