using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.SettlementImportCount
{
    public class SettlementImportCountController : BaseController
    {
        public SettlementImportCountController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/SettlementImportCount
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetSettlementData(Business_SettlementImport searchParams, GridParams para)
        {
            var response = new List<Business_SettlementImport>();
            DbBusinessDataService.Command(db =>
            {
                var responseList = db.Ado.SqlQuery<Business_SettlementImport>(@"select t.*,'小计' as BusinessType,99 as MoneyRow,99 as MoneyColumns from (
                        select Model,ClassType,CarType,parsename(REPLACE(BusinessType,'-','.'),2) as Business,SUM(Money) as Money  from Business_SettlementImport  
                        where parsename(REPLACE(BusinessType,'-','.'),2) is not null
                        group by Model,ClassType,CarType,parsename(REPLACE(BusinessType,'-','.'),2))t
                        UNION ALL
                        select Model,ClassType,CarType,CASE WHEN parsename(REPLACE(BusinessType,'-','.'),2) is null THEN '' ELSE parsename(REPLACE(BusinessType,'-','.'),2) END as Business,
                        Money,parsename(REPLACE(BusinessType,'-','.'),1) as BusinessType,
                        MoneyRow,MoneyColumns from Business_SettlementImport where parsename(REPLACE(BusinessType,'-','.'),1) != '小计'").ToList();
                response = responseList.OrderBy(x => x.MoneyRow).OrderBy(x => x.MoneyColumns).ToList();
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}