using System;
using System.Reflection;

namespace ExpressiveAnnotations.Misc
{
    /// <summary>
    /// Assertions class.
    /// </summary>
    internal static class Assert
    {
        /// <summary>
        /// Forces the dependent property and target object to represent exact types.
        /// This requirement doesn't apply when taret value is null or asterisk.
        /// </summary>
        /// <param name="dependentProperty">The dependent property.</param>
        /// <param name="targetValue">The target value.</param>
        /// <param name="annotatedPropertyName">Name of the annotated property.</param>
        /// <param name="relationalOperator">The relational operator.</param>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        public static void ConsistentTypes(PropertyInfo dependentProperty, object targetValue, string annotatedPropertyName, string relationalOperator)
        {
            if (targetValue == null || string.Equals(targetValue as string, "*")) // type doesn't matter when null or asterisk is provided into target value
                return;

            DateTime date;
            if (dependentProperty.PropertyType.IsDateTime() && DateTime.TryParse(targetValue as string, out date)) // target date is allowed to be explicitly provided as string
                return;

            if (!ConsistentTypes(dependentProperty.PropertyType, targetValue.GetType()))
                throw new InvalidOperationException(
                    string.Format(
                        "Type mismatch detected in attribute definition for {0} field. Types consistency is required for {1} field and its corresponding, " +
                        "explicitly provided, target value (explicit target values, if null or *, does not interfere with types consistency assertions).",
                        annotatedPropertyName, dependentProperty.Name));

            if (!OperationAllowedForType(dependentProperty.PropertyType, relationalOperator))
                throw new InvalidOperationException(
                    string.Format(
                        "Relation abuse detected in attribute definition for {0} field. The type of {1} field and its corresponding, explicitly provided, " +
                        "target value, is invalid for {2} relation. Greater than and less than relational operations not allowed for arguments " +
                        "of types other than: numeric, string or datetime.",
                        annotatedPropertyName, dependentProperty.Name, relationalOperator));
        }

        /// <summary>
        /// Forces the dependent and target properties to represent exact types.
        /// </summary>
        /// <param name="dependentProperty">The dependent property.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="annotatedPropertyName">Name of the annotated property.</param>
        /// <param name="relationalOperator">The relational operator.</param>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        public static void ConsistentTypes(PropertyInfo dependentProperty, PropertyInfo targetProperty, string annotatedPropertyName, string relationalOperator)
        {
            if (!ConsistentTypes(dependentProperty.PropertyType, targetProperty.PropertyType))
                throw new InvalidOperationException(
                    string.Format(
                        "Type mismatch detected in attribute definition for {0} field. Types consistency is required for {1} field and its corresponding, " +
                        "provided indirectly through backing field, target value.",
                        annotatedPropertyName, dependentProperty.Name));

            if (!OperationAllowedForType(dependentProperty.PropertyType, relationalOperator))
                throw new InvalidOperationException(
                    string.Format(
                        "Relation abuse detected in attribute definition for {0} field. The type of {1} field and its corresponding, provided indirectly " +
                        "through backing field, target value, is invalid for {2} relation. Greater than and less than relational operations " +
                        "not allowed for arguments of types other than: numeric, string or datetime.",
                        annotatedPropertyName, dependentProperty.Name, relationalOperator));
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
