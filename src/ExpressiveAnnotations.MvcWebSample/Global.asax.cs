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
using ExpressiveAnnotations.MvcWebSample.Misc;

namespace ExpressiveAnnotations.MvcWebSample
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            CustomToolchain.Register(); // if you need additional utility-like functions, register them here (insted of at models level)

            // !attributes registration done here: -------------------------------------------------------------------

            // required for client-side validation to be working, but redundant if built-in ea model validation provider is used (see statement below)
            // RegisterExpressiveAttributes();

            // ea model validation provider added here, it automatically registers adapters for expressive validation attributes and respects their processing priorities when validation is performed
            // RegisterExpressiveModelValidatorProvider();

            // just for demo, if you redefine attributes and validators, because you need to e.g. override global error messages, register them here as well            
            // RegisterAdditionalExpressiveAttributes();            

            // best way - write custom provider by inheriting from ExpressiveAnnotationsModelValidatorProvider, and do it all there (keeps priorities working everywhere)
            RegisterCustomExpressiveModelValidatorProvider();

            // !------------------------------------------------------------------------------------------------------

            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //private static void RegisterExpressiveAttributes()
        //{
        //    DataAnnotationsModelValidatorProvider.RegisterAdapter(
        //        typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
        //    DataAnnotationsModelValidatorProvider.RegisterAdapter(
        //        typeof(AssertThatAttribute), typeof(AssertThatValidator));
        //}

        //private static void RegisterExpressiveModelValidatorProvider()
        //{
        //    ModelValidatorProviders.Providers.Remove(
        //        ModelValidatorProviders.Providers.FirstOrDefault(x => x is DataAnnotationsModelValidatorProvider));
        //    ModelValidatorProviders.Providers.Add(new ExpressiveAnnotationsModelValidatorProvider());
        //}

        //private static void RegisterAdditionalExpressiveAttributes()
        //{
        //    DataAnnotationsModelValidatorProvider.RegisterAdapter(
        //        typeof (CustomRequiredIfAttribute), typeof (CustomRequiredIfValidator));
        //    DataAnnotationsModelValidatorProvider.RegisterAdapter(
        //        typeof (CustomAssertThatAttribute), typeof (CustomAssertThatValidator));
        //}

        private static void RegisterCustomExpressiveModelValidatorProvider()
        {
            ModelValidatorProviders.Providers.Remove(
                ModelValidatorProviders.Providers.FirstOrDefault(x => x is DataAnnotationsModelValidatorProvider));
            ModelValidatorProviders.Providers.Add(new CustomExpressiveAnnotationsModelValidatorProvider());
        }
    }
}