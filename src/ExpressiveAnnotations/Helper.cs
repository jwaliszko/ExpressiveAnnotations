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

            // non-nullable operand is converted to nullable if necessary, and the lifted-to-nullable form of the comparison is used (C# rule, which is currently not followed by expression trees)
            if (oute1.Type.IsNullable() && !oute2.Type.IsNullable())
                oute2 = Expression.Convert(oute2, oute1.Type);
            else if (!oute1.Type.IsNullable() && oute2.Type.IsNullable())
                oute1 = Expression.Convert(oute1, oute2.Type);
        }

        public static bool IsNumeric(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var numericTypes = new HashSet<TypeCode>
            {
                TypeCode.SByte,    //sbyte
                TypeCode.Byte,     //byte
                TypeCode.Int16,    //short
                TypeCode.UInt16,   //ushort
                TypeCode.Int32,    //int
                TypeCode.UInt32,   //uint
                TypeCode.Int64,    //long
                TypeCode.UInt64,   //ulong
                TypeCode.Single,   //float
                TypeCode.Double,   //double
                TypeCode.Decimal   //decimal
            };
            return numericTypes.Contains(Type.GetTypeCode(type)) || type.IsNullable() && Nullable.GetUnderlyingType(type).IsNumeric();
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
