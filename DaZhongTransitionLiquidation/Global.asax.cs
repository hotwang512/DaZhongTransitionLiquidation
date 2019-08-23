using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using DaZhongTransitionLiquidation.Controllers;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
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
                AutoSyncBankFlow.AutoSyncSeavice();
                AutoSyncBankFlow.AutoSyncYesterdaySeavice();
                AutoSyncBankFlow.AutoBankTransferResult();
                AutoSyncBankFlow.AutoSyncBankSeavice();
                AutoSyncAssetsMaintenance.AutoSyncSeavice();
                AutoSyncBankFlow.AutoVehicleSeavice();
                AutoSyncEmailController.AutoSyncEmailSeavice();
                //AutoSyncBankFlow.AutoTransferVoucherSeavice();
            }
        }
    }
}
