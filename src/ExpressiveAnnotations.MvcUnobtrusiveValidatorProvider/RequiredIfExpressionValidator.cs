using System.Web.Mvc;
using System.Collections.Generic;
using ExpressiveAnnotations.Attributes;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    /// <summary>
    /// Model validator for <see cref="RequiredIfExpressionAttribute"/>.
    /// </summary>
    public class RequiredIfExpressionValidator : DataAnnotationsModelValidator<RequiredIfExpressionAttribute>
    {
        private readonly ExpressionValidatorInternals _internals = new ExpressionValidatorInternals();

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
            _internals.Prepare(metadata, attribute);
        }

        /// <summary>
        /// Retrieves a collection of client validation rules (rules sent to browsers).
        /// </summary>
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = _internals.ErrorMessage,
                ValidationType = "requiredifexpression",
            };
            rule.ValidationParameters.Add("dependentproperties", JsonConvert.SerializeObject(_internals.DependentProperties));
            rule.ValidationParameters.Add("relationaloperators", JsonConvert.SerializeObject(_internals.RelationalOperators));
            rule.ValidationParameters.Add("targetvalues", JsonConvert.SerializeObject(_internals.TargetValues));
            rule.ValidationParameters.Add("types", JsonConvert.SerializeObject(_internals.Types)); 
            rule.ValidationParameters.Add("expression", _internals.Expression);
            rule.ValidationParameters.Add("sensitivecomparisons", JsonConvert.SerializeObject(_internals.SensitiveComparisons));
            yield return rule;
        }
    }
}
