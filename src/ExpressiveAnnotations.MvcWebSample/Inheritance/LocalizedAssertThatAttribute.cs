using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Inheritance
{
    public class LocalizedAssertThatAttribute : ExpressiveAttribute
    {
        public LocalizedAssertThatAttribute(string expression)
            : base(expression, "Assertion for {0} field is not satisfied.") // this default message will be overriden by resources
        {
            ErrorMessageResourceType = typeof(Resources);
            ErrorMessageResourceName = "LocalizedAssertThatDefaultError";
        }

        protected override ValidationResult IsValidInternal(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                Compile(validationContext.ObjectType);
                if (!CachedValidationFuncs[validationContext.ObjectType](validationContext.ObjectInstance)) // check if the assertion condition is not satisfied
                    return new ValidationResult( // assertion not satisfied => notify
                        FormatErrorMessage(validationContext.DisplayName, Expression, validationContext.ObjectInstance),
                        new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
}
