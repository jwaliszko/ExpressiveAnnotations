using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ExpressiveAnnotations.ConditionalAttributes;
using ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider;

namespace ExpressiveAnnotations.MvcWebSample
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof (RequiredIfAttribute), typeof (RequiredIfValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(RequiredIfExpressionAttribute), typeof(RequiredIfExpressionValidator));

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}