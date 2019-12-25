using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
using DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Model;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SqlSugar;
using SyntacticSugar;
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
            var response = new List<v_Business_VehicleUnitList>();
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
                var v_TaxesInfo = db.Ado.SqlQuery<v_TaxesInfo>(@"select b.TaxesType,b.TaxRate,b.TaxRateDec,b.ParentVGUID,b.VGUID,b.AccountModeCode,b.CompanyCode,c.Abbreviation from Business_TaxesInfo as b
                            left join Business_SevenSection as c on c.AccountModeCode=b.AccountModeCode and c.Code =b.CompanyCode and c.SectionVGUID = 'A63BD715-C27D-4C47-AB66-550309794D43' 
                            where b.AccountModeCode!='1002' and b.Year=@Year and b.Month=@Month and c.Status='1' and TaxRate is not null", 
                            new { Year = year, Month = month }).ToList();

                //查询车辆平均天数
                List<Business_VehicleUnitList> vehicleData = new List<Business_VehicleUnitList>();
                vehicleData = GetVehicleUnitList(db,yearMonth);
                //构造托管单位增值税附加税冲销数据
                List<string> taxTypeList = new List<string>() { "增值税税金", "城建税税金", "教育费附加", "地方教育费附加" };
                foreach (var item in vehicleData)
                {
                    double twoShifts = 0;
                    double oneShifts = 0;
                    double cityTax = 0;
                    double EduTax = 0;
                    double EduTaxOther = 0;
                    var twoShiftsList = v_TaxesInfo.Where(x => x.Abbreviation == item.BELONGTO_COMPANY && x.TaxesType == "定额增值税-双班").FirstOrDefault();
                    var oneShiftsList = v_TaxesInfo.Where(x => x.Abbreviation == item.BELONGTO_COMPANY && x.TaxesType == "定额增值税-单班").FirstOrDefault();
                    var cityTaxList = v_TaxesInfo.Where(x => x.Abbreviation == item.BELONGTO_COMPANY && x.TaxesType == "城建税").FirstOrDefault();
                    var EduTaxList = v_TaxesInfo.Where(x => x.Abbreviation == item.BELONGTO_COMPANY && x.TaxesType == "教育费附加").FirstOrDefault();
                    var EduTaxOtherList = v_TaxesInfo.Where(x => x.Abbreviation == item.BELONGTO_COMPANY && x.TaxesType == "地方教育费附加").FirstOrDefault();
                    if (twoShiftsList != null && oneShiftsList != null && cityTaxList != null && EduTaxList != null && EduTaxOtherList != null)
                    {
                        twoShifts = twoShiftsList.TaxRateDec;//增值税税率(承包模式双班车,255)
                        oneShifts = oneShiftsList.TaxRateDec;//增值税税率(通用,192)
                        cityTax = cityTaxList.TaxRateDec;//城建税税率
                        EduTax = EduTaxList.TaxRateDec;//教育费附加税率
                        EduTaxOther = EduTaxOtherList.TaxRateDec;//地方教育费附加税率
                    }
                    List<double> taxRateList = new List<double>() { 0, cityTax, EduTax, EduTaxOther };
                    decimal VAT = 0; //增值税所求值
                    for (int i = 0; i < 4; i++)
                    {
                        v_Business_VehicleUnitList vehicle = new v_Business_VehicleUnitList();
                        vehicle.MODEL_MAJOR = item.MODEL_MAJOR;
                        vehicle.MODEL_MINOR = item.MODEL_MINOR;
                        vehicle.CarType = item.CarType;
                        vehicle.BELONGTO_COMPANY = item.BELONGTO_COMPANY.Split("-")[1];
                        vehicle.TaxType = taxTypeList[i];
                        if(i == 0)
                        {
                            //增值税
                            if (item.MODEL_MAJOR == "承包模式(重资产重人员)" && item.MODEL_MINOR == "双班车")
                            {
                                vehicle.MODEL_DAYS = item.MODEL_DAYS * twoShifts.ObjToDecimal() * -1;
                            }
                            else
                            {
                                vehicle.MODEL_DAYS = item.MODEL_DAYS * oneShifts.ObjToDecimal() * -1;
                            }
                            VAT = vehicle.MODEL_DAYS;
                        }
                        else
                        {
                            vehicle.MODEL_DAYS = VAT * taxRateList[i].ObjToDecimal();
                        }
                        response.Add(vehicle);
                    }
                }
            });
            return Json(
                 response,
                 "application/json",
                 Encoding.UTF8,
                 JsonRequestBehavior.AllowGet
             );
        }
        private List<Business_VehicleUnitList> GetVehicleUnitList(SqlSugarClient db,string yearMonth)
        {
           var vehicleData = db.Ado.SqlQuery<Business_VehicleUnitList>(@"select t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.BELONGTO_COMPANY,t.YearMonth,SUM(MODEL_DAYS) as MODEL_DAYS from (
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
                            group by t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.BELONGTO_COMPANY,t.YearMonth ", new { YearMonth = yearMonth }).ToList();
            return vehicleData;
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