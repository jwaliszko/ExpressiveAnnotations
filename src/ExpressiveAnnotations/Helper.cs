using System;
using System.Linq.Expressions;

namespace ExpressiveAnnotations
{
    internal class Helper
    {
        public static void MakeTypesCompatible(Expression e1, Expression e2, out Expression oute1, out Expression oute2)
        {
            oute1 = e1;
            oute2 = e2;

            // non-nullable operand is converted to nullable if necessary, and the lifted-to-nullable form of the comparison is used (c# rule which is currently not followed by expression trees)
            if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
                oute2 = Expression.Convert(e2, e1.Type);
            else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
                oute1 = Expression.Convert(e1, e2.Type);
        }

        public static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
