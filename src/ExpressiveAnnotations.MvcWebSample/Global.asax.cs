using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusive.Providers;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;

namespace ExpressiveAnnotations.MvcWebSample
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            //DataAnnotationsModelValidatorProvider.RegisterAdapter( // if ea validation provider is used, adapters registration is done there (therefore manual registration is redundant here)
            //    typeof (RequiredIfAttribute), typeof (RequiredIfValidator));
            //DataAnnotationsModelValidatorProvider.RegisterAdapter(
            //    typeof (AssertThatAttribute), typeof (AssertThatValidator));

            ModelValidatorProviders.Providers.Remove(
                ModelValidatorProviders.Providers.FirstOrDefault(x => x is DataAnnotationsModelValidatorProvider));
            ModelValidatorProviders.Providers.Add(new ExpressiveAnnotationsModelValidatorProvider()); // ea validation provider added here
                                                                                                      // it automatically registers adapters for expressive validation attributes and respects their procesing priorities when validation is performed

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}