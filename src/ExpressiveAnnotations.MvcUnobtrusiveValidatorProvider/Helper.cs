using System;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    internal static class Helper
    {
        public static bool IsDateTime(this Type type)
        {
            return type != null && (type == typeof(DateTime) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsDateTime()));
        }

        public static bool IsBool(this Type type)
        {
            return type != null && (type == typeof(bool) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsBool()));
        }

        public static bool IsGuid(this Type type)
        {
            return type != null && (type == typeof(Guid) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsGuid()));
        }

        public static bool IsString(this Type type)
        {
            return type != null && type == typeof(string);
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
