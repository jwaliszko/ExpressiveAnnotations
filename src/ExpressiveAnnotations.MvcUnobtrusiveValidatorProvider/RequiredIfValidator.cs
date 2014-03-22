using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using ExpressiveAnnotations.ConditionalAttributes;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    public class RequiredIfValidator : DataAnnotationsModelValidator<RequiredIfAttribute>
    {
        private readonly string _errorMessage;
        private readonly string _dependentProperty;
        private readonly object _targetValue;

        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            var field = metadata.ContainerType.GetProperty(attribute.DependentProperty);
            var attrib = field.GetCustomAttributes(typeof (DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            var dependentPropertyName = attrib != null ? attrib.GetName() : attribute.DependentProperty;

            _errorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), dependentPropertyName);
            _dependentProperty = attribute.DependentProperty;
            _targetValue = attribute.TargetValue ?? string.Empty;    // null returned as empty string at client side
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = _errorMessage,
                ValidationType = "requiredif",
            };
            rule.ValidationParameters.Add("dependentproperty", JsonConvert.SerializeObject(_dependentProperty));
            rule.ValidationParameters.Add("targetvalue", JsonConvert.SerializeObject(_targetValue));
            yield return rule;
        }
    }
}
