using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ExpressiveAnnotations.LogicalExpressionsAnalysis;
using ExpressiveAnnotations.Misc;

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
            var dependentProperty = PropHelper.ExtractProperty(validationContext.ObjectInstance, DependentProperty);
            
            var dependentValue = PropHelper.ExtractValue(validationContext.ObjectInstance, DependentProperty);
            var relationalOperator = RelationalOperator ?? "==";
            var targetValue = TargetValue;            

            string targetPropertyName;
            var attributeName = GetType().Name;
            if (PropHelper.TryExtractName(targetValue, out targetPropertyName)) // check if target value does not containan encapsulated property name
            {
                var targetProperty = PropHelper.ExtractProperty(validationContext.ObjectInstance, targetPropertyName);
                Assert.ConsistentTypes(dependentProperty, targetProperty, validationContext.DisplayName, attributeName, relationalOperator);
                targetValue = PropHelper.ExtractValue(validationContext.ObjectInstance, targetPropertyName);
            }
            else
                Assert.ConsistentTypes(dependentProperty, targetValue, validationContext.DisplayName, attributeName, relationalOperator);

            if (Comparer.Compute(dependentValue, targetValue, relationalOperator)) // compare dependent value against target value
            {
                var displayAttribute = dependentProperty.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
                var dependentPropertyName = displayAttribute != null ? displayAttribute.GetName() : DependentProperty;

                // match => means we should try to validate this field (verify if required value is provided)  
                if (!_innerAttribute.IsValid(value) || (value is bool && !(bool) value))
                    // validation failed - return an error
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName, dependentPropertyName));
            }

            return ValidationResult.Success;
        }
    }
}