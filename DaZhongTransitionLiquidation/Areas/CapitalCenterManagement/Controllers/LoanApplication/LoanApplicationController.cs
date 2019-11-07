using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.LoanApplication
{
    public class LoanApplicationController : BaseController
    {
        public LoanApplicationController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/LoanApplication
        public ActionResult Index()
        {
            ViewBag.SysUser = CacheManager<Sys_User>.GetInstance()[PubGet.GetUserKey];
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("db4ac4f3-2f54-489d-8baa-c0957cda38f4");
            return View();
        }
        public JsonResult GetLoanApplicationData(Business_LoanApplication searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_LoanApplication>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_LoanApplication>()
                .WhereIF(searchParams.OrgId != null, i => i.OrgId == searchParams.OrgId)
                .WhereIF(searchParams.ApplyDate != null, i => i.ApplyDate == searchParams.ApplyDate)
                .OrderBy(i => i.No, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
    }
}