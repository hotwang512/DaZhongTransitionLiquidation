using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;
using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.CompanySection;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.CapitalAllocation
{
    public class CapitalAllocationController : BaseController
    {
        public CapitalAllocationController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/CapitalAllocation
        public ActionResult Index()
        {
            ViewBag.CurrentModulePermission = GetRoleModuleInfo("40352c56-d1c5-491e-9a99-74f5eff649ef");
            return View();
        }
        public JsonResult GetCapitalAllocationData(Business_CapitalAllocationInfo searchParams, GridParams para)
        {
            var jsonResult = new JsonResultModel<Business_CapitalAllocationInfo>();
            DbBusinessDataService.Command(db =>
            {
                int pageCount = 0;
                para.pagenum = para.pagenum + 1;
                jsonResult.Rows = db.Queryable<Business_CapitalAllocationInfo>()
                .WhereIF(searchParams.TurnInBankAccount != null, i => i.TurnInBankAccount == searchParams.TurnInBankAccount)
                .WhereIF(searchParams.TurnOutBankAccount != null, i => i.TurnOutBankAccount == searchParams.TurnOutBankAccount)
                .WhereIF(searchParams.ApplyDate != null, i => i.ApplyDate == searchParams.ApplyDate)
                .OrderBy(i => i.No, OrderByType.Desc).ToPageList(para.pagenum, para.pagesize, ref pageCount);
                jsonResult.TotalRows = pageCount;
                var data = db.Queryable<Business_CompanyBankInfo>().ToList();
                foreach (var item in jsonResult.Rows)
                {
                    item.TurnInBankName = data.Single(x => x.AccountModeCode == item.TurnInAccountModeCode && x.CompanyCode == item.TurnInCompanyCode && x.BankAccount == item.TurnInBankAccount).BankName;
                    item.TurnOutBankName = data.Single(x => x.AccountModeCode == item.TurnOutAccountModeCode && x.CompanyCode == item.TurnOutCompanyCode && x.BankAccount == item.TurnOutBankAccount).BankName;
                }
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBankInfo()
        {
            var result = new List<Business_CompanyBankInfo>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_CompanyBankInfo>().OrderBy("BankAccount asc").ToList();
                foreach (var item in result)
                {
                    item.BankName = item.BankName + "-" + item.BankAccount.Substring(item.BankAccount.Length - 4, 4);
                }
            });
            return Json(result, JsonRequestBehavior.AllowGet); ;
        }
    }
}