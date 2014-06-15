using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.Misc
{
    /// <summary>
    /// Helper class containing various utility methods.
    /// </summary>
    public static class MiscHelper
    {
        /// <summary>
        /// Tries to extract the name if given inside square brackets in string source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// Extracted property name.
        /// </returns>
        public static bool TryExtractName(object source, out string name)
        {
            name = null;
            var value = source as string;
            if (value != null)
            {
                var match = Regex.Match(value, @"^\[(.+)\]$");
                if (match.Success)
                    name = match.Groups[1].Value;
                return match.Success;
            }
            return false;
        }

        /// <summary>
        /// Extracts the property from given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="property">The property.</param>
        /// <returns>
        /// Extracted property.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// type
        /// or
        /// property
        /// </exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static PropertyInfo ExtractProperty(Type type, string property)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if(property == null)
                throw new ArgumentNullException("property");

            var props = property.Split('.');
            PropertyInfo pi = null;
            foreach (var prop in props)
            {
                pi = type.GetProperty(prop);
                if (pi == null)
                    throw new ArgumentException(string.Format("Dynamic extraction interrupted. Field {0} not found.", prop), property);
                type = pi.PropertyType;
            }
            return pi;
        }

        /// <summary>
        /// Extracts the value of property from given source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="property">The property.</param>
        /// <returns>
        /// Extracted property value.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// property
        /// or
        /// Nested field value dynamic extraction interrupted.
        /// </exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static object ExtractValue(object source, string property)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (property == null)
                throw new ArgumentNullException("property");

            var props = property.Split('.');
            for (var i = 0; i < props.Length; i++)
            {
                var type = source.GetType();
                var prop = props[i];
                var pi = type.GetProperty(prop);
                if (pi == null)
                    throw new ArgumentException(string.Format("Dynamic extraction interrupted. Field {0} not found.", prop), property);
                source = pi.GetValue(source, null);
                if (source == null && i < props.Length - 1) // check for nulls except last loop (final value can be null)
                    throw new ArgumentNullException(pi.Name, "Nested field value dynamic extraction interrupted.");
            }
            return source;
        }

        /// <summary>
        /// Prepares expression to be shown in user-friendly form.
        /// </summary>
        /// <param name="expression">The logical expression.</param>
        /// <param name="dependentProperties">The dependent properties.</param>
        /// <param name="targetValues">The target values.</param>
        /// <param name="relationalOperators">The relational operators.</param>
        /// <returns>
        /// Expression processed to user-friendly form.
        /// </returns>
        public static string ComposeExpression(string expression, string[] dependentProperties, object[] targetValues, string[] relationalOperators)
        {
            var count = dependentProperties.Length;
            var operands = new object[count];
            for (var i = 0; i < count; i++)
            {
                operands[i] = string.Format("({0})", ComposeRelationalExpression(dependentProperties[i], targetValues[i], relationalOperators.Any() ? relationalOperators[i] : "=="));
            }
            return string.Format(expression, operands);
        }

        /// <summary>
        /// Prepares single relational expression to be shown in user-friendly form.
        /// </summary>
        /// <param name="dependentProperty">The dependent property.</param>
        /// <param name="targetValue">The target value.</param>
        /// <param name="relationalOperator">The relational operator.</param>
        /// <returns></returns>
        public static string ComposeRelationalExpression(string dependentProperty, object targetValue, string relationalOperator)
        {
            string name;
            var target = targetValue;
            if (!TryExtractName(target, out name)) // if target value does not containan encapsulated property name, beautify it
                target = (target is string && (string)target != "*") ? string.Format("\"{0}\"", target) : target ?? "null";
            target = (target is bool) ? target.ToString().ToLowerInvariant() : target;
            return string.Format("{0} {1} {2}", dependentProperty, relationalOperator ?? "==", target);
        }
    }
}
