using System;

namespace DaZhongTransitionLiquidation.Common.Pub
{
    public class MasterVGUID
    {
        /// <summary>
        /// 系统管理员的主键
        /// </summary>
        public const string AdminVguid = "47348F53-1A23-4DCF-8C9F-A2614FA0AD91";

        /// <summary>
        /// 系统层级
        /// </summary>
        public const string LevelVguid = "A60C76C5-3F47-4A33-91AB-22BB2D5815BA";

        #region 系统管理模块
        /// <summary>
        /// 渠道管理
        /// </summary>
        public const string ChannelManagement = "106B9043-FAB2-4893-9505-E8D01C5615E0";

        /// <summary>
        /// 用户管理
        /// </summary>
        public const string UserManagement = "A915FEE2-EE09-46EA-B710-85D0C6701ACD";

        /// <summary>
        /// 角色权限模块
        /// </summary>
        public const string AuthorityManagement = "5470788D-E80B-4EFD-BA48-A4B38B9433E6";

        /// <summary>
        /// 渠道科目
        /// </summary>
        public const string SubjectManagement = "1234";
        #endregion

        #region 基础数据模块

        /// <summary>
        /// 营收数据
        /// </summary>
        public const string RevenuePayment = "FB73B118-8FE7-49B0-8E54-93549EF78DC1";

        /// <summary>
        /// T+1数据
        /// </summary>
        public const string NextDayData = "4A51CDEE-1E95-418B-94C1-4BEC16BC7DAE";

        /// <summary>
        /// 银行数据
        /// </summary>
        public const string BankData = "ED966D2E-2C71-4F10-AC97-2BE9021F42AB";
        #endregion


        #region 报表模块
        /// <summary>
        /// 金额对账报表
        /// </summary>
        public const string ReconciliationReport = "2EA44D4A-3C25-4ADB-8E61-81E580671A4A";

        /// <summary>
        /// 异常数据报表
        /// </summary>
        public const string ExceptionDataReport = "9D636941-C3FF-4EFC-A7EE-A7B7D5AE5E17";

        #endregion
    }
}