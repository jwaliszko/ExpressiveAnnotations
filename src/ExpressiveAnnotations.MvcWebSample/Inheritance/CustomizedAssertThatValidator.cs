using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;

namespace ExpressiveAnnotations.MvcWebSample.Inheritance
{
    public class CustomizedAssertThatValidator : ExpressiveValidator<CustomizedAssertThatAttribute>
    {
        public CustomizedAssertThatValidator(ModelMetadata metadata, ControllerContext context, CustomizedAssertThatAttribute attribute)
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
