using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.Attributes;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators
{
    /// <summary>
    /// Model validator for <see cref="RequiredIfAttribute" />.
    /// </summary>
    public class RequiredIfValidator : DataAnnotationsModelValidator<RequiredIfAttribute>
    {
        private string Expression { get; set; }
        private string FormattedErrorMessage { get; set; }        
        private bool AllowEmptyOrFalse { get; set; }
        private Type ModelType { get; set; }
        private IDictionary<string, string> TypesMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfValidator" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            Expression = attribute.Expression;
            FormattedErrorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), attribute.Expression);
            TypesMap = Helper.GetTypesMap(metadata, attribute.Expression);
            AllowEmptyOrFalse = attribute.AllowEmptyOrFalse;
            ModelType = metadata.ModelType;
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
                ErrorMessage = FormattedErrorMessage,
                ValidationType = "requiredif",
            };
            rule.ValidationParameters.Add("expression", JsonConvert.SerializeObject(Expression));
            rule.ValidationParameters.Add("typesmap", JsonConvert.SerializeObject(TypesMap));
            rule.ValidationParameters.Add("allowemptyorfalse", JsonConvert.SerializeObject(AllowEmptyOrFalse));
            rule.ValidationParameters.Add("modeltype", JsonConvert.SerializeObject(Helper.GetCoarseType(ModelType)));
            yield return rule;
        }
    }
}
