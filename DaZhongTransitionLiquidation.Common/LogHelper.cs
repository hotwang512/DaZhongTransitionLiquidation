using System.Collections.Generic;
using System.Threading;

namespace DaZhongTransitionLiquidation.Common
{
    //Author:Luis.liu
    //Date:2017.12.11
    //Desc:记录错误日志
    public class LogHelper
    {
        public static Queue<string> ExceptionStringQueue = new Queue<string>();//异常消息队列
        public static List<ILogWriter> LogWriters = new List<ILogWriter>();
        static LogHelper()
        {
            LogWriters.Add(new Log4NetWriter());
            //从队列中获取消息写到日志中去
            ThreadPool.QueueUserWorkItem(it =>
            {
                while (true)
                {
                    lock (ExceptionStringQueue)
                    {
                        if (ExceptionStringQueue.Count > 0)
                        {
                            string str = ExceptionStringQueue.Dequeue();//错误消息出队列
                            foreach (var logWriter in LogWriters)
                            {
                                logWriter.WriteLogInfo(str);
                            }
                        }
                        else
                        {

                            Thread.Sleep(30);
                        }
                    }
                }
               
            });
        }

        public static void WriteLog(string exceptionText)
        {
            lock (ExceptionStringQueue)
            {
                ExceptionStringQueue.Enqueue(exceptionText);//错误消息进队列
            }
        }
    }
}
