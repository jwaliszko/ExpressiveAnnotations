using System;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    /// <summary>
    /// Validation attribute, executed for non-empty annotated field, which indicates that given assertion has to be satisfied, for such field to be considered as valid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AssertThatAttribute : ValidationAttribute, IAttribute
    {
        private const string _defaultErrorMessage = "Assertion for {0} field is not satisfied by the following logic: {1}.";

        /// <summary>
        /// Gets or sets the name of dependent field from which runtime value is extracted.
        /// </summary>
        public string DependentProperty { get; set; }

        /// <summary>
        /// Gets or sets the expected value for dependent field (wildcard character * stands for any value). There is also possibility 
        /// of value runtime extraction from backing field, by providing its name [inside square brackets].
        /// </summary>
        public object TargetValue { get; set; }

        /// <summary>
        /// Gets or sets the relational operator describing relation between dependent field and target value. Available operators: 
        /// ==, !=, >, >=, &lt;, &lt;=. If this property is not provided, equality operator == is used by default.
        /// </summary>
        public string RelationalOperator { get; set; }

        /// <summary>
        /// Gets or sets whether the string comparisons are case sensitive or not.
        /// </summary>
        public bool SensitiveComparisons { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertThatAttribute"/> class.
        /// </summary>
        public AssertThatAttribute()
            : base(_defaultErrorMessage)
        {
            SensitiveComparisons = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertThatAttribute"/> class.
        /// </summary>
        /// <param name="dependentProperty">The name of dependent field from which runtime value is extracted.</param>
        /// <param name="targetValue">The expected value for dependent field (wildcard character * stands for any value). There is also possibility of value runtime extraction from backing field, by providing its name [inside square brackets].</param>
        /// <param name="relationalOperator">The relational operator describing relation between dependent field and target value. Available operators: ==, !=, &gt;, &gt;=, &lt;, &lt;=. If this property is not provided, equality operator == is used by default.</param>
        /// <param name="sensitiveComparisons">Case sensitivity of string comparisons.</param>
        public AssertThatAttribute(string dependentProperty, object targetValue, string relationalOperator = null,
            bool sensitiveComparisons = true)
            : base(_defaultErrorMessage)
        {
            DependentProperty = dependentProperty;
            TargetValue = targetValue;
            RelationalOperator = relationalOperator;
            SensitiveComparisons = sensitiveComparisons;
        }

        /// <summary>
        /// Formats the error message.
        /// </summary>
        /// <param name="displayName">The user-visible name of the required field to include in the formatted message.</param>
        /// <param name="preprocessedExpression">The user-visible expression to include in the formatted message.</param>
        /// <returns>The localized message to present to the user.</returns>
        public string FormatErrorMessage(string displayName, string preprocessedExpression)
        {
            return string.Format(ErrorMessageString, displayName, preprocessedExpression);
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
            var internals = new AttributeInternals
            {
                DependentProperty = DependentProperty,
                TargetValue = TargetValue,
                RelationalOperator = RelationalOperator,
                SensitiveComparisons = SensitiveComparisons
            };

            if (!value.IsEmpty() && !internals.Validate(value, validationContext)) // executed for non-empty fields
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName,
                    MiscHelper.ComposeRelationalExpression(DependentProperty, TargetValue, RelationalOperator)));
            return ValidationResult.Success;
        }
    }
}