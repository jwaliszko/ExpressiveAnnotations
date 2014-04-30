using System;
using System.Reflection;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    public class Assert
    {
        public static void ConsistentTypes(PropertyInfo dependentField, object targetValue, string field, string attribute)
        {
            if (targetValue == null || string.Equals(targetValue as string, "*"))
                return; // type doesn't matter when null or star is involved

            if (dependentField.PropertyType != targetValue.GetType())
                throw new InvalidOperationException(
                    string.Format(
                        "Type mismatch detected in {0} definition for {1} field. Types consistency is required for {2} field and its corresponding target value (unless target is null or *).",
                        attribute, field, dependentField.Name));
        }
    }
}
