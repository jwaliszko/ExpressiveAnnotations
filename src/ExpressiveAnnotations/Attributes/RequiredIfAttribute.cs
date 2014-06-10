using System;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredIfAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "The {0} field is required by the following logic: {1}.";

        public string Expression { get; set; }

        public RequiredIfAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public RequiredIfAttribute(string expression)
            : base(_defaultErrorMessage)
        {
            Expression = expression;
        }

        public string FormatErrorMessage(string displayName, string preprocessedExpression)
        {
            return string.Format(ErrorMessageString, displayName, preprocessedExpression);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException("validationContext", "ValidationContext not provided.");

            if (IsEmpty(value) || (value is bool && !(bool)value)) // check if the field is empty or false (continue if so, otherwise skip condition verification)
            {
                var parser = new Parser();
                var validator = parser.Parse(validationContext.ObjectInstance.GetType(), Expression);

                if (validator.Invoke(validationContext.ObjectInstance)) // check if the requirement condition is satisfied
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName, Expression)); // requirement confirmed => notify
            }

            return ValidationResult.Success;
        }

        private static bool IsEmpty(object value)
        {
            return value == null || (value is string && string.IsNullOrWhiteSpace((string)value));
        }
    }
}
