using System;
using System.Linq;
using System.Web.Mvc;
using ExpressiveAnnotations.ConditionalAttributes;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    internal class ExpressionValidatorInternals
    {
        public string ErrorMessage { get; private set; }
        public string[] DependentProperties { get; private set; }
        public string[] RelationalOperators { get; private set; }
        public object[] TargetValues { get; private set; }
        public string[] Types { get; private set; }
        public string Expression { get; private set; }
        public bool SensitiveComparisons { get; set; }

        public void Prepare(ModelMetadata metadata, IExpressionAttribute attribute)
        {
            if (attribute.DependentProperties.Length != attribute.TargetValues.Length)
                throw new ArgumentException("Number of elements in DependentProperties and TargetValues must match.");
            if (attribute.RelationalOperators.Any() &&
                attribute.RelationalOperators.Length != attribute.DependentProperties.Length)
                throw new ArgumentException("Number of explicitly provided relational operators is incorrect.");

            var count = attribute.DependentProperties.Count();
            DependentProperties = new string[count];
            RelationalOperators = new string[count];
            TargetValues = new object[count];
            Types = new string[count];

            for (var i = 0; i < count; i++)
            {
                var dependentProperty = MiscHelper.ExtractProperty(metadata.ContainerType, attribute.DependentProperties[i]);
                var relationalOperator = attribute.RelationalOperators.Any() ? attribute.RelationalOperators[i] : "==";

                string targetPropertyName;
                if (MiscHelper.TryExtractName(attribute.TargetValues[i], out targetPropertyName))
                {
                    var targetProperty = MiscHelper.ExtractProperty(metadata.ContainerType, targetPropertyName);
                    Assert.ConsistentTypes(dependentProperty, targetProperty, metadata.PropertyName, relationalOperator);
                }
                else
                    Assert.ConsistentTypes(dependentProperty, attribute.TargetValues[i], metadata.PropertyName,
                        relationalOperator);

                DependentProperties[i] = attribute.DependentProperties[i];
                RelationalOperators[i] = relationalOperator;
                TargetValues[i] = attribute.TargetValues[i];

                Types[i] = TypeHelper.GetCoarseType(dependentProperty.PropertyType);
            }

            Expression = attribute.Expression;
            ErrorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(),
                MiscHelper.ComposeExpression(Expression, DependentProperties, TargetValues, RelationalOperators));

            SensitiveComparisons = attribute.SensitiveComparisons;
        }
    }
}
