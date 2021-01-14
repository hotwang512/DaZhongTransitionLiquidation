using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using DaZhongTransitionLiquidation.Controllers;
using SyntacticSugar;
using DaZhongTransitionLiquidation.Common;
using System;

namespace DaZhongTransitionLiquidation
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            log4net.Config.XmlConfigurator.Configure();
            LogHelper.WriteLog(string.Format("Liquidation 站点启动，时间：{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            //GlobalConfiguration.Configure(WebApiConfig.Register);
            //AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            #region 依赖注入
            var builder = new ContainerBuilder();
            var services = Assembly.Load("DaZhongTransitionLiquidation.Infrastructure");
            builder.RegisterAssemblyTypes(services);
            //builder.RegisterType<DbService>().As<DbService>();
            //builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly());
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            #endregion
            AutoMapper.Configuration.Configure();
            log4net.Config.XmlConfigurator.Configure();
            //是否执行自动服务
            var ExecutionOrNot = ConfigSugar.GetAppString("Execution").TryToBoolean();
            if (ExecutionOrNot)
            {
                //AutoSyncBankFlow.AutoSyncBankSeavice();

                //AutoSyncBankFlow.AutoSyncSeavice();
                //AutoSyncBankFlow.AutoSyncYesterdaySeavice();
                //AutoSyncBankFlow.AutoBankTransferResult();
                //AutoSyncBankFlow.AutoVehicleSeavice();
                //AutoSyncEmailController.AutoSyncEmailSeavice();
                AutoSyncBankFlow.AutoGetVoucherMoneySeavice();

                //AutoSyncAssetsMaintenance.AutoSyncSeavice();
                //AutoSyncBankFlow.AutoTransferVoucherSeavice();
            }
            //资产变更
            AutoSyncAssetsMaintenance.AutoSyncSeavice();
        }

        public void Application_End(object sender, EventArgs e)
        {
            LogHelper.WriteLog(string.Format("Liquidation 站点停止，时间：{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        public static bool IsWirterSyncBankFlow = false;
    }
}
