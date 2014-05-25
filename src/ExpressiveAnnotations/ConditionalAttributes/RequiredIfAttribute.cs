using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ExpressiveAnnotations.LogicalExpressionsAnalysis;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    /// <summary>
    /// Validation attribute which indicates that annotated field is required when dependent field has appropriate value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredIfAttribute : ValidationAttribute
    {
        private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();
        private const string _defaultErrorMessage = "The {0} field is required because depends on {1} field.";

        /// <summary>
        /// Gets or sets the dependent field from which runtime value is extracted.
        /// </summary>
        public string DependentProperty { get; set; }

        /// <summary>
        /// Gets or sets the expected value for dependent field (wildcard character * stands for any value). There is also possibility 
        /// for dynamic extraction of target value from backing field, by providing its name [inside square brackets].
        /// </summary>
        public object TargetValue { get; set; }

        /// <summary>
        /// Gets or sets the relational operator describing relation between dependent field and target value. Available operators: 
        /// ==, !=, >, >=, &lt;, &lt;=. If this property is not provided, equality operator == is used by default.
        /// </summary>
        public string RelationalOperator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        public RequiredIfAttribute()
            : base(_defaultErrorMessage)
        {
        }

        /// <summary>
        /// Formats the error message.
        /// </summary>
        /// <param name="displayName">The user-visible name of the required field to include in the formatted message.</param>
        /// <param name="dependentPropertyName">The user-visible name of the dependent field to include in the formatted message.</param>
        /// <returns>The localized message to present to the user.</returns>
        public string FormatErrorMessage(string displayName, string dependentPropertyName)
        {
            return string.Format(ErrorMessageString, displayName, dependentPropertyName);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException("validationContext", "ValidationContext not provided.");

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