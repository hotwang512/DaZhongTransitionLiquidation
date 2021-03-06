﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;
using SyntacticSugar;
using System.Net;
using DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity;

namespace DaZhongTransitionLiquidation.Areas.ReportManagement.Controllers.ReconciliationReport
{
    public class ReconciliationReportController : BaseController
    {
        // GET: ReportManagement/ReconciliationReport
        public ReconciliationReportController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        public ActionResult Reconciliation()
        {
            ViewData["level"] = GetMasterData(MasterVGUID.LevelVguid);
            ViewBag.UserSubDepartment = GetUserSubDepartment().ModelToJson();
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("88B332E4-DFA2-4591-AF09-EE786C73864A");
            ViewBag.channels = GetChannelInfos();
            return View();
        }
        /// <summary>
        /// 获取所有的渠道信息
        /// </summary>
        /// <returns></returns>
        public List<T_Channel> GetChannelInfos()
        {
            var channels = new List<T_Channel>();
            DbBusinessDataService.Command(db =>
            {
                channels = db.Queryable<T_Channel>().OrderBy(i => i.Id).ToList();
            });
            return channels;
        }

        /// <summary>
        /// 对账信息是否存在 
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public JsonResult IsExistReconciliationInfo(U_RevenuePayment_Search searchParams)
        {
            var start = !string.IsNullOrEmpty(searchParams.PayDateFrom) ? DateTime.Parse(searchParams.PayDateFrom + " 00:00:00") : DateTime.Parse("1900-01-01");
            var end = !string.IsNullOrEmpty(searchParams.PayDateTo) ? DateTime.Parse(searchParams.PayDateTo + " 23:59:59") : DateTime.MaxValue;
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbService.Command(db =>
                  {
                      var deps = GetChildOrgs(searchParams.Department).ToArray();
                      //首先查询已核算表
                      var isExistChecked = db.Queryable<Business_Data_Normal>()
                                 .WhereIF(!string.IsNullOrEmpty(searchParams.Channel), i => i.Channel_Id == searchParams.Channel)
                                 .WhereIF(!string.IsNullOrEmpty(searchParams.Department), i => deps.Contains(i.Department))
                                 .Where(i => SqlFunc.Between(i.PayDate, start, end)).Any();
                      if (isExistChecked)
                      {
                          resultModel.IsSuccess = true;
                          return;
                      }
                      //再查询异常表
                      var isExistException = db.Queryable<Business_Data_Abnormal>()
                                 .WhereIF(!string.IsNullOrEmpty(searchParams.Channel), i => i.Channel_Id == searchParams.Channel)
                                 .WhereIF(!string.IsNullOrEmpty(searchParams.Department), i => deps.Contains(i.Department))
                                 .Where(i => SqlFunc.Between(i.PayDate, start, end)).Any();
                      if (isExistException)
                      {
                          resultModel.IsSuccess = true;
                          return;
                      }
                      resultModel.IsSuccess = false;
                      //  TempData["ReconciliationInfo"] = query.ToList();
                  });
            return Json(resultModel);
        }

        /// <summary>
        /// 删除已经存在的对账信息
        /// </summary>
        /// <returns></returns>
        public JsonResult DeleteReconciliationInfos()
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0" };
            DbService.Command(db =>
            {
                var reconciliationInfos = TempData["ReconciliationInfo"] as List<Business_Data_Normal>;
                int saveChanges = db.Deleteable<Business_Data_Normal>().Where(reconciliationInfos).ExecuteCommand();
                resultModel.Status = saveChanges == reconciliationInfos.Count ? "1" : "0";
            });
            return Json(resultModel);
        }

        /// <summary>
        /// 获取金额对账信息
        /// </summary>
        /// <param name="searchParams"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        //public JsonResult GetReconciliations(U_RevenuePayment_Search searchParams, GridParams paras)
        //{
        //    var jsonResult = new JsonResultModelExt<U_Report_Reconciliation>();
        //    var start = !string.IsNullOrEmpty(searchParams.PayDateFrom) ? DateTime.Parse(searchParams.PayDateFrom + " 00:00:00") : DateTime.Parse("1900-01-01");
        //    var end = !string.IsNullOrEmpty(searchParams.PayDateTo) ? DateTime.Parse(searchParams.PayDateTo + " 23:59:59") : DateTime.MaxValue;
        //    var sum_PaymentAmount = 0.0m;
        //    var sum_Remitamount = 0.0m;
        //    DbService.Command(db =>
        //    {
        //        int pageCount = 0;
        //        paras.pagenum = paras.pagenum + 1;
        //        var layeredValue = "";
        //        if (searchParams.Level == "2")
        //        {
        //            layeredValue = searchParams.Channel;
        //        }
        //        if (searchParams.Level == "3")
        //        {
        //            layeredValue = searchParams.Department;
        //        }
        //        var outputResult = db.Ado.UseStoredProcedure(() =>
        //        {
        //            string spName = "usp_Revenuepayment_Reconciliation";
        //            var p1 = new SugarParameter("@Startdate", start);
        //            var p2 = new SugarParameter("@Enddate", end);
        //            var p3 = new SugarParameter("@LayeredType", searchParams.Level);
        //            var p4 = new SugarParameter("@LayeredValue", layeredValue);
        //            var p5 = new SugarParameter("@PageCurrent", paras.pagenum);
        //            var p6 = new SugarParameter("@PageSize", paras.pagesize);
        //            var p7 = new SugarParameter("@RecordCount", null, true);//isOutput=true
        //            var p8 = new SugarParameter("@Sum_PaymentAmount", null, true);//isOutput=true
        //            var p9 = new SugarParameter("@Sum_Remitamount", null, true);//isOutput=true
        //            var dbResult = db.Ado.SqlQuery<U_Report_Reconciliation>(spName, p1, p2, p3, p4, p5, p6, p7, p8, p9);
        //            pageCount = p7.Value.ObjToInt();
        //            sum_PaymentAmount = p8.Value.ObjToDecimal();
        //            sum_Remitamount = p9.Value.ObjToDecimal();
        //            return dbResult;
        //        });
        //        jsonResult.Rows = outputResult;
        //        jsonResult.TotalRows = pageCount;
        //        jsonResult.SumPaymentAmount = sum_PaymentAmount;
        //        jsonResult.SumRemitamount = sum_Remitamount;
        //    });
        //    return Json(jsonResult, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetReconciliations(U_RevenuePayment_Search searchParams, GridParams paras)
        {
            var jsonResult = new JsonResultModelExt<v_Business_Reconciliation>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                paras.pagenum = paras.pagenum + 1;
                DateTime dateFrom = DateTime.Parse(searchParams.PayDateFrom);
                DateTime dateTo = DateTime.Parse(searchParams.PayDateTo).AddDays(1);
                var outputResult = db.Queryable<v_Business_Reconciliation>();
                if (searchParams.PayDateFrom != null && searchParams.PayDateTo != null)
                {
                    outputResult.Where(i => i.BankBillDate >= dateFrom && i.BankBillDate <= dateTo);
                }
                else if (searchParams.PayDateFrom != null)
                {
                    outputResult.Where(i => i.BankBillDate >= dateFrom);
                }
                else if (searchParams.PayDateTo != null)
                {
                    outputResult.Where(i => i.BankBillDate <= dateTo);
                }
                if (!string.IsNullOrEmpty(searchParams.Channel))
                {
                    outputResult.Where(i => i.Channel_Id == searchParams.Channel);
                }
                if (!string.IsNullOrEmpty(searchParams.Status))
                {
                    outputResult.Where(i => i.Status == searchParams.Status);
                }
                outputResult.OrderBy(c => c.BankBillDate, OrderByType.Desc);
                jsonResult.Rows = outputResult.ToList();
                //jsonResult.Rows = outputResult.ToPageList(paras.pagenum, paras.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取对账详细信息
        /// </summary>
        /// <param name="searchParams"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JsonResult GetReconciliationsDetail(U_RevenuePayment_Search searchParams, GridParams paras)
        {
            var jsonResult = new JsonResultModel<V_Report_Enterprisepayment>();
            //var start = DateTime.Parse(searchParams.PayDateFrom);
            // var status = searchParams.ReasonStatus == "1";
            // int pageCount = 0;
            DbService.Command(db =>
            {
                var deps = GetChildOrgs(searchParams.Department).ToArray();
                paras.pagenum = paras.pagenum + 1;
                var strWhere = "";
                var sql = @";with cte_1 as 
                            (
                            select * from Business_Data_Abnormal 
                            union all
                            select * from Business_Data_Normal
                            ),
                            cte_2 as (
                            select cte_1.* ,b.OrganizationName,t.Name as ChannelName ,row_number() over(order by PayDate DESC) as rn from cte_1 
                            left join  Master_Organization b on b.Vguid=cte_1.Department 
                            left join  DEV_DaZhong_BusinessData_ReckoningSystem.dbo.T_Channel t on t.Id=cte_1.Channel_Id
                                where  CONVERT(varchar(100), PayDate, 23)='{0}' {1}
                            )  ";
                if (!string.IsNullOrEmpty(searchParams.Channel))
                {
                    strWhere = " and t.Id='{0}'".ToFormat(searchParams.Channel);
                }
                if (!string.IsNullOrEmpty(searchParams.Department))
                {
                    strWhere = " and cte_1.Department in ({0})".ToFormat(string.Join(",", deps.Select(i => "'" + i + "'")));
                }
                if (!string.IsNullOrEmpty(searchParams.ReasonStatus))
                {
                    strWhere += " and ReasonStatus='{0}'".ToFormat(searchParams.ReasonStatus);
                }
                sql = sql.ToFormat(searchParams.PayDateFrom, strWhere);
                var sqlRows = sql + " select * from cte_2 where rn between ({0}-1)*{1}+1  and {0}*{1}";
                var sqlCount = sql + " select count(1) from cte_2";
                jsonResult.Rows = db.Ado.SqlQuery<V_Report_Enterprisepayment>(sqlRows.ToFormat(paras.pagenum, paras.pagesize));
                jsonResult.TotalRows = db.Ado.SqlQuery<int>(sqlCount).Single();
                //var query = db.Queryable<Business_Data_Normal, Master_Organization, T_Channel>(
                //    (b, m, t) => new object[]
                //    {
                //        JoinType.Left,b.Department == m.Vguid.ToString(),
                //        JoinType.Left,b.Channel_Id == t.Id,
                //    })
                //.Select((b, m, t) => new V_Report_Enterprisepayment() { VGUID = SqlFunc.GetSelfAndAutoFill(b.VGUID), OrganizationName = m.OrganizationName, ChannelName = t.Name }).MergeTable()
                //.WhereIF(!string.IsNullOrEmpty(searchParams.Channel), i => i.Channel_Id == searchParams.Channel)
                //.WhereIF(!string.IsNullOrEmpty(searchParams.Department), i => deps.Contains(i.Department))
                //// .WhereIF(!string.IsNullOrEmpty(searchParams.ReasonStatus), i => i.ReasonStatus == status)
                //.Where(i => SqlFunc.DateIsSame(i.PayDate, start)).OrderBy(i => i.PayDate, OrderByType.Desc);
                //jsonResult.Rows = query.ToPageList(paras.pagenum, paras.pagesize, ref pageCount);
                //jsonResult.TotalRows = pageCount;
            });

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取对账数据，包含营收系统总额，营收总额、T+1总额，银行总额
        /// </summary>
        /// <param name="date"></param>
        /// <param name="Channel_Id"></param>
        /// <returns></returns>
        public JsonResult GetTotalAmount(string BankDate, string RevenueDate, string Channel_Id)
        {
            var resultModel = ReconciliationReportPack.GetTotalAmount(DbBusinessDataService, BankDate, RevenueDate, Channel_Id);
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 营收数据
        /// </summary>
        /// <param name="date"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JsonResult GetRevenueDetail(string RevenueDate, string Channel_Id, GridParams paras)
        {
            var jsonResult = new JsonResultModel<V_Revenuepayment_Information_Date>();
            int pageCount = 0;

            //DateTime sDataTime = DateTime.Parse(date + " 00:00:00");
            //DateTime eDataTime = DateTime.Parse(date + " 23:59:59");
            DbBusinessDataService.Command(db =>
            {
                RevenueDate.Replace(" ", "");
                string[] revenueDates = RevenueDate.Split(",", StringSplitOptions.RemoveEmptyEntries);
                var query = db.Queryable<V_Revenuepayment_Information_Date>().Where(i => i.Channel_Id == Channel_Id).In(i => i.PayDateStr, revenueDates);
                jsonResult.Rows = query.ToPageList(paras.pagenum, paras.pagesize, ref pageCount);
                foreach (var revenuepayment in jsonResult.Rows)
                {
                    revenuepayment.DriverBearFees = revenuepayment.ActualAmount - revenuepayment.PaymentAmount;
                    revenuepayment.CompanyBearsFees = revenuepayment.copeFee - revenuepayment.DriverBearFees;
                    revenuepayment.ChannelPayableAmount = revenuepayment.ActualAmount - revenuepayment.copeFee;
                }
                jsonResult.TotalRows = jsonResult.Rows.Count;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// T+1
        /// </summary>
        /// <param name="date"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JsonResult GetChannelDetail(string RevenueDate, string Channel_Id, GridParams paras)
        {
            var jsonResult = new JsonResultModel<v_Business_T1Data_Information_Date>();
            int pageCount = 0;

            DbBusinessDataService.Command(db =>
            {
                RevenueDate.Replace(" ", "");
                string[] revenueDates = RevenueDate.Split(",", StringSplitOptions.RemoveEmptyEntries);
                var query = db.Queryable<v_Business_T1Data_Information_Date>().Where(i => i.Channel_Id == Channel_Id).In(i => i.RevenuetimeStr, revenueDates);
                jsonResult.Rows = query.ToPageList(paras.pagenum, paras.pagesize, ref pageCount);
                foreach (var t1Data in jsonResult.Rows)
                {
                    t1Data.DriverBearFees = t1Data.PaidAmount - t1Data.Remitamount;
                    t1Data.CompanyBearsFees = t1Data.RevenueFee - t1Data.DriverBearFees;
                    t1Data.ChannelPayableAmount = t1Data.PaidAmount - t1Data.RevenueFee;
                }
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 押金
        /// </summary>
        /// <param name="date"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JsonResult GetDepositDetail(string RevenueDate, string Channel_Id, GridParams paras)
        {
            var jsonResult = new JsonResultModel<v_Business_T1Data_Information_Date_2>();
            int pageCount = 0;

            DbBusinessDataService.Command(db =>
            {
                RevenueDate.Replace(" ", "");
                string[] revenueDates = RevenueDate.Split(",", StringSplitOptions.RemoveEmptyEntries);
                var query = db.Queryable<v_Business_T1Data_Information_Date_2>().Where(i => i.Channel_Id == Channel_Id).In(i => i.RevenuetimeStr, revenueDates);
                jsonResult.Rows = query.ToPageList(paras.pagenum, paras.pagesize, ref pageCount);
                foreach (var t1Data in jsonResult.Rows)
                {
                    t1Data.DriverBearFees = t1Data.PaidAmount - t1Data.Remitamount;
                    t1Data.CompanyBearsFees = t1Data.RevenueFee - t1Data.DriverBearFees;
                    t1Data.ChannelPayableAmount = t1Data.PaidAmount - t1Data.RevenueFee;
                }
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 银行
        /// </summary>
        /// <param name="date"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public JsonResult GetBankDetail(string BankDate, string Channel_Id, GridParams paras)
        {
            var jsonResult = new JsonResultModel<v_Bank_desc>();
            int pageCount = 0;

            DateTime sDataTime = DateTime.Parse(BankDate + " 00:00:00");
            DateTime eDataTime = DateTime.Parse(BankDate + " 23:59:59");
            DbBusinessDataService.Command(db =>
            {
                var query = db.Queryable<v_Bank_desc>()
                .Where(i => i.ArrivedTime >= sDataTime && i.ArrivedTime <= eDataTime);
                if (!string.IsNullOrEmpty(Channel_Id))
                {
                    query.Where(i => i.Channel_Id == Channel_Id);
                }
                jsonResult.Rows = query.ToPageList(paras.pagenum, paras.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;

            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RevenuepaymentReconciliation(DateTime BankDate,
            string RevenueDate,
            string Channel_Id,
            decimal RevenueSystemTotal,
            decimal ArrearsRevenueTotal,
            decimal RevenueTotal,
            decimal ArrearsChannelTotal,
            decimal ChannelTotal,
            decimal ArrearsDepositTotal,
            decimal DepositTotal,
            decimal BankTotal)
        {
            string operatorUser = UserInfo.LoginName;

            var resultModel = ReconciliationReportPack.RevenuepaymentReconciliation(
                DbBusinessDataService,
                UserInfo.LoginName,
                BankDate,
                RevenueDate,
                Channel_Id,
                RevenueSystemTotal,
                ArrearsRevenueTotal,
                RevenueTotal,
                ArrearsChannelTotal,
                ChannelTotal,
                ArrearsDepositTotal,
                DepositTotal,
                BankTotal);
            return Json(resultModel, JsonRequestBehavior.AllowGet);
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
                dt = db.Queryable<Business_Data_Normal, Master_Organization>((b, m) => b.Department == m.Vguid.ToString())
                .Select((b, m) => new V_Report_Enterprisepayment() { VGUID = SqlFunc.GetSelfAndAutoFill(b.VGUID), OrganizationName = m.OrganizationName }).MergeTable()
                .WhereIF(!string.IsNullOrEmpty(searchParams.Name), i => i.name.Contains(searchParams.Name))
                .Where(i => SqlFunc.Between(i.PayDate, start, end)).OrderBy(i => i.PayDate, OrderByType.Desc).ToDataTable();
            });
            dt.TableName = "Report";
            ExcelHelper.ExportExcel("/Template/Report.xlsx", "对账报表" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls", dt, true);
        }

        /// <summary>
        /// 获取当前部门的子部门
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public List<string> GetChildOrgs(string org)
        {
            var subDepartments = new List<string>();
            if (string.IsNullOrEmpty(org))
            {
                return subDepartments;
            }
            DbService.Command(db =>
            {
                var organizations = db.Ado.UseStoredProcedure().SqlQuery<Master_Organization>("usp_Organization_Underling", new { orgvguid = org });
                subDepartments = organizations.Select(i => i.Vguid.ToString()).ToList();
            });
            return subDepartments;
        }

        /// <summary>
        /// 导出明细
        /// </summary>
        /// <param name="paras"></param>
        public void ExportDetail(string paras)
        {
            U_RevenuePayment_Search searchParams = paras.JsonToModel<U_RevenuePayment_Search>();
            //    var start = !string.IsNullOrEmpty(searchParams.PayDateFrom) ? DateTime.Parse(searchParams.PayDateFrom + " 00:00:00") : DateTime.Parse("1900-01-01");
            DataTable dt = new DataTable();
            DbService.Command(db =>
            {
                var deps = GetChildOrgs(searchParams.Department).ToArray();
                var strWhere = "";
                var sql = @";with cte_1 as 
                            (
                            select * from Business_Data_Abnormal 
                            union all
                            select * from Business_Data_Normal
                            ),
                            cte_2 as (
                            select cte_1.* ,b.OrganizationName,t.Name as ChannelName ,row_number() over(order by PayDate DESC) as rn from cte_1 
                            left join  Master_Organization b on b.Vguid=cte_1.Department 
                            left join  DEV_DaZhong_BusinessData_ReckoningSystem.dbo.T_Channel t on t.Id=cte_1.Channel_Id
                                where  CONVERT(varchar(100), PayDate, 23)='{0}' {1}
                            )  select * from cte_2";
                if (!string.IsNullOrEmpty(searchParams.Channel))
                {
                    strWhere = " and t.Id='{0}'".ToFormat(searchParams.Channel);
                }
                if (!string.IsNullOrEmpty(searchParams.Department))
                {
                    strWhere = " and cte_1.Department in ({0})".ToFormat(string.Join(",", deps.Select(i => "'" + i + "'")));
                }
                if (!string.IsNullOrEmpty(searchParams.ReasonStatus))
                {
                    strWhere += " and ReasonStatus='{0}'".ToFormat(searchParams.ReasonStatus);
                }
                sql = sql.ToFormat(searchParams.PayDateFrom, strWhere);
                dt = db.Ado.GetDataTable(sql);
                //dt = db.Queryable<Business_Data_Normal, Master_Organization>((b, m) => b.Department == m.Vguid.ToString())
                //.Select((b, m) => new V_Report_Enterprisepayment() { VGUID = SqlFunc.GetSelfAndAutoFill(b.VGUID), OrganizationName = m.OrganizationName }).MergeTable()
                //.WhereIF(!string.IsNullOrEmpty(searchParams.Channel), i => i.Channel_Id == searchParams.Channel)
                //.WhereIF(!string.IsNullOrEmpty(searchParams.Department), i => deps.Contains(i.Department))
                //.Where(i => SqlFunc.DateIsSame(i.PayDate, start)).OrderBy(i => i.PayDate, OrderByType.Desc).ToDataTable();
            });
            dt.TableName = "Report";
            ExcelHelper.ExportExcel("/Template/Report.xlsx", "对账报表" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls", dt, true);
        }

        /// <summary>
        /// 对账验证
        /// </summary>
        /// <param name="RevenueDate"></param>
        /// <param name="Channel_Id"></param>
        /// <returns></returns>
        public JsonResult ValidateReconciliation(string RevenueDate, string Channel_Id)
        {
            var resultModel = new ResultModel<string>() { IsSuccess = false, Status = "0", ResultInfo = "对账成功！" };
            v_Business_Reconciliation reconciliation = new v_Business_Reconciliation();

            DbBusinessDataService.Command(db =>
            {
                reconciliation = db.Queryable<v_Business_Reconciliation>()
                .Where(i => i.RevenueDate.Contains(RevenueDate) && i.Channel_Id == Channel_Id)
                .ToList().SingleOrDefault();
            });
            if (reconciliation != null)
            {
                resultModel.IsSuccess = true;
                resultModel.Status = "2";
            }
            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取未对账数据
        /// </summary>
        /// <returns></returns>
        private List<v_Business_Reconciliation> GetAutomaticReconciliationData()
        {
            List<v_Business_Reconciliation> business_Reconciliations = new List<v_Business_Reconciliation>();
            var start = DateTime.Now.AddYears(-1);
            var end = DateTime.Now;
            DbBusinessDataService.Command(db =>
            {
                var outputResult = db.Queryable<v_Business_Reconciliation>();
                outputResult.Where(i => i.BankBillDate >= DateTime.Now.AddYears(-1) && i.BankBillDate <= DateTime.Now);
                outputResult.Where(i => i.Status == "1" || i.Status == "3");
                outputResult.OrderBy(c => c.BankBillDate, OrderByType.Desc);
                business_Reconciliations = outputResult.ToList();
            });
            return business_Reconciliations;
        }

    }
}