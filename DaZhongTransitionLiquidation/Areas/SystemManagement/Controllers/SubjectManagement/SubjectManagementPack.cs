using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.SubjectManagement
{
    public class SubjectManagementPack
    {
        public bool IsExistChannel(SqlSugarClient db, T_Channel_Subject channel, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<T_Channel_Subject>().Any(i => i.SubjectNmae == channel.SubjectNmae && i.Vguid != channel.Vguid);
            }
            return db.Queryable<T_Channel_Subject>().Any(i => i.SubjectNmae == channel.SubjectNmae);
        }

        public bool IsExistChannelid(SqlSugarClient db, T_Channel_Subject channel, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<T_Channel_Subject>().Any(i => i.SubjectId == channel.SubjectId && i.Vguid != channel.Vguid);
            }
            return db.Queryable<T_Channel_Subject>().Any(i => i.SubjectId == channel.SubjectId);
        }

    }
}