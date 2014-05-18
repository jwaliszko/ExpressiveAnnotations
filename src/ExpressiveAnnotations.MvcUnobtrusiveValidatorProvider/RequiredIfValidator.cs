using System;
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
        private readonly string _relationalOperator;
        private readonly object _targetValue;

        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            var dependentProperty = Helper.ExtractProperty(metadata.ContainerType, attribute.DependentProperty);

            string targetPropertyName;
            var attributeName = GetType().BaseType.GetGenericArguments().Single().Name;
            if (attribute.TargetValue.IsEncapsulated(out targetPropertyName))
            {
                var targetProperty = Helper.ExtractProperty(metadata.ContainerType, targetPropertyName);
                Assert.ConsistentTypes(dependentProperty, targetProperty, metadata.PropertyName, attributeName);
            }
            else
                Assert.ConsistentTypes(dependentProperty, attribute.TargetValue, metadata.PropertyName, attributeName);

            var displayAttribute = dependentProperty.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            var dependentPropertyName = displayAttribute != null ? displayAttribute.GetName() : attribute.DependentProperty;

            _errorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), dependentPropertyName);
            _dependentProperty = attribute.DependentProperty;
            _relationalOperator = attribute.RelationalOperator;
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
            rule.ValidationParameters.Add("relationaloperator", JsonConvert.SerializeObject(_relationalOperator));
            rule.ValidationParameters.Add("targetvalue", JsonConvert.SerializeObject(_targetValue));
            yield return rule;
        }
    }
}
