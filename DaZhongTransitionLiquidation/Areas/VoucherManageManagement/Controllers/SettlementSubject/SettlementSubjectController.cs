using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.SettlementSubject
{
    public class SettlementSubjectController : BaseController
    {
        public SettlementSubjectController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/SettlementSubject
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetSettlementSubject()
        {
            var jsonResult = new JsonResultModel<Business_SettlementSubject>();
            DbBusinessDataService.Command(db =>
            {
                jsonResult.Rows = db.Queryable<Business_SettlementSubject>().ToList();
            });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSettingTable(string settlementVGUID)
        {
            var result = new List<Business_SettlementSubjectDetail>();
            DbBusinessDataService.Command(db =>
            {
                result = db.Queryable<Business_SettlementSubjectDetail>().Where(x => x.SettlementVGUID == settlementVGUID).ToList();
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}