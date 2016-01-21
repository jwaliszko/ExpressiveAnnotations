using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusive.Providers;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;
using ExpressiveAnnotations.MvcWebSample.Inheritance;

namespace ExpressiveAnnotations.MvcWebSample
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // RegisterExpressiveAttributes(); // required for client-side validation to be working, but redundant if built-in ea model validation provider is used (see statement below)
            RegisterExpressiveModelValidatorProvider(); // ea model validation provider added here, it automatically registers adapters for expressive validation attributes and respects their processing priorities when validation is performed
            RegisterRedefinedExpressiveAttributes(); // just for demo, if you redefine attributes and validators, to e.g. override global error message, register them here as well

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static void RegisterExpressiveAttributes()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof (RequiredIfAttribute), typeof (RequiredIfValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof (AssertThatAttribute), typeof (AssertThatValidator));
        }

        private static void RegisterExpressiveModelValidatorProvider()
        {
            ModelValidatorProviders.Providers.Remove(
                ModelValidatorProviders.Providers.FirstOrDefault(x => x is DataAnnotationsModelValidatorProvider));
            ModelValidatorProviders.Providers.Add(new ExpressiveAnnotationsModelValidatorProvider());
        }

        private static void RegisterRedefinedExpressiveAttributes()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof (CustomizedRequiredIfAttribute), typeof (CustomizedRequiredIfValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof (CustomizedAssertThatAttribute), typeof (CustomizedAssertThatValidator));
        }
    }
}