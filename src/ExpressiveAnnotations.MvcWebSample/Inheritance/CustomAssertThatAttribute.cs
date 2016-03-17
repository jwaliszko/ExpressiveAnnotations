using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Resources;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Inheritance
{
    public class CustomAssertThatAttribute : ExpressiveAttribute
    {
        public CustomAssertThatAttribute(string expression)
            : base(expression, () => // alternative way of providing dynamic messages resolution - using error message accessor
                {
                    var rm = new ResourceManager(typeof (Resources));
                    return rm.GetString("CustomizedAssertThatDefaultError");
                })
        {
        }

        public override string FormatErrorMessage(string displayName, string expression, object objectInstance) // use if you need custom formatting
        {
            var message = base.FormatErrorMessage(displayName, expression, objectInstance);
            return $"{message} !?";
        }

        public override string FormatErrorMessage(string displayName, string expression, Type objectType, out IDictionary<string, Guid> fieldsMap)  // use if you need custom formatting
        {
            var message = base.FormatErrorMessage(displayName, expression, objectType, out fieldsMap);
            return $"{message} ?!";
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
