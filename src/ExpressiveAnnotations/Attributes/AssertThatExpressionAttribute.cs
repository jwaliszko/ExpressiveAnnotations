using System;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    /// Validation attribute, executed for non-null annotated field, which indicates that assertion given in logical expression has to be satisfied, for such field to be considered as valid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AssertThatExpressionAttribute : ValidationAttribute, IExpressionAttribute
    {
        private const string _defaultErrorMessage = "Assertion for {0} field is not satisfied by the following logic: {1}.";

        /// <summary>
        /// Gets or sets the logical expression based on which requirement condition is computed.
        /// Available expression tokens: &amp;&amp;, ||, !, {, }, numbers and whitespaces.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets the names of dependent fields from which runtime values are extracted.
        /// </summary>
        public string[] DependentProperties { get; set; }

        /// <summary>
        /// Gets or sets the expected values for corresponding dependent fields (wildcard character * stands for any non-empty value). There is also
        /// possibility of values runtime extraction from backing fields, by providing their names [inside square brackets].
        /// </summary>
        public object[] TargetValues { get; set; }

        /// <summary>
        /// Gets or sets the relational operators describing relations between dependent fields and corresponding target values.
        /// Available operators: ==, !=, &gt;, &gt;=, &lt;, &lt;=. If this property is not provided, equality operator == is used by default.
        /// </summary>
        public string[] RelationalOperators { get; set; }

        /// <summary>
        /// Gets or sets whether the string comparisons are case sensitive or not.
        /// </summary>
        public bool SensitiveComparisons { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertThatExpressionAttribute"/> class.
        /// </summary>
        public AssertThatExpressionAttribute()
            : base(_defaultErrorMessage)
        {
            DependentProperties = new string[0];
            TargetValues = new object[0];
            RelationalOperators = new string[0];
            SensitiveComparisons = true;
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
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var internals = new ExpressionAttributeInternals
            {
                Expression = Expression,
                DependentProperties = DependentProperties,
                TargetValues = TargetValues,
                RelationalOperators = RelationalOperators,
                SensitiveComparisons = SensitiveComparisons
            };

            if (value != null)
                if (!internals.Verify(validationContext))
                    return new ValidationResult(
                        FormatErrorMessage(validationContext.DisplayName,
                            MiscHelper.ComposeExpression(Expression, DependentProperties, TargetValues, RelationalOperators)),
                        new[] {validationContext.MemberName});

            return ValidationResult.Success;
        }
    }
}