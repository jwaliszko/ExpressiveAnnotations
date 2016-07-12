using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     Adjusts types compatibility for operations.
    /// </summary>
    internal static class TypeAdapter
    {
        public static void MakeTypesCompatible(Expression e1, Expression e2, out Expression oute1, out Expression oute2, TokenType operation)
        {
            Debug.Assert(e1 != null);
            Debug.Assert(e2 != null);

            oute1 = e1;
            oute2 = e2;

            if (oute1.Type.IsEnum && oute2.Type.IsEnum
                && oute1.Type.UnderlyingType() != oute2.Type.UnderlyingType()) // various enum types
                return;

            if (oute1.Type == typeof (string) && oute2.Type == typeof (char)) // convert char to string
                oute2 = Expression.Call(oute2, typeof (object).GetMethod("ToString"));
            else if (oute1.Type == typeof (char) && oute2.Type == typeof (string))
                oute1 = Expression.Call(oute1, typeof (object).GetMethod("ToString"));

            if (operation != TokenType.DIV // do not promote integral numeric values to double - exception for division operation, e.g. 1/2 should evaluate to 0.5 double like in JS
                && !oute1.Type.IsEnum && !oute2.Type.IsEnum
                && oute1.Type.IsNumeric() && oute2.Type.IsNumeric()
                && !oute1.Type.IsFloatingPointNumeric() && !oute2.Type.IsFloatingPointNumeric())
                return;

            // promote numeric values to double - do computations with higher precision (to be compatible with JavaScript, e.g. 1/2 should evaluate to 0.5 double not 0 int)
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
    }
}
