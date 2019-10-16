using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VehicleCount
{
    public class VehicleCountController : BaseController
    {
        public VehicleCountController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/VehicleCount
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GeVehicleData(Business_SettlementCount searchParams, GridParams para)
        {
            var response = new List<Business_SettlementCount>();
            DbBusinessDataService.Command(db =>
            {
                //int pageCount = 0;
                //para.pagenum = para.pagenum + 1;
                var yearMonth = "";
                if (searchParams.YearMonth != null)
                {
                    yearMonth = searchParams.YearMonth.Replace("-", "");
                }
                //response = db.SqlQueryable<Business_SettlementCount>(@"select c.Model,c.ClassType, c.CarType,c.BusinessType ,a.YearMonth,b.MANAGEMENT_COMPANY,b.BELONGTO_COMPANY,CAST(CAST(a.MODEL_DAYS AS decimal(18,2))/30 as decimal(18,2)) as DAYS,
                //            c.Money,(CAST(CAST(a.MODEL_DAYS AS decimal(18,2))/30 as decimal(18,2))*c.Money) as Account,c.MoneyRow,c.MoneyColumns from Business_VehicleList as a
                //            left join Business_AssetMaintenanceInfo as b on a.PLATE_NUMBER = b.PLATE_NUMBER
                //            left join Business_SettlementImport as c on c.Model=b.MODEL_MAJOR and c.ClassType=b.MODEL_MINOR and
                //            c.CarType = b.DESCRIPTION where b.OPERATING_STATE='在运' and b.GROUP_ID='出租车' and c.Model is not null ")
                response = db.Ado.SqlQuery<Business_SettlementCount>(@"select x.MODEL_MAJOR as Model,x.MODEL_MINOR as ClassType,x.CarType,x.YearMonth,x.MANAGEMENT_COMPANY,x.MODEL_DAYS as DAYS,c.Money,CAST((x.MODEL_DAYS*c.Money) AS decimal(18,2)) as Account,c.BusinessType,c.MoneyRow,c.MoneyColumns from (                           
                            select t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.MANAGEMENT_COMPANY,t.YearMonth,SUM(MODEL_DAYS) as MODEL_DAYS from (select a.VGUID,a.ORIGINALID,a.YearMonth,a.PLATE_NUMBER,b.MODEL_MAJOR, b.MODEL_MINOR,CAST(CAST(a.MODEL_DAYS AS decimal(18,2))/30 as decimal(18,2)) as MODEL_DAYS
                            ,b.MANAGEMENT_COMPANY,b.BELONGTO_COMPANY,b.DESCRIPTION as CarType from Business_VehicleList as a left join Business_AssetMaintenanceInfo
                            as b on a.PLATE_NUMBER = b.PLATE_NUMBER where b.OPERATING_STATE='在运' and b.GROUP_ID='出租车' and a.YearMonth=@YearMonth
                            ) as t where  t.YearMonth=@YearMonth group by t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.MANAGEMENT_COMPANY,t.YearMonth ) as x  
                            left join Business_SettlementImport as c on x.MODEL_MAJOR = c.Model and x.MODEL_MINOR = parsename(REPLACE(c.ClassType,'-','.'),1)
                            and x.CarType = c.CarType where parsename(REPLACE(c.BusinessType,'-','.'),1) != '小计'  and x.YearMonth=@YearMonth ", new { YearMonth = yearMonth })
                .OrderBy(x=>x.MoneyRow).ToList();
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