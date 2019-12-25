using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.EliminationTaxCount
{
    public class EliminationTaxCountController : BaseController
    {
        public EliminationTaxCountController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {
        }
        // GET: VoucherManageManagement/EliminationTaxCount
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GeVehicleData(Business_VehicleUnitList searchParams, GridParams para)
        {
            var response = new List<Business_VehicleUnitList>();
            DbBusinessDataService.Command(db =>
            {
                var yearMonth = "";
                if (searchParams.YearMonth != null)
                {
                    yearMonth = searchParams.YearMonth.Replace("-", "");
                }
                //查询最新的税率设置(除本部的另外4家)
                var year = "";
                var month = "";
                var taxesInfo = db.Queryable<Business_TaxesInfo>().OrderBy("cast(Year as int) desc,cast(Month as int) desc").ToList();
                if (taxesInfo.Count > 0)
                {
                    year = taxesInfo[0].Year;
                    month = taxesInfo[0].Month;
                }
                var v_TaxesInfo = db.Ado.SqlQuery<v_TaxesInfo>(@"select a.Code,a.ParentCode,a.Descrption,b.TaxesType,b.TaxRate,a.VGUID as KeyVGUID,b.VGUID,a.AccountModeCode,a.CompanyCode,c.Abbreviation from Business_SevenSection as a
                            left join Business_TaxesInfo as b on a.VGUID = b.SubjectVGUID and b.Year=@Year and b.Month=@Month and a.AccountModeCode=b.AccountModeCode and a.CompanyCode =b.CompanyCode and a.SectionVGUID = 'B63BD715-C27D-4C47-AB66-550309794D43'
                            left join Business_SevenSection as c on c.AccountModeCode=b.AccountModeCode and c.Code =b.CompanyCode and c.SectionVGUID = 'A63BD715-C27D-4C47-AB66-550309794D43' 
                            where a.AccountModeCode!='1002' and (a.Code like '%6403%' or a.Code like '%2221%') and a.Status='1' and TaxRate is not null order by Code", 
                            new { Year = year, Month = month }).ToList();

                //查询车辆平均天数
                var vehicleData = db.SqlQueryable<Business_VehicleUnitList>(@"select t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.BELONGTO_COMPANY,t.YearMonth,SUM(MODEL_DAYS) as MODEL_DAYS from (
                             select g.* from (
                             select a.VGUID,a.ORIGINALID,a.YearMonth,a.PLATE_NUMBER,
                             m.BusinessName1 as MODEL_MAJOR, 
                             m.BusinessName2 as MODEL_MINOR,
                             --a.MODEL_MINOR,
                             CAST(CAST(a.MODEL_DAYS AS decimal(18,2))/30 as decimal(18,2)) as MODEL_DAYS
                            ,b.MANAGEMENT_COMPANY,
                            b.BELONGTO_COMPANY,b.DESCRIPTION as CarType,b.GROUP_ID,b.OPERATING_STATE from Business_VehicleList as a 
                            left join Business_AssetMaintenanceInfo as b on a.PLATE_NUMBER = b.PLATE_NUMBER 
                            left join (select a.BusinessName as BusinessName1,b.BusinessName as BusinessName2,c.BusinessName as BusinessName3,c.VehicleAge from Business_ManageModel as a
							left join Business_ManageModel as b on a.VGUID = b.ParentVGUID
							left join Business_ManageModel as c on b.VGUID = c.ParentVGUID
							where c.BusinessName is not null and c.VehicleAge is null
							) as m on a.MODEL_MINOR = m.BusinessName3 
							UNION ALL
							    select a.VGUID,a.ORIGINALID,a.YearMonth,a.PLATE_NUMBER,
                             m.BusinessName1 as MODEL_MAJOR, 
                             m.BusinessName2 as MODEL_MINOR,
                             CAST(CAST(a.MODEL_DAYS AS decimal(18,2))/30 as decimal(18,2)) as MODEL_DAYS
                            ,b.MANAGEMENT_COMPANY,
                            b.BELONGTO_COMPANY,b.DESCRIPTION as CarType,b.GROUP_ID,b.OPERATING_STATE from Business_VehicleList as a 
                            left join Business_AssetMaintenanceInfo as b on a.PLATE_NUMBER = b.PLATE_NUMBER 
                            left join (select a.BusinessName as BusinessName1,b.BusinessName as BusinessName2,c.BusinessName as BusinessName3,c.VehicleAge from Business_ManageModel as a
							left join Business_ManageModel as b on a.VGUID = b.ParentVGUID
							left join Business_ManageModel as c on b.VGUID = c.ParentVGUID
							where c.BusinessName is not null and c.VehicleAge is not null
							) as m on a.MODEL_MINOR = m.BusinessName3  and b.VEHICLE_AGE = m.VehicleAge) as g
                            where g.GROUP_ID='出租车' and g.OPERATING_STATE='在运' and g.MODEL_MAJOR is not null and g.YearMonth=@YearMonth and g.BELONGTO_COMPANY != '财务共享-大众出租') as t 
                            group by t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.BELONGTO_COMPANY,t.YearMonth ")
                .WhereIF(searchParams.YearMonth != null, i => i.YearMonth == yearMonth)
                .OrderBy("MODEL_MAJOR asc,MODEL_MINOR asc,CarType asc").ToList();
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