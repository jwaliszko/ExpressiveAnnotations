using ExpressiveAnnotations.MvcUnobtrusive.Providers;

namespace ExpressiveAnnotations.MvcWebSample.Inheritance
{
    public class CustomExpressiveAnnotationsModelValidatorProvider : ExpressiveAnnotationsModelValidatorProvider
    {
        public CustomExpressiveAnnotationsModelValidatorProvider()
        {
            RegisterAdapter(typeof (CustomRequiredIfAttribute), typeof (CustomRequiredIfValidator));
            RegisterAdapter(typeof (CustomAssertThatAttribute), typeof (CustomAssertThatValidator));
        }
    }
}