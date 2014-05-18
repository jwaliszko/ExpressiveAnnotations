using System;
using System.Reflection;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    public class Assert
    {
        public static void ConsistentTypes(PropertyInfo dependentProperty, object targetValue, string annotatedPropertyName, string attributeName)
        {
            if (targetValue == null || string.Equals(targetValue as string, "*"))
                return; // type doesn't matter when null or star is involved

            if (!ConsistentTypes(dependentProperty.PropertyType, targetValue.GetType()))
                throw new InvalidOperationException(
                    string.Format(
                        "Type mismatch detected in {0} definition for {1} field. Types consistency is required for {2} field and its corresponding, " +
                        "explicitly provided, target value (explicit target values, if null or *, does not interfere with types consistency assertions).",
                        attributeName, annotatedPropertyName, dependentProperty.Name));
        }

        public static void ConsistentTypes(PropertyInfo dependentProperty, PropertyInfo targetProperty, string annotatedPropertyName, string attributeName)
        {
            if (!ConsistentTypes(dependentProperty.PropertyType, targetProperty.PropertyType))
                throw new InvalidOperationException(
                    string.Format(
                        "Type mismatch detected in {0} definition for {1} field. Types consistency is required for {2} field and its corresponding, " +
                        "provided indirectly through backend field, target value.",
                        attributeName, annotatedPropertyName, dependentProperty.Name));
        }

        private static bool ConsistentTypes(Type dependentType, Type targetType)
        {
            dependentType = dependentType.IsGenericType && dependentType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? Nullable.GetUnderlyingType(dependentType)
                : dependentType;
            targetType = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? Nullable.GetUnderlyingType(targetType)
                : targetType;

            return dependentType == targetType;            
        }
    }
}
