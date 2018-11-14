using System.Reflection;
using log4net;

namespace DaZhongTransitionLiquidation.Common
{
    public class Log4NetWriter : ILogWriter
    {
        /// <summary>
        /// Author:Luis.liu
        /// Date:2017.12.11
        /// Desc:log4net写错误日志
        /// </summary>
        /// <param name="txt"></param>
        public void WriteLogInfo(string txt)
        {
            ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            log.Error(txt);
        }
    }
}