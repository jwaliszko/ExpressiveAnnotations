using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ExpressiveAnnotations.LogicalExpressionsAnalysis;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    /// <summary>
    /// Validation attribute which indicates that annotated field is required when computed result of given logical expression is true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredIfExpressionAttribute : ValidationAttribute
    {
        private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();
        private const string _defaultErrorMessage = "The {0} field is required by the following logic: {1}.";

        /// <summary>
        /// Gets or sets the logical expression based on which requirement condition is computed. 
        /// Available expression tokens: &amp;&amp;, ||, !, {, }, numbers and whitespaces.
        /// </summary>
        public string Expression { get; set; }
        
        /// <summary>
        /// Gets or sets the dependent fields from which runtime values are extracted.
        /// </summary>
        public string[] DependentProperties { get; set; }
        
        /// <summary>
        /// Gets or sets the expected values for corresponding dependent fields (wildcard character * stands for any value). There is also 
        /// possibility for dynamic extraction of target values from backing fields, by providing their names [inside square brackets].
        /// </summary>
        public object[] TargetValues { get; set; }
        
        /// <summary>
        /// Gets or sets the relational operators describing relations between dependent fields and corresponding target values.
        /// Available operators: ==, !=, >, >=, &lt;, &lt;=. If this property is not provided, equality operator == is used by default.
        /// </summary>
        public string[] RelationalOperators { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfExpressionAttribute"/> class.
        /// </summary>
        public RequiredIfExpressionAttribute()
            : base(_defaultErrorMessage)
        {
            DependentProperties = new string[0];
            TargetValues = new object[0];
            RelationalOperators = new string[0];
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

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException("validationContext", "ValidationContext not provided.");

            if (DependentProperties.Length != TargetValues.Length)
                throw new ArgumentException("Number of elements in DependentProperties and TargetValues must match.");
            if (RelationalOperators.Any() && RelationalOperators.Length != DependentProperties.Length)
                throw new ArgumentException("Number of explicitly provided relational operators is incorrect.");

            var attributeName = GetType().Name;
            var tokens = new List<object>();

            for (var i = 0; i < DependentProperties.Count(); i++)
            {
                var dependentProperty = PropHelper.ExtractProperty(validationContext.ObjectInstance, DependentProperties[i]);

                var dependentValue = PropHelper.ExtractValue(validationContext.ObjectInstance, DependentProperties[i]);
                var relationalOperator = RelationalOperators.Any() ? RelationalOperators[i] : "==";
                var targetValue = TargetValues[i];                                

                string targetPropertyName;
                if (PropHelper.TryExtractName(targetValue, out targetPropertyName)) // check if target value does not containan encapsulated property name
                {
                    var targetProperty = PropHelper.ExtractProperty(validationContext.ObjectInstance, targetPropertyName);
                    Assert.ConsistentTypes(dependentProperty, targetProperty, validationContext.DisplayName, attributeName, relationalOperator);
                    targetValue = PropHelper.ExtractValue(validationContext.ObjectInstance, targetPropertyName);
                }
                else
                    Assert.ConsistentTypes(dependentProperty, targetValue, validationContext.DisplayName, attributeName, relationalOperator);

                var result = Comparer.Compute(dependentValue, targetValue, relationalOperator); // compare dependent value against target value
                tokens.Add(result.ToString().ToLowerInvariant());
            }

            var expression = string.Format(Expression, tokens.ToArray());
            var evaluator = new Evaluator();
            if (evaluator.Compute(expression)) // evaluate logical expression
            {
                // expression result is true => means we should try to validate this field (verify if required value is provided)  
                if (!_innerAttribute.IsValid(value) || (value is bool && !(bool) value))
                    // validation failed - return an error
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName,
                        PropHelper.ComposeExpression(Expression, DependentProperties, TargetValues, RelationalOperators)));
            }

            return ValidationResult.Success;
        }
    }
}