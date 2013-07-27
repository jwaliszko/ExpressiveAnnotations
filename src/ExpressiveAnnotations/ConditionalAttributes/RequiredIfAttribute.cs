using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();

        private const string _defaultErrorMessage = "The {0} field is required because depends on {1} field.";

        /// <summary>
        /// Field from which runtime value is extracted.
        /// </summary>
        public string DependentProperty { get; set; }
        /// <summary>
        /// Expected value for dependent field. If runtime value is the same, 
        /// requirement condition is fulfilled and error message is displayed.
        /// </summary>
        public object TargetValue { get; set; }

        public RequiredIfAttribute()
            : base(_defaultErrorMessage)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // get a reference to the property this validation depends upon
            var containerType = validationContext.ObjectInstance.GetType();
            var field = containerType.GetProperty(DependentProperty);

            var attribs = field.GetCustomAttributes(typeof (DisplayAttribute), false);
            var attrib = attribs.Length > 0 ? (DisplayAttribute) attribs[0] : null;
            var dependentPropertyName = attrib != null ? attrib.GetName() : DependentProperty;

            // get the value of the dependent property
            var dependentvalue = field.GetValue(validationContext.ObjectInstance, null);

            // compare the value against the target value
            if ((dependentvalue == null && TargetValue == null) ||
                (dependentvalue != null && dependentvalue.Equals(TargetValue)) ||
                (dependentvalue != null && dependentvalue.ToString()
                                                         .Equals(
                                                             TargetValue != null ? TargetValue.ToString() : string.Empty,
                                                             StringComparison.OrdinalIgnoreCase)))
            {
                // match => means we should try validating this field
                if (!_innerAttribute.IsValid(value))
                    // validation failed - return an error
                    return new ValidationResult(String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                                                              validationContext.DisplayName, dependentPropertyName));
            }

            return ValidationResult.Success;
        }
    }
}