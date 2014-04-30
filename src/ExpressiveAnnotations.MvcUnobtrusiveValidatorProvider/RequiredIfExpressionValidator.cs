using System.Linq;
using System.Web.Mvc;
using ExpressiveAnnotations.ConditionalAttributes;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    public class RequiredIfExpressionValidator : DataAnnotationsModelValidator<RequiredIfExpressionAttribute>
    {
        private readonly string _errorMessage;
        private readonly string[] _dependentProperties;
        private readonly object[] _targetValues;
        private readonly string _expression;

        public RequiredIfExpressionValidator(ModelMetadata metadata, ControllerContext context, RequiredIfExpressionAttribute attribute)
            : base(metadata, context, attribute)
        {
            if (attribute.DependentProperties.Length != attribute.TargetValues.Length)
                throw new ArgumentException("Number of elements in DependentProperties and TargetValues must match.");

            var count = attribute.DependentProperties.Count();
            _dependentProperties = new string[count];
            _targetValues = new object[count];

            for (var i = 0; i < count; i++)
            {
                _dependentProperties[i] = attribute.DependentProperties[i];
                _targetValues[i] = attribute.TargetValues[i] ?? string.Empty;   // null returned as empty string at client side

                var field = metadata.ContainerType.GetProperty(attribute.DependentProperties[i]);
                Assert.ConsistentTypes(field, attribute.TargetValues[i], metadata.PropertyName, GetType().BaseType.GetGenericArguments().Single().Name);
            }

            _errorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName());
            _expression = attribute.Expression;
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = _errorMessage,
                ValidationType = "requiredifexpression",
            };
            rule.ValidationParameters.Add("dependentproperties", JsonConvert.SerializeObject(_dependentProperties));
            rule.ValidationParameters.Add("targetvalues", JsonConvert.SerializeObject(_targetValues));
            rule.ValidationParameters.Add("expression", _expression);
            yield return rule;
        }
    }
}
