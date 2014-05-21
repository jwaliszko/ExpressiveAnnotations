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
        private readonly string[] _relationalOperators;
        private readonly object[] _targetValues;
        private readonly string[] _types;
        private readonly string _expression;

        public RequiredIfExpressionValidator(ModelMetadata metadata, ControllerContext context, RequiredIfExpressionAttribute attribute)
            : base(metadata, context, attribute)
        {
            if (attribute.DependentProperties.Length != attribute.TargetValues.Length)
                throw new ArgumentException("Number of elements in DependentProperties and TargetValues must match.");
            if (attribute.RelationalOperators.Any() && attribute.RelationalOperators.Length != attribute.DependentProperties.Length)
                throw new ArgumentException("Number of explicitly provided relational operators is incorrect.");           

            var count = attribute.DependentProperties.Count();
            _dependentProperties = new string[count];
            _relationalOperators = new string[count];
            _targetValues = new object[count];
            _types = new string[count];

            var attributeName = GetType().BaseType.GetGenericArguments().Single().Name;

            for (var i = 0; i < count; i++)
            {
                var dependentProperty = Helper.ExtractProperty(metadata.ContainerType, attribute.DependentProperties[i]);
                var relationalOperator = attribute.RelationalOperators.Any() ? attribute.RelationalOperators[i] : "==";

                string targetPropertyName;
                if (attribute.TargetValues[i].TryExtractPropertyName(out targetPropertyName))
                {
                    var targetProperty = Helper.ExtractProperty(metadata.ContainerType, targetPropertyName);
                    Assert.ConsistentTypes(dependentProperty, targetProperty, metadata.PropertyName, attributeName, relationalOperator);
                }
                else
                    Assert.ConsistentTypes(dependentProperty, attribute.TargetValues[i], metadata.PropertyName, attributeName, relationalOperator);

                _dependentProperties[i] = attribute.DependentProperties[i];
                _relationalOperators[i] = relationalOperator;
                _targetValues[i] = attribute.TargetValues[i];

                _types[i] = Helper.GetCoarseType(dependentProperty.PropertyType);
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
            rule.ValidationParameters.Add("relationaloperators", JsonConvert.SerializeObject(_relationalOperators));
            rule.ValidationParameters.Add("targetvalues", JsonConvert.SerializeObject(_targetValues));
            rule.ValidationParameters.Add("types", JsonConvert.SerializeObject(_types)); 
            rule.ValidationParameters.Add("expression", _expression);
            yield return rule;
        }
    }
}
