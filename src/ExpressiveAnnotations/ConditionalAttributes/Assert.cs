using System;
using System.Reflection;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    public class Assert
    {
        public static void ConsistentTypes(PropertyInfo dependentProperty, object targetValue, string annotatedPropertyName, string attributeName, string relationalOperator)
        {
            if (targetValue == null || string.Equals(targetValue as string, "*"))
                return; // type doesn't matter when null or * is involved

            if (!ConsistentTypes(dependentProperty.PropertyType, targetValue.GetType()))
                throw new InvalidOperationException(
                    string.Format(
                        "Type mismatch detected in {0} definition for {1} field. Types consistency is required for {2} field and its corresponding, " +
                        "explicitly provided, target value (explicit target values, if null or *, does not interfere with types consistency assertions).",
                        attributeName, annotatedPropertyName, dependentProperty.Name));

            if (!OperationAllowedForType(dependentProperty.PropertyType, relationalOperator))
                throw new InvalidOperationException(
                    string.Format(
                        "Relation abuse detected in {0} definition for {1} field. The type of {2} field and its corresponding, explicitly provided, " +
                        "target value, is invalid for {3} relation. Greater than and less than relational operations not allowed for arguments " +
                        "of types other than: numeric, string or datetime.",
                        attributeName, annotatedPropertyName, dependentProperty.Name, relationalOperator));
        }

        public static void ConsistentTypes(PropertyInfo dependentProperty, PropertyInfo targetProperty, string annotatedPropertyName, string attributeName, string relationalOperator)
        {
            if (!ConsistentTypes(dependentProperty.PropertyType, targetProperty.PropertyType))
                throw new InvalidOperationException(
                    string.Format(
                        "Type mismatch detected in {0} definition for {1} field. Types consistency is required for {2} field and its corresponding, " +
                        "provided indirectly through backing field, target value.",
                        attributeName, annotatedPropertyName, dependentProperty.Name));

            if (!OperationAllowedForType(dependentProperty.PropertyType, relationalOperator))
                throw new InvalidOperationException(
                    string.Format(
                        "Relation abuse detected in {0} definition for {1} field. The type of {2} field and its corresponding, provided indirectly " +
                        "through backing field, target value, is invalid for {3} relation. Greater than and less than relational operations " +
                        "not allowed for arguments of types other than: numeric, string or datetime.",
                        attributeName, annotatedPropertyName, dependentProperty.Name, relationalOperator));
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

        private static bool OperationAllowedForType(Type type, string relationalOperator)
        {
            switch (relationalOperator)
            {
                case "==":
                case "!=":
                    return true;
                case ">":
                case ">=":
                case "<":
                case "<=":
                    return type.IsNumeric() || type.IsDateTime() || type.IsString();
            }

            throw new ArgumentException(string.Format("Relational operator {0} is invalid. Available operators: ==, !=, >, >=, <, <=.", relationalOperator));
        }
    }
}
