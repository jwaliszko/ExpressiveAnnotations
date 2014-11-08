/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System.ComponentModel.DataAnnotations;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    ///     Validation attribute which indicates that annotated field is required when computed result of given logical expression is true.
    /// </summary>
    public sealed class RequiredIfAttribute : ExpressiveAttribute
    {
        private const string _defaultErrorMessage = "The {0} field is required by the following logic: {1}.";

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
        public bool AllowEmptyStrings { get; set; }

        protected override ValidationResult IsValidInternal(object value, ValidationContext validationContext)
        {
            var isEmpty = value is string && string.IsNullOrWhiteSpace((string) value);
            if (value == null || (isEmpty && !AllowEmptyStrings))
            {
                Compile(validationContext.ObjectType);
                if (CachedValidationFuncs[validationContext.ObjectType](validationContext.ObjectInstance)) // check if the requirement condition is satisfied
                    return new ValidationResult( // requirement confirmed => notify
                        FormatErrorMessage(validationContext.DisplayName, Expression),
                        new[] {validationContext.MemberName});
            }

            return ValidationResult.Success;
        }
    }
}
