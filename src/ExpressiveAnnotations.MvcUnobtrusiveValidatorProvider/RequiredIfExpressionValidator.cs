using System;
using System.Web.Mvc;
using System.Collections.Generic;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.Misc;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    /// <summary>
    /// Model validator for <see cref="RequiredIfExpressionAttribute" />.
    /// </summary>
    public class RequiredIfExpressionValidator : DataAnnotationsModelValidator<RequiredIfExpressionAttribute>
    {
        private ExpressionValidatorInternals Internals { get; set; }
        private bool AllowEmptyOrFalse { get; set; }
        private Type ModelType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfExpressionValidator" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        public RequiredIfExpressionValidator(ModelMetadata metadata, ControllerContext context, RequiredIfExpressionAttribute attribute)
            : base(metadata, context, attribute)
        {
            Internals = new ExpressionValidatorInternals();
            Internals.Prepare(metadata, attribute);
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
                ErrorMessage = Internals.ErrorMessage,
                ValidationType = "requiredifexpression",
            };
            rule.ValidationParameters.Add("dependentproperties", JsonConvert.SerializeObject(Internals.DependentProperties));
            rule.ValidationParameters.Add("relationaloperators", JsonConvert.SerializeObject(Internals.RelationalOperators));
            rule.ValidationParameters.Add("targetvalues", JsonConvert.SerializeObject(Internals.TargetValues));
            rule.ValidationParameters.Add("types", JsonConvert.SerializeObject(Internals.Types));
            rule.ValidationParameters.Add("expression", Internals.Expression);
            rule.ValidationParameters.Add("sensitivecomparisons", JsonConvert.SerializeObject(Internals.SensitiveComparisons));
            rule.ValidationParameters.Add("allowemptyorfalse", JsonConvert.SerializeObject(AllowEmptyOrFalse));
            rule.ValidationParameters.Add("modeltype", JsonConvert.SerializeObject(TypeHelper.GetCoarseType(ModelType)));
            yield return rule;
        }
    }
}
