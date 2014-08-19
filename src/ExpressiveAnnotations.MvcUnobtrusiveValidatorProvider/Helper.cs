using System;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    internal static class Helper
    {
        public static bool IsDateTime(this Type type)
        {
            if (type == null)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DateTime:
                    return true;
                case TypeCode.Object:
                    return type.IsNullable() && Nullable.GetUnderlyingType(type).IsDateTime();
                default:
                    return false;
            }
        }

        public static bool IsString(this Type type)
        {
            return type != null && Type.GetTypeCode(type) == TypeCode.String;
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
                    return type.IsNullable() && Nullable.GetUnderlyingType(type).IsBool();
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
            if (type.IsGuid())
                return "guid";

            return "complex";
        }
    }
}
