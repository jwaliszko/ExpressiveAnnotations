using System;

namespace ExpressiveAnnotations.Misc
{
    /// <summary>
    /// Helper class containing methods related to types maintenance.
    /// </summary>
    internal static class TypeHelper
    {
        /// <summary>
        /// Determines whether the specified value is type of numeric (string representing a number does not count).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if type is numeric, false otherwise.</returns>
        public static bool IsNumeric(this object value)
        {
            return value != null && value.GetType().IsNumeric();
        }

        /// <summary>
        /// Determines whether the specified type is numeric.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True if type is numeric, false otherwise.</returns>
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
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return Nullable.GetUnderlyingType(type).IsNumeric();
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified value is type of DateTime.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if value is date time, false otherwise.</returns>
        public static bool IsDateTime(this object value)
        {
            return value != null && value.GetType().IsDateTime();
        }

        /// <summary>
        /// Determines whether the specified type is DateTime.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True if type is date time, false otherwise.</returns>
        public static bool IsDateTime(this Type type)
        {
            if (type == null)
                return false;
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

        /// <summary>
        /// Determines whether the specified value is type of string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if value is string, false otherwise.</returns>
        public static bool IsString(this object value)
        {
            return value != null && value.GetType().IsString();
        }

        /// <summary>
        /// Determines whether the specified type is string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True if type is string, false otherwise.</returns>
        public static bool IsString(this Type type)
        {
            if (type == null)
                return false;
            return Type.GetTypeCode(type) == TypeCode.String;
        }

        /// <summary>
        /// Determines whether the specified value is type of bool.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if value is bool, false otherwise.</returns>
        public static bool IsBool(this object value)
        {
            return value != null && value.GetType().IsBool();
        }

        /// <summary>
        /// Determines whether the specified type is bool.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True if type is bool, false otherwise.</returns>
        public static bool IsBool(this Type type)
        {
            if (type == null)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return Nullable.GetUnderlyingType(type).IsBool();
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the coarse type name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Coarse type name: datetime, numeric, string, bool or complex.</returns>
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
