using System;
using System.Collections.Generic;
using DaZhongTransitionLiquidation.Common;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.SystemManagement.Controllers.UserManagement
{
    public class UserManagementPack
    {

        /// <summary>
        /// 将查询条件封装成ConditionalModel
        /// </summary>
        /// <returns></returns>
        public List<ConditionalModel> GetConditionalModels(Sys_User searchParams)
        {
            List<ConditionalModel> conditionalModels = new List<ConditionalModel>();
            var dicParams = Global.EntityToDictionary(searchParams);
            foreach (var dicParam in dicParams)
            {
                if (dicParam.Value != "null" && dicParam.Value != Guid.Empty.ToString())
                {
                    conditionalModels.Add(new ConditionalModel()
                    {
                        ConditionalType = ConditionalType.Like,
                        FieldName = dicParam.Key,
                        FieldValue = dicParam.Value
                    });
                }
            }
            //过滤掉系统管理员
            conditionalModels.Add(new ConditionalModel()
            {
                ConditionalType = ConditionalType.NoEqual,
                FieldName = "LoginName",
                FieldValue = "admin"
            });
            return conditionalModels;
        }

        /// <summary>
        /// 判断登录名是否存在
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userInfo"></param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        public bool IsExistLoginName(SqlSugarClient db, Sys_User userInfo, bool isEdit)
        {
            if (isEdit)
            {
                return db.Queryable<Sys_User>().Any(i => i.LoginName == userInfo.LoginName && i.Vguid != userInfo.Vguid);
            }
            return db.Queryable<Sys_User>().Any(i => i.LoginName == userInfo.LoginName);

        }
    }
}