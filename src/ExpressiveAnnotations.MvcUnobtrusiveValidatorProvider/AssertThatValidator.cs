using System.Web.Mvc;
using System.Collections.Generic;
using ExpressiveAnnotations.Attributes;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    /// <summary>
    /// Model validator for <see cref="AssertThatAttribute" />.
    /// </summary>
    public class AssertThatValidator : DataAnnotationsModelValidator<AssertThatAttribute>
    {
        private readonly ValidatorInternals _internals = new ValidatorInternals();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertThatValidator" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        public AssertThatValidator(ModelMetadata metadata, ControllerContext context, AssertThatAttribute attribute)
            : base(metadata, context, attribute)
        {
            _internals.Prepare(metadata, attribute);
        }

        /// <summary>
        /// Retrieves a collection of client validation rules (rules sent to browsers).
        /// </summary>
        /// <returns>
        /// A collection of client validation rules.
        /// </returns>
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = _internals.ErrorMessage,
                ValidationType = "assertthat",
            };
            rule.ValidationParameters.Add("dependentproperty", JsonConvert.SerializeObject(_internals.DependentProperty));
            rule.ValidationParameters.Add("relationaloperator", JsonConvert.SerializeObject(_internals.RelationalOperator));
            rule.ValidationParameters.Add("targetvalue", JsonConvert.SerializeObject(_internals.TargetValue));
            rule.ValidationParameters.Add("type", JsonConvert.SerializeObject(_internals.Type));
            rule.ValidationParameters.Add("sensitivecomparisons", JsonConvert.SerializeObject(_internals.SensitiveComparisons));
            yield return rule;
        }
    }
}
