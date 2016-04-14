/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.ComponentModel.DataAnnotations;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    ///     Validation attribute, executed for null-only annotated field, which indicates that such a field
    ///     is required to be non-null, when computed result of given logical expression is true.
    /// </summary>
    public sealed class RequiredIfAttribute : ExpressiveAttribute
    {
        private const string _defaultErrorMessage = "The {0} field is required by the following logic: {1}";

        /// <summary>
        ///     Initializes a new instance of the <see cref="RequiredIfAttribute" /> class.
        /// </summary>
        /// <param name="expression">The logical expression based on which requirement condition is computed.</param>
        public RequiredIfAttribute(string expression)
            : base(expression, _defaultErrorMessage)
        {
            AllowEmptyStrings = false;
        }

        /// <summary>
        ///     Gets or sets a flag indicating whether the attribute should allow empty or whitespace strings.
        /// </summary>
        /// <remarks>
        ///     <c>false</c> by default.
        /// </remarks>
        public bool AllowEmptyStrings { get; set; }

        /// <summary>
        ///     Validates a specified value with respect to the associated validation attribute.
        ///     Internally used by the <see cref="ExpressiveAttribute.IsValid(object,System.ComponentModel.DataAnnotations.ValidationContext)" /> method.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>
        ///     An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected override ValidationResult IsValidInternal(object value, ValidationContext validationContext)
        {
            var propType = validationContext.ObjectType.GetProperty(validationContext.MemberName).PropertyType;
            if (value != null && propType.IsNonNullableValueType())
                throw new InvalidOperationException(
                    $"{nameof(RequiredIfAttribute)} has no effect when applied to a field of non-nullable value type '{propType.FullName}'. Use nullable '{propType.FullName}?' version instead.");

            var isEmpty = value is string && string.IsNullOrWhiteSpace((string) value);
            if (value == null || (isEmpty && !AllowEmptyStrings))
            {
                Compile(validationContext.ObjectType);
                if (CachedValidationFuncs[validationContext.ObjectType](validationContext.ObjectInstance)) // check if the requirement condition is satisfied
                    return new ValidationResult( // requirement confirmed => notify
                        FormatErrorMessage(validationContext.DisplayName, Expression, validationContext.ObjectInstance),
                        new[] {validationContext.MemberName});
            }

            return ValidationResult.Success;
        }
    }
}
