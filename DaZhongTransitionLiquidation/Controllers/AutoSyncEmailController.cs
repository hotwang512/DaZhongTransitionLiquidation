using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.NextDayData;
using DaZhongTransitionLiquidation.Common;
using OpenPop.Mime;
using OpenPop.Pop3;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class AutoSyncEmailController : Controller
    {
        public static void AutoSyncEmailSeavice()
        {
            Thread LogThread = new Thread(new ThreadStart(DoSyncEmailNextDataHuiDoQuan));
            //设置线程为后台线程,那样进程里就不会有未关闭的程序了  
            LogThread.IsBackground = true;
            LogThread.Start();//起线程  
        }
        public static void DoSyncEmailNextDataHuiDoQuan()
        {
            string syncTime = ConfigSugar.GetAppString("Email_SyncTime");
            while (true)
            {
                if (DateTime.Now.ToString("HH:mm:ss") == syncTime)
                {
                    ExceSyncEmail();
                }
                Thread.Sleep(1000);
            }
        }
        public static void ExceSyncEmail()
        {
            List<string> fileNames = GetEmailAttachments();
            if (fileNames.Count > 0)
            {
                ImportFile(fileNames);
            }
        }
        private static List<string> GetEmailAttachments()
        {
            List<string> fileNames = new List<string>();
            try
            {
                string bankSendMail = ConfigSugar.GetAppString("Email_BankSendMail");
                string fileFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Global.Temp.Replace("/", ""));
                using (Pop3Client pop3Client = new Pop3Client())
                {
                    if (pop3Client.Connected)
                        pop3Client.Disconnect();
                    pop3Client.Connect(ConfigSugar.GetAppString("Email_Server"), ConfigSugar.GetAppInt("Email_Port"), ConfigSugar.GetAppBool("Email_SSl"));
                    pop3Client.Authenticate(ConfigSugar.GetAppString("Email_UserName"), ConfigSugar.GetAppString("Email_Password"));
                    int count = pop3Client.GetMessageCount();

                    for (int i = count; i >= 1; i -= 1)
                    {
                        Message message = pop3Client.GetMessage(i);
                        if (message.Headers.DateSent > DateTime.Today && message.Headers.From.Address == bankSendMail)
                        {
                            List<MessagePart> attachments = message.FindAllAttachments();
                            if (attachments != null && attachments.Count > 0)
                            {
                                var messagePart = attachments[0];
                                string fileName = messagePart.FileName;
                                FileInfo file = new FileInfo(Path.Combine(fileFolder, fileName));
                                messagePart.Save(file);
                                fileNames.Add(fileName);
                                break;
                            }
                        }
                    }
                }
                LogHelper.WriteLog(string.Format("读取邮件成功:邮件数量：{0}", fileNames.Count));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("读取邮件错误:{0}", ex.ToString()));
            }
            return fileNames;
        }

        private static void ImportFile(List<string> fileNames)
        {
            NextDayDataPack nextDayDataPack = new NextDayDataPack();
            foreach (var fileName in fileNames)
            {
                nextDayDataPack.ImportDataHuiDouQuan(fileName);

            }

        }
    }
}