using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    internal static class Helper
    {
        public static IDictionary<string, string> GetTypesMap(ModelMetadata metadata, string expression)
        {
            var lexer = new Lexer();
            var properties =
                lexer.Analyze(expression).Where(x => x.Id == TokenId.PROPERTY).Select(x => x.Value.ToString());
            var typesMap = new Dictionary<string, string>();
            foreach (var prop in properties)
            {
                var pi = ExtractProperty(metadata.ContainerType, prop);
                if (pi != null && !typesMap.ContainsKey(prop))
                    typesMap.Add(prop, GetCoarseType(pi.PropertyType));
            }
            return typesMap;
        }

        public static PropertyInfo ExtractProperty(Type type, string property)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (property == null)
                throw new ArgumentNullException("property");

            var props = property.Split('.');
            PropertyInfo pi = null;
            foreach (var prop in props)
            {
                pi = type.GetProperty(prop);
                if (pi == null)
                    return null;
                type = pi.PropertyType;
            }
            return pi;
        }

        public static bool IsNumeric(this object value)
        {
            return value != null && value.GetType().IsNumeric();
        }

        public static bool IsNumeric(this Type type)
        {
            if (type == null)
                return false;
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
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
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
            if (type == null)
                return false;
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

        public static bool IsString(this object value)
        {
            return value != null && value.GetType().IsString();
        }

        public static bool IsString(this Type type)
        {
            if (type == null)
                return false;
            return Type.GetTypeCode(type) == TypeCode.String;
        }

        public static bool IsBool(this object value)
        {
            return value != null && value.GetType().IsBool();
        }

        public static bool IsBool(this Type type)
        {
            if (type == null)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
                        return Nullable.GetUnderlyingType(type).IsBool();
                    return false;
                default:
                    return false;
            }
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
