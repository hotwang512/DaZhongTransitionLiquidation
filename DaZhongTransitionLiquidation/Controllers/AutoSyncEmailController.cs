using DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.NextDayData;
using DaZhongTransitionLiquidation.Common;
using OpenPop.Mime;
using OpenPop.Pop3;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                ExceSyncEmail();
                Thread.Sleep(2000*60*60);
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
        public static List<string> GetEmailAttachments()
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
                    //获取文件中的邮件数
                    var emailCount = 0;
                    var filePath = AppDomain.CurrentDomain.BaseDirectory + "\\App_Data\\EmailCount.txt";
                    if (FileSugar.IsExistFile(filePath))
                    { 
                        var str = FileSugar.GetFileSream(filePath);
                        emailCount = Encoding.UTF8.GetString(str).TryToInt();
                    }
                    LogHelper.SaveEmailCount(count);
                    for (int i = count; i > emailCount; i -= 1)
                    {
                        Message message = pop3Client.GetMessage(i);
                        if (message.Headers.From.Address == bankSendMail)
                        {
                            List<MessagePart> attachments = message.FindAllAttachments();
                            if (attachments != null && attachments.Count > 0)
                            {
                                var messagePart = attachments[0];
                                string fileName = messagePart.FileName;
                                FileInfo file = new FileInfo(Path.Combine(fileFolder, fileName));
                                messagePart.Save(file);
                                fileNames.Add(fileName);
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

        public static void ImportFile(List<string> fileNames)
        {
            NextDayDataPack nextDayDataPack = new NextDayDataPack();
            foreach (var fileName in fileNames)
            {
                nextDayDataPack.ImportDataHuiDouQuan(fileName);

            }

        }
    }
}