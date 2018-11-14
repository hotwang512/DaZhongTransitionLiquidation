using System;
using System.Collections.Generic;
using SqlSugar;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Infrastructure.Dao
{
    public class DbConfig
    {
        public static SqlSugarClient GetInstance()
        {
            try
            {
                var expMethods = new List<SqlFuncExternal>();
                expMethods.Add(new SqlFuncExternal()
                {
                    UniqueMethodName = "GetVguid",
                    MethodValue = (expInfo, dbType, expContext) =>
                    {
                        if (dbType == DbType.SqlServer)
                            return "newID()";
                        else
                            throw new Exception("未实现");
                    }
                });
                return new SqlSugarClient(new ConnectionConfig
                {
                    ConnectionString = ConfigSugar.GetConnectionString("sqlConnStr"),
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    ConfigureExternalServices = new ConfigureExternalServices
                    {
                        SqlFuncServices = expMethods//set ext method
                    }

                });
            }
            catch (Exception ex)
            {
                throw new Exception("连接数据库出错，请检查您的连接字符串，和网络。 ex:".AppendString(ex.Message));
            }
        }

        /// <summary>
        /// sqlFunc的扩展方法
        /// 方法名定义在DbConfig中
        /// </summary>
        /// <returns></returns>
        public static Guid GetVguid()
        {
            throw new NotSupportedException("Can only be used in expressions");
        }

    }
}