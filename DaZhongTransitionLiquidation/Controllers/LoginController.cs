using System.Web.Mvc;
using DaZhongTransitionLiquidation.Common.Pub;
using DaZhongTransitionLiquidation.Infrastructure.Dao;
using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity;
using SyntacticSugar;

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
                    resultModel.ResultInfo = "用户名错误！";
                    return;
                }
                var enabled = db.Queryable<Sys_User>().Any(i => i.LoginName == userLoginInfo.LoginName && i.Enable == "1");
                if (!enabled)
                {
                    resultModel.ResultInfo = "用户已被禁用！";
                    return;
                }
                var userInfo = db.Queryable<Sys_User>().Where(i => i.LoginName == userLoginInfo.LoginName && i.Password == userLoginInfo.Password).Single();
                if (userInfo == null)
                {
                    resultModel.ResultInfo = "密码错误！";
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
    }


}