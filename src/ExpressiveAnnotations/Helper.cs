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

            // convert enum values to their underlying types
            if (e1.Type.IsEnum)
                oute1 = Expression.Convert(e1, Enum.GetUnderlyingType(e1.Type));
            else if (e1.Type.IsNullable() && Nullable.GetUnderlyingType(e1.Type).IsEnum)
                oute1 = Expression.Convert(e1, Enum.GetUnderlyingType(Nullable.GetUnderlyingType(e1.Type)).ToNullable());
            if (e2.Type.IsEnum)
                oute2 = Expression.Convert(e2, Enum.GetUnderlyingType(e2.Type));
            else if (e2.Type.IsNullable() && Nullable.GetUnderlyingType(e2.Type).IsEnum)
                oute2 = Expression.Convert(e2, Enum.GetUnderlyingType(Nullable.GetUnderlyingType(e2.Type)).ToNullable());

            // promote int to double - do all computations with higher precision (to be compatible with javascript, e.g. notation 1/2, should give 0.5 double not 0 int)
            if ((oute1.Type == typeof(int)) || (oute1.Type == typeof(byte)))
                oute1 = Expression.Convert(oute1, typeof(double));
            if ((oute2.Type == typeof(int)) || (oute2.Type == typeof(byte)))
                oute2 = Expression.Convert(oute2, typeof(double));

            // non-nullable operand is converted to nullable if necessary, and the lifted-to-nullable form of the comparison is used (C# rule, which is currently not followed by expression trees)
            if (oute1.Type.IsNullable() && !oute2.Type.IsNullable())
                oute2 = Expression.Convert(oute2, oute1.Type);
            else if (!oute1.Type.IsNullable() && oute2.Type.IsNullable())
                oute1 = Expression.Convert(oute1, oute2.Type);
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
