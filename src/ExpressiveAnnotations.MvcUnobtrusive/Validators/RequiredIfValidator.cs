/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcUnobtrusive.Validators
{
    /// <summary>
    ///     Model validator for <see cref="RequiredIfAttribute" />.
    /// </summary>
    public class RequiredIfValidator : ExpressiveValidator<RequiredIfAttribute>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RequiredIfValidator" /> class.
        /// </summary>
        /// <param name="metadata">The model metadata instance.</param>
        /// <param name="context">The controller context instance.</param>
        /// <param name="attribute">The expressive requirement attribute instance.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"></exception>
        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            AllowEmpty = attribute.AllowEmptyStrings;
        }

        private bool AllowEmpty { get; set; }

        /// <summary>
        ///     Retrieves a collection of client validation rules (which are next sent to browsers).
        /// </summary>
        /// <returns>
        ///     A collection of client validation rules.
        /// </returns>
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = GetBasicRule("requiredif");
            rule.ValidationParameters.Add("allowempty", AllowEmpty.ToJson());
            yield return rule;
        }
    }
}
