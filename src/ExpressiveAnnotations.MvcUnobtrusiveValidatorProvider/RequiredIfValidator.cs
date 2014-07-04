using System;
using System.Web.Mvc;
using System.Collections.Generic;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.Misc;
using ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Common;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    /// <summary>
    /// Model validator for <see cref="RequiredIfAttribute" />.
    /// </summary>
    public class RequiredIfValidator : DataAnnotationsModelValidator<RequiredIfAttribute>
    {
        private ValidatorInternals Internals { get; set; }
        private bool AllowEmptyOrFalse { get; set; }
        private Type ModelType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfValidator" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            Internals = new ValidatorInternals();
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
                ValidationType = "requiredif",
            };
            rule.ValidationParameters.Add("dependentproperty", JsonConvert.SerializeObject(Internals.DependentProperty));
            rule.ValidationParameters.Add("relationaloperator", JsonConvert.SerializeObject(Internals.RelationalOperator));
            rule.ValidationParameters.Add("targetvalue", JsonConvert.SerializeObject(Internals.TargetValue));
            rule.ValidationParameters.Add("type", JsonConvert.SerializeObject(Internals.Type));
            rule.ValidationParameters.Add("sensitivecomparisons", JsonConvert.SerializeObject(Internals.SensitiveComparisons));
            rule.ValidationParameters.Add("allowemptyorfalse", JsonConvert.SerializeObject(AllowEmptyOrFalse));
            rule.ValidationParameters.Add("modeltype", JsonConvert.SerializeObject(TypeHelper.GetCoarseType(ModelType)));
            yield return rule;
        }
    }
}
