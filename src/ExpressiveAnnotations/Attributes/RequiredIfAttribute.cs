using System;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    /// Validation attribute which indicates that annotated field is required when computed result of given logical expression is true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredIfAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "The {0} field is required by the following logic: {1}.";
        private Func<object, bool> CachedFunc { get; set; }
        private Parser Parser { get; set; }

        /// <summary>
        /// Gets or sets the logical expression based on which requirement condition is computed. 
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the attribute should allow empty or whitespace strings or false boolean values (null never allowed).
        /// </summary>
        public bool AllowEmptyOrFalse { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute" /> class.
        /// </summary>
        /// <param name="expression">The logical expression based on which requirement condition is computed.</param>
        public RequiredIfAttribute(string expression)
            : base(_defaultErrorMessage)
        {
            Parser = new Parser();
            Parser.RegisterMethods();

            Expression = expression;
            AllowEmptyOrFalse = false;
        }

        /// <summary>
        /// Formats the error message.
        /// </summary>
        /// <param name="displayName">The user-visible name of the required field to include in the formatted message.</param>
        /// <param name="expression">The user-visible expression to include in the formatted message.</param>
        /// <returns>The localized message to present to the user.</returns>
        public string FormatErrorMessage(string displayName, string expression)
        {
            return string.Format(ErrorMessageString, displayName, expression);
        }

        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">validationContext;ValidationContext not provided.</exception>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException("validationContext", "ValidationContext not provided.");

            var emptyOrFalse = (value is string && string.IsNullOrWhiteSpace((string)value)) || (value is bool && !(bool)value);
            if (value == null || (emptyOrFalse && !AllowEmptyOrFalse))
            {
                var validator = CachedFunc ?? (CachedFunc = Parser.Parse(validationContext.ObjectInstance.GetType(), Expression));
                if (validator(validationContext.ObjectInstance)) // check if the requirement condition is satisfied
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName, Expression)); // requirement confirmed => notify
            }

            return ValidationResult.Success;
        }
    }
}
