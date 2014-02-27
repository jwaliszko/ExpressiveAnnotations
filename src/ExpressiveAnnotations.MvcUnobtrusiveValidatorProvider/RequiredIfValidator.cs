using System.Web.Mvc;
using ExpressiveAnnotations.ConditionalAttributes;
using System.Collections.Generic;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    public class RequiredIfValidator : DataAnnotationsModelValidator<RequiredIfAttribute>
    {
        private readonly string _errorMessage;
        private readonly string _dependentProperty;
        private readonly string _targetValue;

        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            _errorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName());
            _dependentProperty = attribute.DependentProperty;
            _targetValue = attribute.TargetValue.ToString().ToLowerInvariant();
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = _errorMessage,
                ValidationType = "requiredif",
            };
            rule.ValidationParameters.Add("dependentproperty", _dependentProperty);
            rule.ValidationParameters.Add("targetvalue", _targetValue);
            yield return rule;
        }
    }
}
