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

            var propertyType = dependentField.PropertyType;
            propertyType = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof (Nullable<>)
                ? Nullable.GetUnderlyingType(propertyType)
                : propertyType;
            var targetType = targetValue.GetType();
            targetType = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof (Nullable<>)
                ? Nullable.GetUnderlyingType(targetType)
                : targetType;

            if (propertyType != targetType)
                throw new InvalidOperationException(
                    string.Format(
                        "Type mismatch detected in {0} definition for {1} field. Types consistency is required for {2} field and its corresponding target value (unless target is null or *).",
                        attribute, field, dependentField.Name));
        }
    }
}
