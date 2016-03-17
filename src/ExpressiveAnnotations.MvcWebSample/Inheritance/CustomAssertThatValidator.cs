using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;

namespace ExpressiveAnnotations.MvcWebSample.Inheritance
{
    public class CustomAssertThatValidator : ExpressiveValidator<CustomAssertThatAttribute>
    {
        public CustomAssertThatValidator(ModelMetadata metadata, ControllerContext context, CustomAssertThatAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = GetBasicRule("assertthat");
            yield return rule;
        }
    }
}
