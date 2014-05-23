using System;

namespace ExpressiveAnnotations.Misc
{
    public static class TypeHelper
    {
        public static bool IsEmpty(this object value)
        {
            return value == null || (value is string && string.IsNullOrWhiteSpace((string)value));
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
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
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
            if (type == null) return false;
            return Type.GetTypeCode(type) == TypeCode.String;
        }

        public static bool IsBool(this object value)
        {
            return value != null && value.GetType().IsBool();
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
