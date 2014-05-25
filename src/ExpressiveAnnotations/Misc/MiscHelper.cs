using System;
using System.Linq;
using System.Linq.Expressions;
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
        /// <returns></returns>
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
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static PropertyInfo ExtractProperty(Type type, string property)
        {
            var props = property.Split('.');
            PropertyInfo pi = null;
            foreach (var prop in props)
            {
                pi = type.GetProperty(prop);
                if (pi == null)
                    throw new ArgumentException(string.Format("Field {0} not found.", prop));
                type = pi.PropertyType;
            }
            return pi;
        }

        /// <summary>
        /// Extracts the value of property from given source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static object ExtractValue(object source, string property)
        {
            var props = property.Split('.');
            var type = source.GetType();
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (var prop in props)
            {
                var pi = type.GetProperty(prop);
                if (pi == null)
                    throw new ArgumentException(string.Format("Field {0} not found.", prop));
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(source.GetType(), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var compiledLambda = lambda.Compile();
            var value = compiledLambda.DynamicInvoke(source);
            return value;
        }

        /// <summary>
        /// Composes the expression into the printable, user friendly form.
        /// </summary>
        /// <param name="expression">The logical expression.</param>
        /// <param name="dependentProperties">The dependent properties.</param>
        /// <param name="targetValues">The target values.</param>
        /// <param name="relationalOperators">The relational operators.</param>
        /// <returns></returns>
        public static string ComposeExpression(string expression, string[] dependentProperties, object[] targetValues, string[] relationalOperators)
        {
            var count = dependentProperties.Length;
            var operands = new object[count];
            for (var i = 0; i < count; i++)
            {
                string name;
                var target = targetValues[i];
                if (!TryExtractName(target, out name)) // if target value does not containan encapsulated property name, beautify it
                    target = (target is string && (string)target != "*") ? string.Format("\"{0}\"", target) : target ?? "null";
                target = (target is bool) ? target.ToString().ToLowerInvariant() : target;
                operands[i] = string.Format("({0} {1} {2})", dependentProperties[i], relationalOperators.Any() ? relationalOperators[i] : "==", target);
            }
            return string.Format(expression, operands);
        }
    }
}
