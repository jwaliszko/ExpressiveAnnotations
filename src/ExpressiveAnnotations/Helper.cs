/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressiveAnnotations
{
    internal static class Helper
    {
        public static void MakeTypesCompatible(Expression e1, Expression e2, out Expression oute1, out Expression oute2)
        {
            oute1 = e1;
            oute2 = e2;

            // promote numeric values to double - do all computations with higher precision (to be compatible with JavaScript, e.g. notation 1/2, should give 0.5 double not 0 int)
            if (oute1.Type != typeof (double) && oute1.Type != typeof (double?) && oute1.Type.IsNumeric())
                oute1 = oute1.Type.IsNullable()
                    ? Expression.Convert(oute1, typeof (double?))
                    : Expression.Convert(oute1, typeof (double));
            if (oute2.Type != typeof (double) && oute2.Type != typeof (double?) && oute2.Type.IsNumeric())
                oute2 = oute2.Type.IsNullable()
                    ? Expression.Convert(oute2, typeof (double?))
                    : Expression.Convert(oute2, typeof (double));

            // non-nullable operand is converted to nullable if necessary, and the lifted-to-nullable form of the comparison is used (C# rule, which is currently not followed by expression trees)
            if (oute1.Type.UnderlyingType() == oute2.Type.UnderlyingType())
            {
                if (oute1.Type.IsNullable() && !oute2.Type.IsNullable())
                    oute2 = Expression.Convert(oute2, oute1.Type);
                else if (!oute1.Type.IsNullable() && oute2.Type.IsNullable())
                    oute1 = Expression.Convert(oute1, oute2.Type);
            }

            // make DateTime and TimeSpan compatible (also do not care when first argument is TimeSpan and second DateTime because it is not allowed)
            if (oute1.Type.IsDateTime() && oute2.Type.IsTimeSpan())
            {
                if (oute1.Type.IsNullable() && !oute2.Type.IsNullable())
                    oute2 = Expression.Convert(oute2, typeof (TimeSpan?));
                else if (!oute1.Type.IsNullable() && oute2.Type.IsNullable())
                    oute1 = Expression.Convert(oute1, typeof (DateTime?));
            }
        }

        public static object ExtractValue(object source, string property)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (property == null)
                throw new ArgumentNullException("property");

            var props = property.Split('.');

            var prop = props[0];
            var type = source.GetType();
            var pi = type.GetProperty(prop);
            if (pi == null)
                throw new ArgumentException(
                    string.Format("Value extraction interrupted. Field {0} not found.", prop), property);

            source = pi.GetValue(source, null);
            for (var i = 1; i < props.Length; i++)
            {
                if (source == null)
                    throw new ArgumentException(
                        string.Format("Value extraction interrupted. Field {0} is null.", prop), property);

                prop = props[i];
                type = source.GetType();
                pi = type.GetProperty(prop);
                if (pi == null)
                    throw new ArgumentException(
                        string.Format("Value extraction interrupted. Field {0} not found.", prop), property);

                source = pi.GetValue(source, null);
            }

            return source;
        }

        public static string ExtractDisplayName(Type source, string property)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (property == null)
                throw new ArgumentNullException("property");

            var props = property.Split('.');

            var prop = props[0];
            var pi = source.GetProperty(prop);
            if (pi == null)
                throw new ArgumentException(
                    string.Format("Display name extraction interrupted. Field {0} not found.", prop), property);

            for (var i = 1; i < props.Length; i++)
            {
                prop = props[i];
                source = pi.PropertyType;
                pi = source.GetProperty(prop);
                if (pi == null)
                    throw new ArgumentException(
                        string.Format("Display name extraction interrupted. Field {0} not found.", prop), property);
            }

            var attrib = pi.GetAttributes<DisplayAttribute>().SingleOrDefault();
            if (attrib == null)
                throw new ArgumentException(
                    string.Format("No DisplayName attribute provided for {0} field.", prop), property);

            return attrib.GetName(); // instead of .Name, to work with resources
        }

        public static bool IsDateTime(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type == typeof (DateTime) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsDateTime());
        }

        public static bool IsTimeSpan(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type == typeof(TimeSpan) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsTimeSpan());
        }

        public static bool IsBool(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type == typeof (bool) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsBool());
        }

        public static bool IsGuid(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type == typeof (Guid) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsGuid());
        }

        public static bool IsString(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type == typeof (string);
        }

        public static bool IsNumeric(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var numericTypes = new HashSet<TypeCode>
            {
                TypeCode.SByte,     //sbyte
                TypeCode.Byte,      //byte
                TypeCode.Int16,     //short
                TypeCode.UInt16,    //ushort
                TypeCode.Int32,     //int
                TypeCode.UInt32,    //uint
                TypeCode.Int64,     //long
                TypeCode.UInt64,    //ulong
                TypeCode.Single,    //float
                TypeCode.Double,    //double
                TypeCode.Decimal    //decimal
            };
            return numericTypes.Contains(Type.GetTypeCode(type)) ||
                   type.IsNullable() && Nullable.GetUnderlyingType(type).IsNumeric();
        }

        public static bool IsNullable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        public static bool IsObject(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return typeof (object) == type;
        }

        public static bool IsNonNullableValueType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return !type.IsNullable() && type.IsValueType;
        }

        public static bool IsNullLiteral(this Expression expr)
        {
            if (expr == null)
                throw new ArgumentNullException("expr");

            return "null".Equals(expr.ToString());
        }

        public static Type UnderlyingType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type.IsNullable() ? Nullable.GetUnderlyingType(type) : type;
        }        

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static string GetMemberNameFromDisplayAttribute(this Type type, string displayName)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (displayName == null)
                throw new ArgumentNullException("displayName");
            
            // get member name from display attribute (if such an attribute exists) based on display name
            var props = type.GetProperties()
                .Where(p => p.GetAttributes<DisplayAttribute>().Any(a => a.GetName() == displayName))
                .Select(p => p.Name).ToList();

            // if there is an ambiguity, return nothing
            return props.Count == 1 ? props.SingleOrDefault() : null;
        }

        public static IEnumerable<T> GetAttributes<T>(this MemberInfo element) where T : Attribute
        {
#if NET40
            return element.GetCustomAttributes(typeof (T), false).Cast<T>();
#else
            return element.GetCustomAttributes<T>(false);
#endif
        }

        public static string TrimStart(this string input, out int line, out int column)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            var output = input.TrimStart();
            var redundancy = input.RemoveSuffix(output);
            var lastLineBreak = redundancy.LastIndexOf('\n');
            column = lastLineBreak > 0
                ? redundancy.Length - lastLineBreak
                : input.Length - output.Length;
            line = redundancy.CountLineBreaks();
            return output;
        }

        public static string Substring(this string input, int start, out int line, out int column)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (start < 0)
                throw new ArgumentOutOfRangeException("start", "Start index can not be negative.");

            var output = input.Substring(start);
            var redundancy = input.RemoveSuffix(output);
            var lastLineBreak = redundancy.LastIndexOf('\n');
            column = lastLineBreak > 0
                ? redundancy.Length - lastLineBreak
                : input.Length - output.Length;
            line = redundancy.CountLineBreaks();
            return output;
        }

        public static string RemoveSuffix(this string input, string suffix)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (suffix == null)
                throw new ArgumentNullException("suffix");

            return input.EndsWith(suffix)
                ? input.Substring(0, input.Length - suffix.Length)
                : input;
        }

        public static int CountLineBreaks(this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            return input.Count(n => n == '\n');
        }

        public static string TakeLine(this string input, int index)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Line indexes are 0-based.");

            return input.Split('\n').Skip(index).First().TrimEnd();
        }

        public static string ToOrdinal(this int num)
        {
            if (num <= 0)
                return num.ToString(CultureInfo.InvariantCulture);

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

        public static string Indicator(this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            return string.Format(
                @"
... {0} ...
    ^--- ", input);
        }
    }
}
