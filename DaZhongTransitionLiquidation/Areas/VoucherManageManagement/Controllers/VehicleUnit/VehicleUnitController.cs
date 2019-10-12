using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VehicleUnit
{
    public class VehicleUnitController : BaseController
    {
        public VehicleUnitController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/VehicleUnit
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GeVehicleData(Business_VehicleUnitList searchParams, GridParams para)
        {
            var response = new List<Business_VehicleUnitList>();
            DbBusinessDataService.Command(db =>
            {
                //int pageCount = 0;
                //para.pagenum = para.pagenum + 1;
                var yearMonth = "";
                if (searchParams.YearMonth != null)
                {
                    yearMonth = searchParams.YearMonth.Replace("-", "");
                }
                response = db.SqlQueryable<Business_VehicleUnitList>(@"select a.VGUID,a.ORIGINALID,a.YearMonth,a.PLATE_NUMBER,a.MODEL_MINOR,CAST(CAST(a.MODEL_DAYS AS decimal(18,2))/30 as decimal(18,2)) as MODEL_DAYS
                            , b.MANAGEMENT_COMPANY, b.BELONGTO_COMPANY,b.MODEL_MAJOR, b.DESCRIPTION as CarType from Business_VehicleList as a left join Business_AssetMaintenanceInfo
                            as b on a.PLATE_NUMBER = b.PLATE_NUMBER where b.OPERATING_STATE='在运' and b.GROUP_ID='出租车' ")
                .WhereIF(searchParams.YearMonth != null, i => i.YearMonth == yearMonth)
                .WhereIF(searchParams.PLATE_NUMBER != null, i => i.PLATE_NUMBER.Contains(searchParams.PLATE_NUMBER))
                .WhereIF(searchParams.MODEL_MINOR != null, i => i.MODEL_MINOR.Contains(searchParams.MODEL_MINOR))
                //.WhereIF(searchParams.MODEL_DAYS != null, i => i.MODEL_DAYS == searchParams.MODEL_DAYS)
                .OrderBy("BELONGTO_COMPANY asc,MODEL_MINOR asc").ToList();
                //jsonResult.TotalRows = pageCount;
            });
            return Json(
                 response,
                 "application/json",
                 Encoding.UTF8,
                 JsonRequestBehavior.AllowGet
             );
        }
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
    }
}