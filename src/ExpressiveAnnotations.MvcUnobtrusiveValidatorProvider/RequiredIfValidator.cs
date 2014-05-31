using System.Web.Mvc;
using ExpressiveAnnotations.ConditionalAttributes;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    /// <summary>
    /// Model validator for <see cref="RequiredIfAttribute"/>.
    /// </summary>
    public class RequiredIfValidator : DataAnnotationsModelValidator<RequiredIfAttribute>
    {
        private readonly ValidatorInternals m_internals = new ValidatorInternals();

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfValidator"/> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            m_internals.Prepare(metadata, attribute);
        }

        /// <summary>
        /// Retrieves a collection of client validation rules (rules sent to browsers).
        /// </summary>
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = m_internals.ErrorMessage,
                ValidationType = "requiredif",
            };
            rule.ValidationParameters.Add("dependentproperty", JsonConvert.SerializeObject(m_internals.DependentProperty));
            rule.ValidationParameters.Add("relationaloperator", JsonConvert.SerializeObject(m_internals.RelationalOperator));
            rule.ValidationParameters.Add("targetvalue", JsonConvert.SerializeObject(m_internals.TargetValue));
            rule.ValidationParameters.Add("type", JsonConvert.SerializeObject(m_internals.Type));
            yield return rule;
        }
    }
}
