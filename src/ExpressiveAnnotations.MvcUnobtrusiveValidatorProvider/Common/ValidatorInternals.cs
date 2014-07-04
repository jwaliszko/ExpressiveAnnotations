using System.Web.Mvc;
using ExpressiveAnnotations.Attributes.Common;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Common
{
    internal class ValidatorInternals
    {
        public string ErrorMessage { get; private set; }
        public string DependentProperty { get; private set; }
        public string RelationalOperator { get; private set; }
        public object TargetValue { get; private set; }
        public string Type { get; private set; }
        public bool SensitiveComparisons { get; set; }

        public void Prepare(ModelMetadata metadata, IAttribute attribute)
        {
            var dependentProperty = MiscHelper.ExtractProperty(metadata.ContainerType, attribute.DependentProperty);
            var relationalOperator = attribute.RelationalOperator ?? "==";

            string targetPropertyName;
            if (MiscHelper.TryExtractName(attribute.TargetValue, out targetPropertyName))
            {
                var targetProperty = MiscHelper.ExtractProperty(metadata.ContainerType, targetPropertyName);
                Assert.ConsistentTypes(dependentProperty, targetProperty, metadata.PropertyName, relationalOperator);
            }
            else
                Assert.ConsistentTypes(dependentProperty, attribute.TargetValue, metadata.PropertyName, relationalOperator);

            DependentProperty = attribute.DependentProperty;
            RelationalOperator = relationalOperator;
            TargetValue = attribute.TargetValue;

            Type = TypeHelper.GetCoarseType(dependentProperty.PropertyType);
            ErrorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(),
                MiscHelper.ComposeRelationalExpression(DependentProperty, TargetValue, RelationalOperator));

            SensitiveComparisons = attribute.SensitiveComparisons;
        }
    }
}
