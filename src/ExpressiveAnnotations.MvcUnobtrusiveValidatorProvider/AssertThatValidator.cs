using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Collections.Generic;
using ExpressiveAnnotations.ConditionalAttributes;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    public class AssertThatValidator : DataAnnotationsModelValidator<AssertThatAttribute>
    {
        private readonly string _expression;
        private readonly string _errorMessage;
        private readonly IDictionary<string, string> _types;

        public AssertThatValidator(ModelMetadata metadata, ControllerContext context, AssertThatAttribute attribute)
            : base(metadata, context, attribute)
        {
            _expression = attribute.Expression;
            _errorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), attribute.Expression);

            var regex = new Regex("[a-zA-Z0-9.]+");
            var matches = regex.Matches(attribute.Expression);
            _types = new Dictionary<string, string>();
            foreach (Match match in matches)
            {
                if (!_types.Keys.Contains(match.Value))
                {
                    if (!new[] { "true", "false", "null" }.Contains(match.Value))
                    {
                        var pi = Helper.ExtractProperty(metadata.ContainerType, match.Value);
                        if (pi != null)
                            _types.Add(match.Value, Helper.GetCoarseType(pi.PropertyType));
                    }
                }
            }
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = _errorMessage,
                ValidationType = "assertthat",
            };
            rule.ValidationParameters.Add("expression", JsonConvert.SerializeObject(_expression));
            rule.ValidationParameters.Add("types", JsonConvert.SerializeObject(_types));
            yield return rule;
        }
    }
}
