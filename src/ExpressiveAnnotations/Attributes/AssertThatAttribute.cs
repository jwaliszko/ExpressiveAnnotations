using System;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.LogicalExpressionsAnalysis.SyntacticAnalysis;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AssertThatAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "Assertion for {0} field is not satisfied by the following logic: {1}.";

        public string Expression { get; set; }

        public AssertThatAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public AssertThatAttribute(string expression)
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

            if (!IsEmpty(value)) // check if the field is non-empty (continue if so, otherwise skip condition verification)
            {
                var parser = new Parser();
                var validator = parser.Parse(validationContext.ObjectInstance.GetType(), Expression);

                if (!validator.Invoke(validationContext.ObjectInstance)) // check if the assertion condition is not satisfied
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName, Expression)); // assertion not satisfied => notify
            }

            return ValidationResult.Success;
        }

        private static bool IsEmpty(object value)
        {
            return value == null || (value is string && string.IsNullOrWhiteSpace((string)value));
        }
    }
}
