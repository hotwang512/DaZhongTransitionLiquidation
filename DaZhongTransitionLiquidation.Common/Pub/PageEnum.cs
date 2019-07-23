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
    /// <summary>
    /// 固定资产订单提交状态
    /// </summary>
    public enum FixedAssetsSubmitStatusEnum
    {
        /// <summary>
        /// 待提交
        /// </summary>
        UnSubmit = 0,
        /// <summary>
        /// 支付中
        /// </summary>
        UnPay = 1,
        /// <summary>
        /// 已支付
        /// </summary>
        Submited = 2
    }
    /// <summary>
    /// 无形资产订单提交状态
    /// </summary>
    public enum IntangibleAssetsSubmitStatusEnum
    {
        /// <summary>
        /// 首付款待发起支付
        /// </summary>
        FirstPaymentUnSubmit = 0,
        /// <summary>
        /// 首付款支付中
        /// </summary>
        FirstPaymentUnPay = 1,
        /// <summary>
        /// 中期款待发起支付
        /// </summary>
        InterimPaymentUnSubmit = 2,
        /// <summary>
        /// 中期款支付中
        /// </summary>
        InterimPaymentUnPay = 3,
        /// <summary>
        /// 尾款待发起支付
        /// </summary>
        TailPaymentUnSubmit = 4,
        /// <summary>
        /// 尾款支付中
        /// </summary>
        TailPaymentUnPay = 5,
        /// <summary>
        /// 已支付
        /// </summary>
        Submited = 6
    }
}