using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Controllers.OrderListDraftPrint
{
    public class OrderListDraftPrintController : BaseController
    {
        public OrderListDraftPrintController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: CapitalCenterManagement/OrderListDraftPrint
        public ActionResult Index(Guid VGUID)
        {
            ViewBag.VGUID = VGUID;
            //ViewBag.CurrentModulePermission = GetRoleModuleInfo(MasterVGUID.BankData);
            return View();
        }
        public JsonResult GetOrderListDraftPrint(Guid vguid)
        {
            Business_OrderListDraft orderList = new Business_OrderListDraft();
            DbBusinessDataService.Command(db =>
            {
                //主信息
                orderList = db.Queryable<Business_OrderListDraft>().Single(x => x.VGUID == vguid);
            });
            return Json(orderList, JsonRequestBehavior.AllowGet); ;
        }
    }
}