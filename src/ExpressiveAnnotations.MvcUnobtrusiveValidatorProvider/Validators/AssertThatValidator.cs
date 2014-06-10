using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.Attributes;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators
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
            _types = Helper.GetTypesMap(metadata, attribute.Expression);
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
