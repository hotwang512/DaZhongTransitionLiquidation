namespace DaZhongTransitionLiquidation.Common.Pub
{
    public enum AuthorityEnum
    {
        /// <summary>
        /// 查询
        /// </summary>
        Reads = 1,

        /// <summary>
        /// 新增
        /// </summary>
        Adds = 2,

        /// <summary>
        /// 编辑
        /// </summary>
        Edit = 3,

        /// <summary>
        /// 删除
        /// </summary>
        Deletes = 4,

        /// <summary>
        /// 启用
        /// </summary>
        Enable = 5,

        /// <summary>
        /// 禁用
        /// </summary>
        Disable = 6,

        /// <summary>
        /// 导入
        /// </summary>
        Import = 7,

        /// <summary>
        /// 导出
        /// </summary>
        Export = 8
    }
}