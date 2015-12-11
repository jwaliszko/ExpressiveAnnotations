using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Inheritance
{
    public class LocalizedRequiredIfAttribute: ExpressiveAttribute
    {
        public bool AllowEmptyStrings { get; set; }

        public LocalizedRequiredIfAttribute(string expression)
            : base(expression, "The {0} field is conditionally required.") // this default message will be overriden by resources
        {
            AllowEmptyStrings = false;
            ErrorMessageResourceType = typeof(Resources);
            ErrorMessageResourceName = "LocalizedRequiredIfDefaultError";
        }

        protected override ValidationResult IsValidInternal(object value, ValidationContext validationContext)
        {
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
    }
}
