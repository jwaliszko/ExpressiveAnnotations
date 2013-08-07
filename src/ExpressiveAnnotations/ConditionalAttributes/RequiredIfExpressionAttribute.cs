using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ExpressiveAnnotations.BooleanExpressionsAnalyser;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    /// <summary>
    /// Provides conditional attribute to calculate validation result based on related properties values
    /// and relations between them, which are defined in logical expression.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
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
        /// target values from other fields, by providing their names inside square parentheses.
        /// </summary>
        public object[] TargetValues { get; set; }

        public RequiredIfExpressionAttribute()
            : base(_defaultErrorMessage)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var tokens = new List<object>();
            var containerType = validationContext.ObjectInstance.GetType();
            for (var i = 0; i < DependentProperties.Count(); i++)
            {
                var field = containerType.GetProperty(DependentProperties[i]);
                var dependentValue = field.GetValue(validationContext.ObjectInstance, null);
                var targetValue = Helper.FetchTargetValue(TargetValues[i], validationContext);

                var result = dependentValue != null
                                 ? dependentValue.Equals(targetValue)
                                 : targetValue == null;

                tokens.Add(result.ToString().ToLowerInvariant());
            }

            var expression = string.Format(Expression, tokens.ToArray());
            var evaluator = new Evaluator();

            if (evaluator.Compute(expression))
            {
                // match => means we should try validating this field
                if (!_innerAttribute.IsValid(value))
                {
                    // validation failed - return an error
                    return new ValidationResult(String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                                                              validationContext.DisplayName));
                }
            }

            return ValidationResult.Success;
        }
    }
}