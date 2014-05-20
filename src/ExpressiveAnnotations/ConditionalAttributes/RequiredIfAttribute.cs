using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    /// <summary>
    /// Provides conditional attribute to calculate validation result based on related property value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();

        private const string _defaultErrorMessage = "The {0} field is required because depends on {1} field.";

        /// <summary>
        /// Field from which runtime value is extracted.
        /// </summary>
        public string DependentProperty { get; set; }
        /// <summary>
        /// Expected value for dependent field. Instead of hardcoding there is also possibility for dynamic extraction of 
        /// target value from other field, by providing its name inside square parentheses. Star character stands for any value.
        /// </summary>
        public object TargetValue { get; set; }
        /// <summary>
        /// Operator describing relation between dependent property and target value.
        /// Available operators: ==, !=, >, >=, &lt;, &le;. If this property is not provided, default relation is ==.
        /// </summary>
        public string RelationalOperator { get; set; }

        public RequiredIfAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public string FormatErrorMessage(string displayName, string dependentPropertyName)
        {
            return string.Format(ErrorMessageString, displayName, dependentPropertyName);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {            
            // get a reference to the property this validation depends upon
            var dependentProperty = Helper.ExtractProperty(validationContext.ObjectInstance, DependentProperty);            
            
            var dependentValue = Helper.ExtractValue(validationContext.ObjectInstance, DependentProperty);
            var targetValue = TargetValue;

            string targetPropertyName;
            var attributeName = GetType().Name;
            if (targetValue.TryExtractPropertyName(out targetPropertyName)) // check if target value is not an encapsulated property
            {
                var targetProperty = Helper.ExtractProperty(validationContext.ObjectInstance, targetPropertyName);
                Assert.ConsistentTypes(dependentProperty, targetProperty, validationContext.DisplayName, attributeName);
                targetValue = Helper.ExtractValue(validationContext.ObjectInstance, targetPropertyName);
            }
            else
                Assert.ConsistentTypes(dependentProperty, targetValue, validationContext.DisplayName, attributeName);            

            // compare the value against the target value
            if (Helper.Compute(dependentValue, targetValue, RelationalOperator ?? "=="))
            {
                var displayAttribute = dependentProperty.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
                var dependentPropertyName = displayAttribute != null ? displayAttribute.GetName() : DependentProperty;

                // match => means we should try to validate this field
                if (!_innerAttribute.IsValid(value) || (value is bool && !(bool) value))
                    // validation failed - return an error
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName, dependentPropertyName));
            }

            return ValidationResult.Success;
        }
    }
}