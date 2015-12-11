using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;

namespace ExpressiveAnnotations.MvcWebSample.Inheritance
{
    public class LocalizedAssertThatValidator : ExpressiveValidator<LocalizedAssertThatAttribute>
    {
        public LocalizedAssertThatValidator(ModelMetadata metadata, ControllerContext context, LocalizedAssertThatAttribute attribute)
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
