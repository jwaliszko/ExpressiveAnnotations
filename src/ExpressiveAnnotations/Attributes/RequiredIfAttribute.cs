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
        private static string _defaultErrorMessage = "The {0} field is required by the following logic: {1}";

        /// <summary>
        ///     Gets or sets the default error message.
        /// </summary>
        public static string DefaultErrorMessage
        {
            get { return _defaultErrorMessage; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Default error message cannot be null.");
                _defaultErrorMessage = value;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RequiredIfAttribute" /> class.
        /// </summary>
        /// <param name="expression">The logical expression based on which requirement condition is computed.</param>
        public RequiredIfAttribute(string expression)
            : base(expression, DefaultErrorMessage)
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
            AssertNonValueType(value);

            var isEmpty = value is string && string.IsNullOrWhiteSpace((string)value);
            if (value == null || (isEmpty && !AllowEmptyStrings))
            {
                Compile(validationContext.ObjectType);
                if (CachedValidationFuncs[validationContext.ObjectType](validationContext.ObjectInstance)) // check if the requirement condition is satisfied
                    return new ValidationResult( // requirement confirmed => notify
                        FormatErrorMessage(validationContext.DisplayName, Expression, validationContext.ObjectInstance),
                        new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }

        /// <summary>
        ///     Indicates if the property is required with respect to the attribute and model value
        /// </summary>
        /// <param name="value">Model value to test</param>
        /// <returns>
        ///     True if the property is required, false otherwise
        /// </returns>
        public bool IsRequired(object value)
        {
            if (value == null)
                return false;

            Type type = value.GetType();
            if (!CachedValidationFuncs.ContainsKey(type))
            {
                CachedValidationFuncs[type] = Parser.Parse<bool>(type, Expression);
            }
            return CachedValidationFuncs[type](value);
        }

        private void AssertNonValueType(object value)
        {
            if (PropertyType == null)
                return;

            if (value != null && PropertyType.IsNonNullableValueType())
                throw new InvalidOperationException(
                    $"{nameof(RequiredIfAttribute)} has no effect when applied to a field of non-nullable value type '{PropertyType.FullName}'. Use nullable '{PropertyType.FullName}?' version instead, or switch to {nameof(AssertThatAttribute)} otherwise.");
        }
    }
}
