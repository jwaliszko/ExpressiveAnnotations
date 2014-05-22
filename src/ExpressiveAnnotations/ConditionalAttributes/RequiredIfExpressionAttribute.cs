using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ExpressiveAnnotations.LogicalExpressionAnalysis;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    /// <summary>
    /// Provides conditional attribute to calculate validation result based on related properties values
    /// and relations between them, which are defined in logical expression.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredIfExpressionAttribute : ValidationAttribute
    {
        private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();

        private const string _defaultErrorMessage = "The {0} field is required because is based on custom defined logic expression.";

        /// <summary>
        /// Logical expression based on which requirement condition is calculated. If condition is fulfilled, error message is displayed. 
        /// Attribute logic replaces one or more format items in specific expression string with comparison results of dependent fields 
        /// and corresponding target values. 
        /// 
        /// Available expression tokens are: &amp;&amp;, ||, !, {, }, numbers and whitespaces.
        /// 
        /// Example: "{0} &amp;&amp; !{1}" is parsed to (DependentProperties[0] == TargetValues[0]) &amp;&amp; (DependentProperties[1] != TargetValues[1]).
        /// </summary>
        public string Expression { get; set; }
        /// <summary>
        /// Dependent fields from which runtime values are extracted.
        /// </summary>
        public string[] DependentProperties { get; set; }
        /// <summary>
        /// Expected values for corresponding dependent fields. Instead of hardcoding there is also possibility for dynamic extraction of 
        /// target values from other fields, by providing their names inside square parentheses. Star character stands for any value.
        /// </summary>
        public object[] TargetValues { get; set; }
        /// <summary>
        /// Operators describing relations between dependent properties and corresponding target values.
        /// Available operators: ==, !=, >, >=, &lt;, &le;. If this property is not provided, default relation for all operands is ==.
        /// </summary>
        public string[] RelationalOperators { get; set; }

        public RequiredIfExpressionAttribute()
            : base(_defaultErrorMessage)
        {
            DependentProperties = new string[0];
            TargetValues = new object[0];
            RelationalOperators = new string[0];
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
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
                    return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }
    }
}