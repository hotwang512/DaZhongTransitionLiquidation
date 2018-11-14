using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.StoredProcedureEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using DaZhongTransitionLiquidation.Infrastructure.ViewEntity;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.RevenuePayment
{
    public class RevenuePaymentController : BaseController
    {
        // GET: PaymentManagement/RevenuePayment
        public RevenuePaymentController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }

        public ActionResult Index()
        {
            ViewData["transactionId"] = Request["transactionId"] ?? "";
            ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.RevenuePayment);
            return View();
        }

        /// <summary>
        /// 获取营收支付信息
        /// </summary>
        /// <param name="searchParas">搜索条件</param>
        /// <param name="para">表格分页条件</param>
        /// <returns></returns>
        public JsonResult GetRevenuePaymentInfos(U_RevenuePayment_Search searchParas, GridParams para)
        {
            var jsonResult = new JsonResultModel<V_Revenuepayment_Information>();
            var start = !string.IsNullOrEmpty(searchParas.PayDateFrom) ? DateTime.Parse(searchParas.PayDateFrom + " 00:00:00") : DateTime.Parse("1900-01-01");
            var end = !string.IsNullOrEmpty(searchParas.PayDateTo) ? DateTime.Parse(searchParas.PayDateTo + " 23:59:59") : DateTime.MaxValue;
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                List<V_Revenuepayment_Information> revenuepayments = db.Queryable<V_Revenuepayment_Information>()
               .WhereIF(!string.IsNullOrEmpty(searchParas.Name), i => i.Name.Contains(searchParas.Name))
               .WhereIF(!string.IsNullOrEmpty(searchParas.JobNumber), i => i.JobNumber.Contains(searchParas.JobNumber))
               .WhereIF(!string.IsNullOrEmpty(searchParas.TransactionID), i => i.TransactionID.Contains(searchParas.TransactionID))
               .Where(i => SqlFunc.Between(i.PayDate, start, end))
               .OrderBy(i => i.PayDate, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);

                foreach (var revenuepayment in revenuepayments)
                {
                    revenuepayment.DriverBearFees = revenuepayment.ActualAmount - revenuepayment.PaymentAmount;
                    revenuepayment.CompanyBearsFees = revenuepayment.copeFee - revenuepayment.DriverBearFees;
                    revenuepayment.ChannelPayableAmount = revenuepayment.ActualAmount - revenuepayment.copeFee;
                }

                jsonResult.Rows = revenuepayments;
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }



    }
}