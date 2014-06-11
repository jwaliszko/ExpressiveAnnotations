using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.Attributes;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators
{
    /// <summary>
    /// Model validator for <see cref="AssertThatAttribute"/>.
    /// </summary>
    public class AssertThatValidator : DataAnnotationsModelValidator<AssertThatAttribute>
    {
        private readonly string _expression;
        private readonly string _errorMessage;
        private readonly IDictionary<string, string> _typesMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertThatValidator" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        public AssertThatValidator(ModelMetadata metadata, ControllerContext context, AssertThatAttribute attribute)
            : base(metadata, context, attribute)
        {
            _expression = attribute.Expression;
            _errorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), attribute.Expression);            
            _typesMap = Helper.GetTypesMap(metadata, attribute.Expression);
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
                ErrorMessage = _errorMessage,
                ValidationType = "assertthat",
            };
            rule.ValidationParameters.Add("expression", JsonConvert.SerializeObject(_expression));
            rule.ValidationParameters.Add("types", JsonConvert.SerializeObject(_typesMap));
            yield return rule;
        }
    }
}
