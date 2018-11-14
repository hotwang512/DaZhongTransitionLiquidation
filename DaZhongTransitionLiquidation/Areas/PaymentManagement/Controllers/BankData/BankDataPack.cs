using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.BankData
{
    public class BankDataPack
    {
        /// <summary>
        /// 银行数据名是否存在
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="bank">银行数据</param>
        /// <param name="isEdit">是否编辑</param>
        /// <returns></returns>
        public bool IsExistBankData(SqlSugarClient db, T_Bank bank, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<T_Bank>().Any(i => i.ArrivedTime == bank.ArrivedTime && i.VGUID != bank.VGUID);
            }
            return db.Queryable<T_Bank>().Any(i => i.ArrivedTime == bank.ArrivedTime);
        }
    }
}