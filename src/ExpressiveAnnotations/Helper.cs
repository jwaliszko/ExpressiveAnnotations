using System;
using System.Collections.Generic;
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

            // promote numeric values to double - do all computations with higher precision (to be compatible with javascript, e.g. notation 1/2, should give 0.5 double not 0 int)
            if (oute1.Type != typeof(double) && oute1.Type != typeof(double?) && oute1.Type.IsNumeric())
                oute1 = oute1.Type.IsNullable()
                    ? Expression.Convert(oute1, typeof (double?))
                    : Expression.Convert(oute1, typeof (double));
            if (oute2.Type != typeof(double) && oute2.Type != typeof(double?) && oute2.Type.IsNumeric())
                oute2 = oute2.Type.IsNullable()
                    ? Expression.Convert(oute2, typeof(double?))
                    : Expression.Convert(oute2, typeof(double));

            //Attempt to convert strings to Guids
            if (oute1.Type.IsGuid() && oute2.Type == typeof(string))
                oute2 = Expression.New(typeof(Guid).GetConstructor(new[] { typeof(string) }), oute2);

            // non-nullable operand is converted to nullable if necessary, and the lifted-to-nullable form of the comparison is used (C# rule, which is currently not followed by expression trees)
            if (oute1.Type.IsNullable() && !oute2.Type.IsNullable())
                oute2 = Expression.Convert(oute2, oute1.Type);
            else if (!oute1.Type.IsNullable() && oute2.Type.IsNullable())
                oute1 = Expression.Convert(oute1, oute2.Type);

        }

        public static bool IsGuid(this Type type)
        {
            return type != null && (type == typeof(Guid) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsGuid()));
        }

        public static bool IsNumeric(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:    //sbyte
                case TypeCode.Byte:     //byte
                case TypeCode.Int16:    //short
                case TypeCode.UInt16:   //ushort
                case TypeCode.Int32:    //int
                case TypeCode.UInt32:   //uint
                case TypeCode.Int64:    //long
                case TypeCode.UInt64:   //ulong
                case TypeCode.Single:   //float
                case TypeCode.Double:   //double
                case TypeCode.Decimal:  //decimal
                    return true;
                case TypeCode.Object:
                    return type.IsNullable() && Nullable.GetUnderlyingType(type).IsNumeric();
                default:
                    return false;
            }
        }

        public static bool IsNullable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type ToNullable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return typeof (Nullable<>).MakeGenericType(type);
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
    }
}
