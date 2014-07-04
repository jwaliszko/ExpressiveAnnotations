using System;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.Attributes.Common
{
    internal class AttributeInternals
    {
        public string DependentProperty { get; set; }
        public object TargetValue { get; set; }
        public string RelationalOperator { get; set; }
        public bool SensitiveComparisons { get; set; }

        public bool Verify(ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException("validationContext", "ValidationContext not provided.");

            var dependentProperty = MiscHelper.ExtractProperty(validationContext.ObjectInstance.GetType(), DependentProperty);

            var dependentValue = MiscHelper.ExtractValue(validationContext.ObjectInstance, DependentProperty);
            var relationalOperator = RelationalOperator ?? "==";
            var targetValue = TargetValue;

            string targetPropertyName;
            if (MiscHelper.TryExtractName(targetValue, out targetPropertyName)) // check if target value does not containan encapsulated property name            
            {
                var targetProperty = MiscHelper.ExtractProperty(validationContext.ObjectInstance.GetType(), targetPropertyName);
                Assert.ConsistentTypes(dependentProperty, targetProperty, validationContext.DisplayName, relationalOperator);
                targetValue = MiscHelper.ExtractValue(validationContext.ObjectInstance, targetPropertyName);
            }
            else
                Assert.ConsistentTypes(dependentProperty, targetValue, validationContext.DisplayName, relationalOperator);

            return Comparer.Compute(dependentValue, targetValue, relationalOperator, SensitiveComparisons); // compare dependent value against target value
        }
    }
}
