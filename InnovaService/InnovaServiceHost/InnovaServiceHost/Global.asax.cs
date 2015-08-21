using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Routing;
using System.Web.UI;
using InnovaServiceHost.App_Start;

namespace InnovaServiceHost {
    public class WebApiApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            var myScriptResDef = new ScriptResourceDefinition();
            myScriptResDef.Path = "~/Scripts/jquery-2.0.3.min.js";
            myScriptResDef.DebugPath = "~/Scripts/jquery-2.0.3.min.js";
            ScriptManager.ScriptResourceMapping.AddDefinition("jquery", null, myScriptResDef);
            var config = GlobalConfiguration.Configuration;
            config.Formatters.Insert(0, new JsonMediaTypeFormatter());
            FormatterConfig.RegisterFormatters(GlobalConfiguration.Configuration.Formatters);
            //HttpConfiguration.EnsureInitialized();
            //config.Formatters.Remove(config.Formatters.XmlFormatter);    


            /*
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
             * */
        }
    }
}
