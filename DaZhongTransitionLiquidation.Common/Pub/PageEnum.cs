namespace DaZhongTransitionLiquidation.Common.Pub
{
    public enum PageEnum
    {
        /// <summary>
        /// 营收数据
        /// </summary>
        RevenuePayment = 1,
        /// <summary>
        /// T+1数据
        /// </summary>
        NextDayData = 2,

        /// <summary>
        /// 金额对账报表
        /// </summary>
        ReconciliationReport = 3,

        /// <summary>
        /// 异常数据报表
        /// </summary>
        ExceptionDataReport = 4,
        /// <summary>
        /// 用户管理
        /// </summary>
        UserManagement = 5,

        /// <summary>
        /// 角色权限管理
        /// </summary>
        AuthorityManagement = 6,

        /// <summary>
        /// 渠道管理
        /// </summary>
        ChannelManagement = 7,

        /// <summary>
        /// 银行数据
        /// </summary>
        BankData = 8,
        /// <summary>
        /// 渠道科目管理
        /// </summary>
        SubjectManagement = 10,
        /// <summary>
        /// 银行渠道映射
        /// </summary>
        BankChannelMapping = 11
    }
}