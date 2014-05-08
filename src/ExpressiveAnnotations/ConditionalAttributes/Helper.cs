using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    internal static class Helper
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

        internal static PropertyInfo ExtractProperty(object source, string property)
        {
            var props = property.Split('.');
            var type = source.GetType();
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

        internal static bool Compute(object dependentValue, object targetValue, string relationalOperator)
        {
            switch (relationalOperator)
            {
                case "==":
                    return Compare(dependentValue, targetValue);
                case "!=":
                    return !Compare(dependentValue, targetValue);
                case ">":
                    return Greater(dependentValue, targetValue);
                case ">=":
                    return !Less(dependentValue, targetValue);
                case "<":
                    return Less(dependentValue, targetValue);
                case "<=":
                    return !Greater(dependentValue, targetValue);
            }

            throw new ArgumentException(string.Format("Relational operator {0} is invalid. Select from: EQ (==), NE (!=), GT (>), GE (>=), LT (<), LE (<=).", relationalOperator));
        }

        internal static bool Compare(object dependentValue, object targetValue)
        {
            return Equals(dependentValue, targetValue)
                   || (dependentValue is string
                       && targetValue is string
                       && string.Equals(((string)dependentValue).Trim(), ((string)targetValue).Trim()))
                   || (dependentValue != null
                       && string.Equals(targetValue as string, "*"));
        }

        internal static bool Greater(object dependentValue, object targetValue)
        {
            if (dependentValue.IsNumeric() && targetValue.IsNumeric())
                return Convert.ToDouble(dependentValue) > Convert.ToDouble(targetValue);
            if (dependentValue.IsDateTime() && targetValue.IsDateTime())
                return Convert.ToDateTime(dependentValue) > Convert.ToDateTime(targetValue);
            if ((dependentValue == null || dependentValue is string) && (targetValue == null || targetValue is string))
                return string.CompareOrdinal(dependentValue as string, targetValue as string) > 0;
            if (dependentValue == null || targetValue == null)
                return false;
            
            throw new InvalidOperationException("Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or date time.");
        }

        internal static bool Less(object dependentValue, object targetValue)
        {
            if (dependentValue.IsNumeric() && targetValue.IsNumeric())
                return Convert.ToDouble(dependentValue) < Convert.ToDouble(targetValue);
            if (dependentValue.IsDateTime() && targetValue.IsDateTime())
                return Convert.ToDateTime(dependentValue) < Convert.ToDateTime(targetValue);
            if ((dependentValue == null || dependentValue is string) && (targetValue == null || targetValue is string))
                return string.CompareOrdinal(dependentValue as string, targetValue as string) < 0;
            if (dependentValue == null || targetValue == null)
                return false;

            throw new InvalidOperationException("Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or date time.");
        }

        internal static bool IsNumeric(this object value)
        {
            return value != null && value.GetType().IsNumeric();
        }

        internal static bool IsNumeric(this Type type)
        {
            if (type == null) return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return Nullable.GetUnderlyingType(type).IsNumeric();
                    return false;
                default:
                    return false;
            }
        }

        internal static bool IsDateTime(this object value)
        {
            return value != null && value.GetType().IsDateTime();
        }

        internal static bool IsDateTime(this Type type)
        {
            if (type == null) return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DateTime:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
                        return Nullable.GetUnderlyingType(type).IsDateTime();
                    return false;
                default:
                    return false;
            }
        }        
    }
}
