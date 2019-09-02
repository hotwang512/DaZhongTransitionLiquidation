using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;
using System.Net;
using DaZhongTransitionLiquidation.Common;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public DbService DbService;
        public LoginController(DbService dbService)
        {
            DbService = dbService;
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 处理登录
        /// </summary>
        /// <param name="userLoginInfo">用户登录信息</param>
        /// <returns></returns>
        public JsonResult ProcessLogin(Sys_User userLoginInfo)
        {
            var resultModel = new ResultModel<string> { IsSuccess = false, Status = "0" };
            DbService.Command(db =>
            {
                bool hasLoginName = db.Queryable<Sys_User>().Any(i => i.LoginName == userLoginInfo.LoginName);
                if (!hasLoginName)
                {
                    resultModel.ResultInfo = "用户名不存在！";
                    return;
                }
                var enabled = db.Queryable<Sys_User>().Any(i => i.LoginName == userLoginInfo.LoginName && i.Enable == "1");
                if (!enabled)
                {
                    resultModel.ResultInfo = "用户已被禁用！";
                    return;
                }
               var userInfo = db.Queryable<Sys_User>().Where(i => i.LoginName == userLoginInfo.LoginName && i.Password == userLoginInfo.Password).Single();
                //if (userInfo == null)
                //{
                //    resultModel.ResultInfo = "密码错误！";
                //    return;
                //}
                //账号存在且启用前往大众统一用户认证平台检验
                var url = ConfigSugar.GetAppString("GetUserLogin");
                var data = "{" +
                                "\"loginName\":\"{loginName}\",".Replace("{loginName}", userLoginInfo.LoginName) +
                                "\"loginPwd\":\"{loginPwd}\",".Replace("{loginPwd}", md5(userLoginInfo.Password)) +
                                "\"OperatorIP\":\"{OperatorIP}\",".Replace("{OperatorIP}", "192.168.173.4") +
                                "\"version\":\"{version}\",".Replace("{version}", "1.0.0") +
                                "\"versionLabel\":\"{versionLabel}\",".Replace("{versionLabel}", "Alpha") +
                                "\"FunctionSiteId\":\"{FunctionSiteId}\"".Replace("{FunctionSiteId}", "61") +
                                "}";
                var messageDate = "";
                try
                {
                    WebClient wc = new WebClient();
                    wc.Headers.Clear();
                    wc.Headers.Add("Content-Type", "application/json;charset=utf-8");
                    wc.Encoding = System.Text.Encoding.UTF8;
                    var resultData = wc.UploadString(new Uri(url), data);
                    messageDate = resultData;
                    var modelData = resultData.JsonToModel<DZLoginInfo>();
                    if (modelData.success)
                    {
                        var token = modelData.data.token;
                        var name = modelData.data.user.name;
                        var loginName = modelData.data.user.loginName;
                        db.Updateable<Sys_User>().UpdateColumns(it => new Sys_User()
                        {
                            Token = token,
                            Name = name
                        }).Where(it => it.LoginName == loginName).ExecuteCommand();
                        userInfo.Token = token; 
                        userInfo.Name = name; 
                    }
                    else
                    {
                        resultModel.ResultInfo = modelData.message;
                        return;
                    }
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, resultData));
                }
                catch (Exception ex)
                {
                    JObject Json = JObject.Parse(messageDate);
                    LogHelper.WriteLog(string.Format("Data:{0},result:{1}", data, ex.ToString()));
                    resultModel.ResultInfo = Json["message"].ToString();
                    resultModel.Status = "0";
                    return;
                }
                CacheManager<Sys_User>.GetInstance().Add(PubGet.GetUserKey, userInfo, 8 * 60 * 60);
                resultModel.IsSuccess = true;
                resultModel.Status = "1"; //登录成功
            });
            return Json(resultModel);
        }
        /// <summary>
        /// 退出系统
        /// 清除缓存
        /// </summary>
        public void Logout()
        {
            var cm = CacheManager<Sys_User>.GetInstance();
            cm.Remove(PubGet.GetUserKey);
            CookiesManager<string>.GetInstance().Remove(PubConst.CostCache);
        }
        /// <summary>
        /// 加密用户密码
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="codeLength">加密位数</param>
        /// <returns>加密密码</returns>
        public static string md5(string password)
        {
            MD5 algorithm = MD5.Create();
            // 32位加密
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(password))
            {          
                byte[] md5Bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < md5Bytes.Length; i++)
                {
                    builder.AppendFormat("{0:X2}", md5Bytes[i]);
                }
            }
            return builder.ToString();
        }
        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns>本机IP地址</returns>
        public static string GetLocalIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }


}