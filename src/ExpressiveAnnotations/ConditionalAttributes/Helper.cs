using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    public static class Helper
    {
        public static bool TryExtractPropertyName(this object targetValue, out string property)
        {
            property = null;
            var value = targetValue as string;
            if (value != null)
            {
                var match = Regex.Match(value, @"^\[(.+)\]$");
                if (match.Success)
                    property = match.Groups[1].Value;
                return match.Success;
            }
            return false;
        }

        public static PropertyInfo ExtractProperty(object source, string property)
        {
            return ExtractProperty(source.GetType(), property);
        }

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

        public static bool Compute(object dependentValue, object targetValue, string relationalOperator)
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

            throw new ArgumentException(string.Format("Relational operator {0} is invalid. Available operators: ==, !=, >, >=, <, <=.", relationalOperator));
        }

        public static bool Compare(object dependentValue, object targetValue)
        {
            return Equals(dependentValue, targetValue)
                   || (dependentValue is string && targetValue is string
                       && string.Equals(((string) dependentValue).Trim(), ((string) targetValue).Trim()))
                   || (!dependentValue.IsEmpty() && string.Equals(targetValue as string, "*"))
                   || (dependentValue.IsEmpty() && targetValue.IsEmpty());
        }

        public static bool Greater(object dependentValue, object targetValue)
        {
            if (dependentValue.IsNumeric() && targetValue.IsNumeric())
                return Convert.ToDouble(dependentValue) > Convert.ToDouble(targetValue);
            if (dependentValue.IsDateTime() && targetValue.IsDateTime())
                return Convert.ToDateTime(dependentValue) > Convert.ToDateTime(targetValue);
            if ((dependentValue == null || dependentValue is string) && (targetValue == null || targetValue is string))
                return string.CompareOrdinal(dependentValue as string, targetValue as string) > 0;
            if (dependentValue == null || targetValue == null)
                return false;
            
            throw new InvalidOperationException("Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.");
        }

        public static bool Less(object dependentValue, object targetValue)
        {
            if (dependentValue.IsNumeric() && targetValue.IsNumeric())
                return Convert.ToDouble(dependentValue) < Convert.ToDouble(targetValue);
            if (dependentValue.IsDateTime() && targetValue.IsDateTime())
                return Convert.ToDateTime(dependentValue) < Convert.ToDateTime(targetValue);
            if ((dependentValue == null || dependentValue is string) && (targetValue == null || targetValue is string))
                return string.CompareOrdinal(dependentValue as string, targetValue as string) < 0;
            if (dependentValue == null || targetValue == null)
                return false;

            throw new InvalidOperationException("Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.");
        }

        public static bool IsEmpty(this object value)
        {
            return value == null || (value is string && string.IsNullOrEmpty((string)value));
        }

        public static bool IsNumeric(this object value)
        {
            return value != null && value.GetType().IsNumeric();
        }

        public static bool IsNumeric(this Type type)
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

        public static bool IsDateTime(this object value)
        {
            return value != null && value.GetType().IsDateTime();
        }

        public static bool IsDateTime(this Type type)
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

        public static bool IsString(this Type type)
        {
            if (type == null) return false;
            return Type.GetTypeCode(type) == TypeCode.String;
        }

        public static bool IsBool(this Type type)
        {
            if (type == null) return false;
            return Type.GetTypeCode(type) == TypeCode.Boolean;
        }

        public static string GetCoarseType(Type type)
        {
            if (type.IsDateTime())
                return "datetime";            
            if (type.IsNumeric())
                return "numeric";
            if (type.IsString())
                return "string";           
            if (type.IsBool())
                return "bool";

            return "complex";
        }
    }
}
