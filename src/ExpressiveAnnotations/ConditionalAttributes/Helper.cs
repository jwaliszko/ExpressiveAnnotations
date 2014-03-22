using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    internal class Helper
    {
        internal static object FetchTargetValue(object targetValue, ValidationContext validationContext)
        {
            var value = targetValue as string;
            if (value != null)
            {
                var match = Regex.Match(value, @"^\[(.+)\]$");
                if (match.Success)
                {
                    var fieldName = match.Groups[1].Value;
                    return ExtractValue(validationContext.ObjectInstance, fieldName);
                }
            }
            return targetValue;
        }

        internal static bool Compare(object dependentValue, object targetValue)
        {
            return Equals(dependentValue, targetValue)
                   || (dependentValue is string
                       && targetValue is string
                       && string.Equals(((string) dependentValue).Trim(), ((string) targetValue).Trim()))
                   || (dependentValue != null
                       && targetValue is string
                       && string.Equals((string) targetValue, "*"));
        }

        internal static PropertyInfo ExtractProperty(object source, string property)
        {
            var props = property.Split('.');
            var type = source.GetType();
            PropertyInfo pi = null;
            foreach (var prop in props)
            {
                pi = type.GetProperty(prop);
                if (pi == null)
                {
                    throw new ArgumentException(string.Format("Field \"{0}\" not found.", prop));
                }
                type = pi.PropertyType;
            }
            return pi;
        }

        internal static object ExtractValue(object source, string property)
        {
            var props = property.Split('.');
            var type = source.GetType();
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (var prop in props)
            {
                var pi = type.GetProperty(prop);
                if (pi == null)
                {
                    throw new ArgumentException(string.Format("Field \"{0}\" not found.", prop));
                }
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(source.GetType(), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var compiledLambda = lambda.Compile();
            var value = compiledLambda.DynamicInvoke(source);
            return value;
        }
    }
}
