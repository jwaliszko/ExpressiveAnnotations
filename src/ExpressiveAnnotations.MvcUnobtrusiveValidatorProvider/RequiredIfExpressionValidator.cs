using System.Linq;
using System.Web.Mvc;
using ExpressiveAnnotations.ConditionalAttributes;
using System.Collections.Generic;
using System;
using ExpressiveAnnotations.Misc;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    /// <summary>
    /// Model validator for <see cref="RequiredIfExpressionAttribute"/>.
    /// </summary>
    public class RequiredIfExpressionValidator : DataAnnotationsModelValidator<RequiredIfExpressionAttribute>
    {
        private readonly string _errorMessage;
        private readonly string[] _dependentProperties;
        private readonly string[] _relationalOperators;
        private readonly object[] _targetValues;
        private readonly string[] _types;
        private readonly string _expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfExpressionValidator"/> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        /// <exception cref="System.ArgumentException">
        /// Number of elements in DependentProperties and TargetValues must match.
        /// or
        /// Number of explicitly provided relational operators is incorrect.
        /// </exception>
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
                var dependentProperty = PropHelper.ExtractProperty(metadata.ContainerType, attribute.DependentProperties[i]);
                var relationalOperator = attribute.RelationalOperators.Any() ? attribute.RelationalOperators[i] : "==";

                string targetPropertyName;
                if (PropHelper.TryExtractName(attribute.TargetValues[i], out targetPropertyName))
                {
                    var targetProperty = PropHelper.ExtractProperty(metadata.ContainerType, targetPropertyName);
                    Assert.ConsistentTypes(dependentProperty, targetProperty, metadata.PropertyName, attributeName, relationalOperator);
                }
                else
                    Assert.ConsistentTypes(dependentProperty, attribute.TargetValues[i], metadata.PropertyName, attributeName, relationalOperator);

                _dependentProperties[i] = attribute.DependentProperties[i];
                _relationalOperators[i] = relationalOperator;
                _targetValues[i] = attribute.TargetValues[i];

                _types[i] = TypeHelper.GetCoarseType(dependentProperty.PropertyType);
            }
            
            _expression = attribute.Expression;
            _errorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(),
                PropHelper.ComposeExpression(_expression, _dependentProperties, _targetValues, _relationalOperators));
        }

        /// <summary>
        /// Retrieves a collection of client validation rules.
        /// </summary>
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
