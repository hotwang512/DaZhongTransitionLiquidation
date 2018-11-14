using DaZhongTransitionLiquidation.Common;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.Dao
{
   public class DbBusinessDataService : IDisposable
    {
        private readonly SqlSugarClient _db;

        public DbBusinessDataService()
        {
            _db = DbBusinessDataConfig.GetInstance();
        }

        /// <summary>
        /// 服务命令
        /// </summary>
        /// <param name="func"></param>
        public void Command(Action<SqlSugarClient> func)
        {
            try
            {
                func(_db);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// 服务命令
        /// </summary>
        /// <typeparam name="T">外包对象</typeparam>
        /// <param name="func"></param>
        public void Command<T>(Action<SqlSugarClient, T> func) where T : class, new()
        {
            var t = new T();
            try
            {
                func(_db, t);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.ToString());
                throw;
            }
            finally
            {
                t = null;  //释放对象
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
