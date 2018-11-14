using System;
using System.Data;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Areas.ReportManagement.Controllers.ExceptionDataReport
{
    public class ExceptionDataReportController : BaseController
    {
        // GET: ReportManagement/ExceptionDataReport
        public ExceptionDataReportController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        public ActionResult ExceptionData()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.ExceptionDataReport);
            return View();
        }

        /// <summary>
        /// 获取金额对账异常信息
        /// </summary>
        /// <param name="searchParams"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JsonResult GetReconciliations(U_RevenuePayment_Search searchParams, GridParams paras)
        {
            var jsonResult = new JsonResultModel<V_Report_Enterprisepayment>();
            
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public void Export(string paras)
        {
            U_RevenuePayment_Search searchParams = paras.JsonToModel<U_RevenuePayment_Search>();
            var start = !string.IsNullOrEmpty(searchParams.PayDateFrom) ? DateTime.Parse(searchParams.PayDateFrom + " 00:00:00") : DateTime.Parse("1900-01-01");
            var end = !string.IsNullOrEmpty(searchParams.PayDateTo) ? DateTime.Parse(searchParams.PayDateTo + " 23:59:59") : DateTime.MaxValue;
            DataTable dt = new DataTable();
            DbService.Command(db =>
            {
                dt = db.Queryable<Business_Data_Abnormal, Master_Organization>((b, m) => b.Department == m.Vguid.ToString())
                 .Select((b, m) => new V_Report_Enterprisepayment() { VGUID = SqlFunc.GetSelfAndAutoFill(b.VGUID), OrganizationName = m.OrganizationName }).MergeTable()
                 .WhereIF(!string.IsNullOrEmpty(searchParams.Name), i => i.name.Contains(searchParams.Name))
                 .Where(i => SqlFunc.Between(i.PayDate, start, end)).Where(i => i.ReasonStatus == true).OrderBy(i => i.PayDate, OrderByType.Desc).ToDataTable();
            });
            dt.TableName = "Report";
            ExcelHelper.ExportExcel("/Template/Report.xlsx", "异常报表" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls", dt);
        }
    }
}