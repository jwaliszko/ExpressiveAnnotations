using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using ExpressiveAnnotations.BooleanExpressionsAnalyser;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class RequiredIfExpressionAttribute : ValidationAttribute
    {
        private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();

        private const string _defaultErrorMessage = "The {0} field is required because is based on custom defined logic expression.";

        /// <summary>
        /// Logical expression based on which requirement condition is calculated. If condition is fulfilled, error message is displayed. 
        /// Attribute logic replaces one or more format items in specific expression string with comparison results of dependent fields 
        /// and corresponding target values. Available string expression tokens are: &amp;&amp;, ||, !, {, }, numbers and whitespaces.
        /// Example: "{0} &amp;&amp; !{1}" is parsed to (DependentProperties[0] == TargetValues[0]) &amp;&amp; (DependentProperties[1] != TargetValues[1]).
        /// </summary>
        public string Expression { get; set; }
        /// <summary>
        /// Dependent fields from which runtime values are extracted.
        /// </summary>
        public string[] DependentProperties { get; set; }
        /// <summary>
        /// Expected values for corresponding dependent fields.
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
                var dependentvalue = field.GetValue(validationContext.ObjectInstance, null);
                var result = dependentvalue != null
                                 ? dependentvalue.Equals(TargetValues[i])
                                 : TargetValues[i] == null;
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