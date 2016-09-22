/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations
{
    internal interface ITypeProvider
    {
        IEnumerable<Type> GetTypes();
    }

    internal class AssemblyTypeProvider : ITypeProvider
    {
        public AssemblyTypeProvider(Assembly assembly)
        {
            Assembly = assembly;
        }

        private Assembly Assembly { get; set; }
        public IEnumerable<Type> GetTypes() { return Assembly.GetTypes(); }
    }

    internal static class Helper
    {
        public static object ExtractValue(object source, string property)
        {
            Debug.Assert(source != null);
            Debug.Assert(property != null);

            var props = property.Split('.');

            var prop = props[0];
            var type = source.GetType();
            var pi = type.GetProperty(prop);
            if (pi == null)
                throw new ArgumentException($"Value extraction interrupted. Field {prop} not found.", property);

            source = pi.GetValue(source, null);
            for (var i = 1; i < props.Length; i++)
            {
                if (source == null)
                    throw new ArgumentException($"Value extraction interrupted. Field {prop} is null.", property);

                prop = props[i];
                type = source.GetType();
                pi = type.GetProperty(prop);
                if (pi == null)
                    throw new ArgumentException($"Value extraction interrupted. Field {prop} not found.", property);

                source = pi.GetValue(source, null);
            }

            return source;
        }

        public static string ExtractDisplayName(Type source, string property)
        {
            Debug.Assert(source != null);
            Debug.Assert(property != null);

            var props = property.Split('.');

            var prop = props[0];
            var pi = source.GetProperty(prop);
            if (pi == null)
                throw new ArgumentException($"Display name extraction interrupted. Field {prop} not found.", property);

            for (var i = 1; i < props.Length; i++)
            {
                prop = props[i];
                source = pi.PropertyType;
                pi = source.GetProperty(prop);
                if (pi == null)
                    throw new ArgumentException($"Display name extraction interrupted. Field {prop} not found.", property);
            }

            var displayAttrib = pi.GetAttributes<DisplayAttribute>().SingleOrDefault();
            if (displayAttrib == null)
            {
                var displayNameAttrib = pi.GetAttributes<DisplayNameAttribute>().SingleOrDefault();
                if (displayNameAttrib == null)
                    throw new ArgumentException($"No display name provided for {prop} field. Use either Display attribute or DisplayName attribute.", property);

                return displayNameAttrib.DisplayName;
            }

            return displayAttrib.GetName(); // instead of .Name, to work with resources
        }

        public static bool IsDateTime(this Type type)
        {
            Debug.Assert(type != null);

            return type == typeof (DateTime) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsDateTime());
        }

        public static bool IsTimeSpan(this Type type)
        {
            Debug.Assert(type != null);

            return type == typeof (TimeSpan) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsTimeSpan());
        }

        public static bool IsBool(this Type type)
        {
            Debug.Assert(type != null);

            return type == typeof (bool) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsBool());
        }

        public static bool IsGuid(this Type type)
        {
            Debug.Assert(type != null);

            return type == typeof (Guid) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsGuid());
        }

        public static bool IsString(this Type type)
        {
            Debug.Assert(type != null);

            return type == typeof (string);
        }

        public static bool IsNumeric(this Type type)
        {
            Debug.Assert(type != null);

            return type.IsIntegralNumeric() || type.IsFloatingPointNumeric();
        }

        public static bool IsIntegralNumeric(this Type type)
        {
            Debug.Assert(type != null);

            var numericTypes = new HashSet<TypeCode>
            {
                TypeCode.SByte,     //sbyte
                TypeCode.Byte,      //byte
                TypeCode.Int16,     //short
                TypeCode.UInt16,    //ushort
                TypeCode.Int32,     //int
                TypeCode.UInt32,    //uint
                TypeCode.Int64,     //long
                TypeCode.UInt64     //ulong
            };
            return numericTypes.Contains(Type.GetTypeCode(type)) ||
                   type.IsNullable() && Nullable.GetUnderlyingType(type).IsNumeric();
        }

        public static bool IsFloatingPointNumeric(this Type type)
        {
            Debug.Assert(type != null);

            var numericTypes = new HashSet<TypeCode>
            {
                TypeCode.Single,    //float (floating binary point type, e.g. 1001.101)
                TypeCode.Double,    //double (floating binary point type, e.g. 1001.101)
                TypeCode.Decimal    //decimal (floating decimal point type, e.g. 1234.567)
            };
            return numericTypes.Contains(Type.GetTypeCode(type)) ||
                   type.IsNullable() && Nullable.GetUnderlyingType(type).IsNumeric();
        }

        public static int HasHigherPrecisionThan(this Type type, Type other) // < 0 - lower precision, 0 - the same, > 0 - higher
        {
            Debug.Assert(type != null);
            Debug.Assert(other != null);
            Debug.Assert(type.IsIntegralNumeric());
            Debug.Assert(other.IsIntegralNumeric());

            var orderedTypes = new List<Type>
            {
                typeof (sbyte),
                typeof (byte),
                typeof (short),
                typeof (ushort),
                typeof (int),
                typeof (uint),
                typeof (long),
                typeof (ulong)
            };
            return orderedTypes.IndexOf(type.UnderlyingType()) - orderedTypes.IndexOf(other.UnderlyingType());
        }

        public static Type GetNullableEquivalent(this Type type)
        {
            Debug.Assert(type != null);
            Debug.Assert(type.IsIntegralNumeric());

            var map = new Dictionary<Type, Type>
            {
                {typeof (sbyte), typeof (sbyte?)},
                {typeof (byte), typeof (byte?)},
                {typeof (short), typeof (short?)},
                {typeof (ushort), typeof (ushort?)},
                {typeof (int), typeof (int?)},
                {typeof (uint), typeof (uint?)},
                {typeof (long), typeof (long?)},
                {typeof (ulong), typeof (ulong?)}
            };
            return map[type];
        }

        public static bool IsNullable(this Type type)
        {
            Debug.Assert(type != null);

            return type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        public static bool IsObject(this Type type)
        {
            Debug.Assert(type != null);

            return typeof (object) == type;
        }

        public static bool IsInteger(this Type type)
        {
            Debug.Assert(type != null);

            return typeof (int) == type;
        }

        public static bool IsNonNullableValueType(this Type type)
        {
            Debug.Assert(type != null);

            return !type.IsNullable() && type.IsValueType;
        }

        public static bool IsNullLiteral(this Expression expr)
        {
            Debug.Assert(expr != null);

            return "null".Equals(expr.ToString());
        }

        public static Type UnderlyingType(this Type type)
        {
            Debug.Assert(type != null);

            return type.IsNullable() ? Nullable.GetUnderlyingType(type) : type;
        }        

        public static IEnumerable<Type> GetLoadableTypes(this ITypeProvider provider)
        {
            Debug.Assert(provider != null);

            try
            {
                return provider.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static PropertyInfo GetPropertyByDisplayName(this Type type, string displayName)
        {
            Debug.Assert(type != null);
            Debug.Assert(displayName != null);

            return type.GetPropertyByDisplayAttribute(displayName)
                   ?? type.GetPropertyByDisplayNameAttribute(displayName);
        }

        public static PropertyInfo GetPropertyByDisplayAttribute(this Type type, string displayName)
        {
            Debug.Assert(type != null);
            Debug.Assert(displayName != null);

            // get member name through Display attribute (if such an attribute exists) based on display name
            var props = type.GetProperties()
                .Where(p => p.GetAttributes<DisplayAttribute>().Any(a => a.GetName() == displayName))
                .ToList();

            // if there is an ambiguity, return nothing
            return props.Count == 1 ? props.Single() : null;
        }

        public static PropertyInfo GetPropertyByDisplayNameAttribute(this Type type, string displayName)
        {
            Debug.Assert(type != null);
            Debug.Assert(displayName != null);

            // get member name through DisplayName attribute (if such an attribute exists) based on display name
            var props = type.GetProperties()
                .Where(p => p.GetAttributes<DisplayNameAttribute>().Any(a => a.DisplayName == displayName))
                .ToList();

            // if there is an ambiguity, return nothing
            return props.Count == 1 ? props.Single() : null;
        }

        public static IEnumerable<T> GetAttributes<T>(this MemberInfo element) where T : Attribute
        {
            Debug.Assert(element != null);

#if NET40
            return element.GetCustomAttributes(typeof (T), false).Cast<T>();
#else
            return element.GetCustomAttributes<T>(false);
#endif
        }

        public static string TrimStart(this string input, out int line, out int column)
        {
            Debug.Assert(input != null);

            var output = input.TrimStart();
            input.ComputeContactLocation(output, out line, out column);
            return output;
        }

        public static string Substring(this string input, int start, out int line, out int column)
        {
            Debug.Assert(input != null);
            Debug.Assert(start >= 0);

            var output = input.Substring(start);
            input.ComputeContactLocation(output, out line, out column);
            return output;
        }

        public static void ComputeContactLocation(this string input, string output, out int line, out int column)
        {
            Debug.Assert(input != null);
            Debug.Assert(output != null);            

            var redundancy = input.RemoveSuffix(output);
            var lastLineBreak = redundancy.LastIndexOf('\n');
            column = lastLineBreak > 0
                ? redundancy.Length - lastLineBreak
                : input.Length - output.Length;
            line = redundancy.CountLineBreaks();
        }

        public static string RemoveSuffix(this string input, string suffix)
        {
            Debug.Assert(input != null);
            Debug.Assert(suffix != null);

            return input.Substring(0, input.Length - suffix.Length);
        }

        public static int CountLineBreaks(this string input)
        {
            Debug.Assert(input != null);

            return input.Count(n => n == '\n');
        }

        public static string TakeLine(this string input, int index)
        {
            Debug.Assert(input != null);
            Debug.Assert(index >= 0);

            var lines = input.Split('\n');
            Debug.Assert(lines.Length > index);

            return lines.Skip(index).First();
        }

        public static string ToOrdinal(this int num)
        {
            Debug.Assert(num > 0);

            switch (num%100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num%10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }

        public static Location Clone(this Location location)
        {
            Debug.Assert(location != null);

            return new Location(location.Line, location.Column);
        }

        public static string BuildParseError(this Location location, string message, string expression)
        {
            Debug.Assert(location != null);
            Debug.Assert(message != null);
            Debug.Assert(expression != null);

            var line = expression.TakeLine(location.Line - 1);
            var suffix = line.Substring(location.Column - 1);

            return suffix.Length == 0
                ? $"Parse error on line {location.Line}, last column: {message}"
                : $"Parse error on line {location.Line}, column {location.Column}:{suffix.Indicator(100)}{message}";
        }

        public static int Position(this Location location, string expression)
        {
            return Enumerable.Range(0, location.Line - 1).Sum(i => expression.TakeLine(i).Length + 1) + location.Column - 1;
        }

        public static string Indicator(this string input, int max)
        {
            Debug.Assert(input != null);

            return $@"
... {input.Substring(0, input.Length > max ? max : input.Length)} ...
    ^--- ";
        }
    }
}
