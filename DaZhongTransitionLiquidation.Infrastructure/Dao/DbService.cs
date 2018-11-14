using System;
using DaZhongTransitionLiquidation.Common;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Infrastructure.Dao
{
    public class DbService : IDisposable
    {
        private readonly SqlSugarClient _db;

        public DbService()
        {
            _db = DbConfig.GetInstance();
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