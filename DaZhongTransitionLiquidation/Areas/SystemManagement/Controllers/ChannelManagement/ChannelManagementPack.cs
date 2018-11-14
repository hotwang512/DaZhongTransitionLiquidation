using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.ChannelManagement
{
    public class ChannelManagementPack
    {
        /// <summary>
        /// 渠道名是否存在
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="channel">渠道信息</param>
        /// <param name="isEdit">是否编辑</param>
        /// <returns></returns>
        public bool IsExistChannel(SqlSugarClient db, T_Channel channel, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<T_Channel>().Any(i => i.Name == channel.Name && i.Vguid != channel.Vguid);
            }
            return db.Queryable<T_Channel>().Any(i => i.Name == channel.Name);
        }
    }
}