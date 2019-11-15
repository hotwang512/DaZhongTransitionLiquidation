using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;
using DaZhongTransitionLiquidation.Areas.CapitalCenterManagement.Model;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class CreateNo
    {
        public static string GetCreateNo(SqlSugarClient db, string autoID)
        {
            var isAny = db.Queryable<SMAUTO_1>().Any(x=>x.AID == autoID);
            if(!isAny)
            {
                SMAUTO_1 sm = new SMAUTO_1();
                sm.AID = autoID;
                sm.ADESC = "";
                sm.ADESCCHS = "";
                sm.APREFIX = autoID.Substring(autoID.Length - 6, 6);
                sm.ADATE = null;
                sm.ALENGTH = 4;
                sm.ANEXTNO = 1;
                sm.ALASTDATE = null;
                sm.VGUID = Guid.NewGuid();
                sm.VCRTTIME = DateTime.Now;
                sm.VCRTUSER = "admin";
                db.Insertable(sm).ExecuteCommand();
            }
            var No = db.Ado.SqlQuery<string>(@"declare @output varchar(50) exec getautono '" + autoID + "', @output output  select @output").FirstOrDefault(); ;
            return No;
        }

        internal static string GetCreateCashNo(SqlSugarClient db, string autoID)
        {
            var isAny = db.Queryable<SMAUTO_1>().Any(x => x.AID == autoID);
            if (!isAny)
            {
                SMAUTO_1 sm = new SMAUTO_1();
                sm.AID = autoID;
                sm.ADESC = "";
                sm.ADESCCHS = "";
                sm.APREFIX = "";
                sm.ADATE = "YYYYMM";
                sm.ALENGTH = 4;
                sm.ANEXTNO = 1;
                sm.ALASTDATE = DateTime.Now.ToString("YYYYMM");
                sm.VGUID = Guid.NewGuid();
                sm.VCRTTIME = DateTime.Now;
                sm.VCRTUSER = "admin";
                db.Insertable(sm).ExecuteCommand();
            }
            var No = db.Ado.SqlQuery<string>(@"declare @output varchar(50) exec getautono '" + autoID + "', @output output  select @output").FirstOrDefault(); ;
            return No;
        }
    }
}