﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class CreateNo
    {
        public static string GetCreateNo(SqlSugarClient db, string bank)
        {
            var No = db.Ado.SqlQuery<string>(@"declare @output varchar(50) exec getautono '" + bank + "', @output output  select @output").FirstOrDefault(); ;
            return No;
        }
    }
}