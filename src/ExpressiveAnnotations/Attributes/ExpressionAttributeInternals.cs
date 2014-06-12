using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.Attributes
{
    internal class ExpressionAttributeInternals
    {
        public string Expression { get; set; }
        public string[] DependentProperties { get; set; }
        public object[] TargetValues { get; set; }
        public string[] RelationalOperators { get; set; }
        public bool SensitiveComparisons { get; set; }

        public bool Verify(ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException("validationContext", "ValidationContext not provided.");

            if (DependentProperties.Length != TargetValues.Length)
                throw new ArgumentException("Number of elements in DependentProperties and TargetValues must match.");
            if (RelationalOperators.Any() && RelationalOperators.Length != DependentProperties.Length)
                throw new ArgumentException("Number of explicitly provided relational operators is incorrect.");

            var tokens = new List<object>();

            for (var i = 0; i < DependentProperties.Count(); i++)
            {
                var dependentProperty = MiscHelper.ExtractProperty(validationContext.ObjectInstance.GetType(), DependentProperties[i]);

                var dependentValue = MiscHelper.ExtractValue(validationContext.ObjectInstance, DependentProperties[i]);
                var relationalOperator = RelationalOperators.Any() ? RelationalOperators[i] : "==";
                var targetValue = TargetValues[i];                                

                string targetPropertyName;
                if (MiscHelper.TryExtractName(targetValue, out targetPropertyName)) // check if target value does not containan encapsulated property name
                {
                    var targetProperty = MiscHelper.ExtractProperty(validationContext.ObjectInstance.GetType(), targetPropertyName);
                    Assert.ConsistentTypes(dependentProperty, targetProperty, validationContext.DisplayName, relationalOperator);
                    targetValue = MiscHelper.ExtractValue(validationContext.ObjectInstance, targetPropertyName);
                }
                else
                    Assert.ConsistentTypes(dependentProperty, targetValue, validationContext.DisplayName, relationalOperator);

                var result = Comparer.Compute(dependentValue, targetValue, relationalOperator, SensitiveComparisons); // compare dependent value against target value
                tokens.Add(result.ToString().ToLowerInvariant());
            }

            var expression = string.Format(Expression, tokens.ToArray());
            var evaluator = new Evaluator();
            return evaluator.Compute(expression); // evaluate logical expression
        }
    }
}
