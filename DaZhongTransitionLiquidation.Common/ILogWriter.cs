namespace DaZhongTransitionLiquidation.Common
{
    /// <summary>
    /// Author:Lis.liu
    /// Date:2017.12.11
    /// Desc:创建写错误日志的接口
    /// </summary>
    public interface ILogWriter
    {
        void WriteLogInfo(string txt);
    }
}