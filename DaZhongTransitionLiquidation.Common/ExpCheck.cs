using System;

namespace DaZhongTransitionLiquidation.Common
{
    public class ExpCheck
    {
        /// <summary>
        /// 使用指定的错误消息初始化 System.Exception 类的新实例。
        /// </summary>
        /// <param name="isException">true则引发异常</param>
        /// <param name="message">错误信息</param>
        /// <param name="args">参数</param>
        public static void Exception(bool isException, string message, params string[] args)
        {
            if (isException)
                throw new Exception(string.Format(message, args));
        }
    }
}