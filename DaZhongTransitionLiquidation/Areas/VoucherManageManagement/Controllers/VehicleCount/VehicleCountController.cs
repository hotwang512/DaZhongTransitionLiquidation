﻿using DaZhongTransitionLiquidation.Areas.PaymentManagement.Models;
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
using SqlSugar;

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
        public JsonResult GetVehicleData(string YearMonth, string Type,GridParams para)
        {
            var response = new List<Business_SettlementCount>();
            DbBusinessDataService.Command(db =>
            {
                //int pageCount = 0;
                //para.pagenum = para.pagenum + 1;
                //b.OPERATING_STATE='在运' and 
                var company = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == UserInfo.AccountModeCode && x.Code == UserInfo.CompanyCode).ToList().FirstOrDefault().Abbreviation;
                var yearMonth = "";
                if (YearMonth != "" && YearMonth != null)
                {
                    yearMonth = YearMonth.Replace("-", "");
                }
                if(Type == "S")
                {
                    if (UserInfo.AccountModeCode == "1002" && (UserInfo.CompanyCode == "02" || UserInfo.CompanyCode == "03" || UserInfo.CompanyCode == "04" || UserInfo.CompanyCode == "05"))
                    {
                        response = db.Queryable<Business_SettlementCount>().Where(x => x.YearMonth == yearMonth && x.MANAGEMENT_COMPANY == company).OrderBy("MoneyRow asc,MoneyColumns asc").ToList();
                    }
                    else
                    {
                        response = db.Queryable<Business_SettlementCount>().Where(x => x.YearMonth == yearMonth && x.BELONGTO_COMPANY == company).OrderBy("MoneyRow asc,MoneyColumns asc").ToList();
                    }       
                }
                else if(Type == "C")
                {
                    
                    if(UserInfo.AccountModeCode == "1002" && (UserInfo.CompanyCode == "02" || UserInfo.CompanyCode == "03" || UserInfo.CompanyCode == "04" || UserInfo.CompanyCode == "05"))
                    {
                        //按管理公司分类
                        response = GetSettlementMANAGEMENT(db,yearMonth, company);
                        if (response.Count > 0)
                        {
                            db.Deleteable<Business_SettlementCount>().Where(x => x.YearMonth == yearMonth && x.MANAGEMENT_COMPANY == company).ExecuteCommand();
                            db.Insertable(response).ExecuteCommand();
                        }
                    }
                    else
                    {
                        //按归属公司分类
                        response = GetSettlementBELONGTO(db, yearMonth, company);
                        if (response.Count > 0)
                        {
                            db.Deleteable<Business_SettlementCount>().Where(x => x.YearMonth == yearMonth && x.BELONGTO_COMPANY == company).ExecuteCommand();
                            db.Insertable(response).ExecuteCommand();
                        }
                    }
                }
                foreach (var item in response)
                {
                    if (item.BusinessType.Contains("-"))
                    {
                        item.Business = item.BusinessType.Split("-")[0];
                        item.BusinessType = item.BusinessType.Split("-")[1];
                    }
                    else
                    {
                        item.Business = item.BusinessType;
                    }
                }
                //jsonResult.TotalRows = pageCount;
            });
            return Json(
                 response,
                 "application/json",
                 Encoding.UTF8,
                 JsonRequestBehavior.AllowGet
             );
        }
        private List<Business_SettlementCount> GetSettlementMANAGEMENT(SqlSugarClient db, string yearMonth, string company)
        {
            var response = db.Ado.SqlQuery<Business_SettlementCount>(@"select newid() as VGUID,x.MODEL_MAJOR as Model,x.MODEL_MINOR as ClassType,x.CarType,x.YearMonth,x.MANAGEMENT_COMPANY,x.MODEL_DAYS as DAYS,c.Money,CAST((CAST(x.MODEL_DAYS AS int)*c.Money) AS decimal(18,2)) as Account,c.BusinessType,c.MoneyRow,c.MoneyColumns from (                           
                            select t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.MANAGEMENT_COMPANY,t.YearMonth,SUM(MODEL_DAYS) as MODEL_DAYS from (
                            select a.VGUID,a.ORIGINALID,a.YearMonth,a.PLATE_NUMBER,m.BusinessName1 as MODEL_MAJOR, 
                            m.BusinessName2 as MODEL_MINOR,CAST(CAST(a.MODEL_DAYS AS decimal(18,2))/30 as decimal(18,2)) as MODEL_DAYS
                            ,b.MANAGEMENT_COMPANY,b.BELONGTO_COMPANY,b.DESCRIPTION as CarType from Business_VehicleList as a 
                            left join Business_AssetMaintenanceInfo as b on a.PLATE_NUMBER = b.PLATE_NUMBER 
                            left join (select a.BusinessName as BusinessName1,b.BusinessName as BusinessName2,c.BusinessName as BusinessName3 from Business_ManageModel as a
							            left join Business_ManageModel as b on a.VGUID = b.ParentVGUID
							            left join Business_ManageModel as c on b.VGUID = c.ParentVGUID
							            where c.BusinessName is not null
							            group by a.BusinessName,b.BusinessName,c.BusinessName) as m on a.MODEL_MINOR = m.BusinessName3
                            where b.GROUP_ID='出租车' and b.MANAGEMENT_COMPANY=@COMPANY ) as t 
                            group by t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.MANAGEMENT_COMPANY,t.YearMonth ) as x  
                            left join Business_SettlementImport as c on x.MODEL_MAJOR = c.Model and x.MODEL_MINOR = parsename(REPLACE(c.ClassType,'-','.'),1)
                            and x.CarType = c.CarType where parsename(REPLACE(c.BusinessType,'-','.'),1) != '小计'  and x.YearMonth=@YearMonth  order by c.MoneyRow asc,c.MoneyColumns asc",
                            new { YearMonth = yearMonth, COMPANY = company }).ToList();
            return response;
        }
        private List<Business_SettlementCount> GetSettlementBELONGTO(SqlSugarClient db,string yearMonth, string company)
        {
            var response = db.Ado.SqlQuery<Business_SettlementCount>(@"select newid() as VGUID,x.MODEL_MAJOR as Model,x.MODEL_MINOR as ClassType,x.CarType,x.YearMonth,x.BELONGTO_COMPANY,x.MODEL_DAYS as DAYS,c.Money,CAST((CAST(x.MODEL_DAYS AS int)*c.Money) AS decimal(18,2)) as Account,c.BusinessType,c.MoneyRow,c.MoneyColumns from (                           
                            select t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.BELONGTO_COMPANY,t.YearMonth,SUM(MODEL_DAYS) as MODEL_DAYS from (
                            select a.VGUID,a.ORIGINALID,a.YearMonth,a.PLATE_NUMBER,m.BusinessName1 as MODEL_MAJOR, 
                            m.BusinessName2 as MODEL_MINOR,CAST(CAST(a.MODEL_DAYS AS decimal(18,2))/30 as decimal(18,2)) as MODEL_DAYS
                            ,b.MANAGEMENT_COMPANY,b.BELONGTO_COMPANY,b.DESCRIPTION as CarType from Business_VehicleList as a 
                            left join Business_AssetMaintenanceInfo as b on a.PLATE_NUMBER = b.PLATE_NUMBER 
                            left join (select a.BusinessName as BusinessName1,b.BusinessName as BusinessName2,c.BusinessName as BusinessName3 from Business_ManageModel as a
							            left join Business_ManageModel as b on a.VGUID = b.ParentVGUID
							            left join Business_ManageModel as c on b.VGUID = c.ParentVGUID
							            where c.BusinessName is not null
							            group by a.BusinessName,b.BusinessName,c.BusinessName) as m on a.MODEL_MINOR = m.BusinessName3
                            where b.GROUP_ID='出租车' and b.BELONGTO_COMPANY=@COMPANY ) as t 
                            group by t.MODEL_MAJOR,t.MODEL_MINOR,t.CarType,t.BELONGTO_COMPANY,t.YearMonth ) as x  
                            left join Business_SettlementImport as c on x.MODEL_MAJOR = c.Model and x.MODEL_MINOR = parsename(REPLACE(c.ClassType,'-','.'),1)
                            and x.CarType = c.CarType where parsename(REPLACE(c.BusinessType,'-','.'),1) != '小计'  and x.YearMonth=@YearMonth  order by c.MoneyRow asc,c.MoneyColumns asc",
                            new { YearMonth = yearMonth, COMPANY = company }).ToList();
            return response;
        }
        public JsonResult SelectVehicleData(string YearMonth)
        {
            var response = new List<Business_SettlementCount>();
            DbBusinessDataService.Command(db =>
            {
                var company = db.Queryable<Business_SevenSection>().Where(x => x.SectionVGUID == "A63BD715-C27D-4C47-AB66-550309794D43" && x.AccountModeCode == UserInfo.AccountModeCode && x.Code == UserInfo.CompanyCode).ToList().FirstOrDefault().Abbreviation;
                var yearMonth = "";
                if (YearMonth != "" && YearMonth != null)
                {
                    yearMonth = YearMonth.Replace("-", "");
                }
                if (UserInfo.AccountModeCode == "1002" && (UserInfo.CompanyCode == "02" || UserInfo.CompanyCode == "03" || UserInfo.CompanyCode == "04" || UserInfo.CompanyCode == "05"))
                {
                    response = db.Queryable<Business_SettlementCount>().Where(x => x.YearMonth == yearMonth && x.MANAGEMENT_COMPANY == company).OrderBy("MoneyRow asc,MoneyColumns asc").ToList();
                }
                else
                {
                    response = db.Queryable<Business_SettlementCount>().Where(x => x.YearMonth == yearMonth && x.BELONGTO_COMPANY == company).OrderBy("MoneyRow asc,MoneyColumns asc").ToList();
                }
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