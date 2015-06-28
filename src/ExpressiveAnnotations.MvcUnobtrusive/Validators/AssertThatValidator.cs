/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcUnobtrusive.Validators
{
    /// <summary>
    ///     Model validator for <see cref="AssertThatAttribute" />.
    /// </summary>
    public class AssertThatValidator : ExpressiveValidator<AssertThatAttribute>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssertThatValidator" /> class.
        /// </summary>
        /// <param name="metadata">The model metadata instance.</param>
        /// <param name="context">The controller context instance.</param>
        /// <param name="attribute">The expressive assertion attribute instance.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"></exception>
        public AssertThatValidator(ModelMetadata metadata, ControllerContext context, AssertThatAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        /// <summary>
        ///     Retrieves a collection of client validation rules (which are next sent to browsers).
        /// </summary>
        /// <returns>
        ///     A collection of client validation rules.
        /// </returns>
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormattedErrorMessage,
                ValidationType = ProvideUniqueValidationType("assertthat")
            };

            rule.ValidationParameters.Add("expression", Expression);
            rule.ValidationParameters.Add("fieldsmap", FieldsMap.ToJson());
            rule.ValidationParameters.Add("constsmap", ConstsMap.ToJson());
            yield return rule;
        }
    }
}
