using System;
using System.Linq.Expressions;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     An <see cref="Expression" /> wrapper.
    /// </summary>
    /// <remarks>
    ///     Provides some additional type checks and unburdens parser form this excessive logic.
    /// </remarks>
    internal class Expr
    {
        public Expr(string expression)
        {
            ExprString = expression;
            Wall = new TypeWall(expression);
        }

        private string ExprString { get; set; }
        private TypeWall Wall { get; set; }

        public Expression OrElse(Expression arg1, Expression arg2, Token oper)
        {
            Wall.LOr(arg1, arg2, oper);

            try
            {
                return Expression.OrElse(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, arg1.Type, arg2.Type), ExprString, oper.Location);
            }
        }

        public Expression AndAlso(Expression arg1, Expression arg2, Token oper)
        {
            Wall.LAnd(arg1, arg2, oper);

            try
            {
                return Expression.AndAlso(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, arg1.Type, arg2.Type), ExprString, oper.Location);
            }
        }

        public Expression Or(Expression arg1, Expression arg2, Token oper)
        {
            Wall.BOr(arg1, arg2, oper);

            try
            {
                return Expression.Or(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, arg1.Type, arg2.Type), ExprString, oper.Location);
            }
        }

        public Expression ExclusiveOr(Expression arg1, Expression arg2, Token oper)
        {
            Wall.Xor(arg1, arg2, oper);

            try
            {
                return Expression.ExclusiveOr(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, arg1.Type, arg2.Type), ExprString, oper.Location);
            }
        }

        public Expression And(Expression arg1, Expression arg2, Token oper)
        {
            Wall.BAnd(arg1, arg2, oper);

            try
            {
                return Expression.And(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, arg1.Type, arg2.Type), ExprString, oper.Location);
            }
        }

        public Expression Equal(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);
            Wall.Eq(arg1, arg2, type1, type2, oper);

            try
            {
                return Expression.Equal(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression NotEqual(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);
            Wall.Eq(arg1, arg2, type1, type2, oper);

            try
            {
                return Expression.NotEqual(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression LessThan(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);
            Wall.Rel(arg1, arg2, type1, type2, oper);

            try
            {
                return Expression.LessThan(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression LessThanOrEqual(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);
            Wall.Rel(arg1, arg2, type1, type2, oper);

            try
            {
                return Expression.LessThanOrEqual(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression GreaterThan(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);
            Wall.Rel(arg1, arg2, type1, type2, oper);

            try
            {
                return Expression.GreaterThan(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression GreaterThanOrEqual(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);
            Wall.Rel(arg1, arg2, type1, type2, oper);

            try
            {
                return Expression.GreaterThanOrEqual(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression LeftShift(Expression arg1, Expression arg2, Token oper)
        {
            Wall.Shift(arg1, arg2, oper);

            try
            {
                return Expression.LeftShift(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, arg1.Type, arg2.Type), ExprString, oper.Location);
            }
        }

        public Expression RightShift(Expression arg1, Expression arg2, Token oper)
        {
            Wall.Shift(arg1, arg2, oper);

            try
            {
                return Expression.RightShift(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, arg1.Type, arg2.Type), ExprString, oper.Location);
            }
        }

        public Expression Add(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);
            Wall.Add(arg1, arg2, type1, type2, oper);

            try
            {
                return Expression.Add(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression Subtract(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);
            Wall.Add(arg1, arg2, type1, type2, oper);

            try
            {
                return Expression.Subtract(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression Multiply(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);
            Wall.Mul(arg1, arg2, type1, type2, oper);            

            try
            {
                return Expression.Multiply(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression Divide(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            Wall.Mul(arg1, arg2, type1, type2, oper);
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);

            try
            {
                return Expression.Divide(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression Modulo(Expression arg1, Expression arg2, Token oper)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;
            Wall.Mul(arg1, arg2, type1, type2, oper);
            TypeAdapter.MakeTypesCompatible(arg1, arg2, out arg1, out arg2, oper.Type);

            try
            {
                return Expression.Modulo(arg1, arg2);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypesError(oper, type1, type2), ExprString, oper.Location);
            }
        }

        public Expression UnaryPlus(Expression arg, Token oper)
        {
            Wall.Unary(arg, oper);

            try
            {
                return Expression.UnaryPlus(arg);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypeError(oper, arg.Type), ExprString, oper.Location);
            }
        }

        public Expression Negate(Expression arg, Token oper)
        {
            Wall.Unary(arg, oper);

            try
            {
                return Expression.Negate(arg);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypeError(oper, arg.Type), ExprString, oper.Location);
            }
        }

        public Expression Not(Expression arg, Token oper)
        {
            Wall.Unary(arg, oper);

            try
            {
                return Expression.Not(arg);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypeError(oper, arg.Type), ExprString, oper.Location);
            }
        }

        public Expression OnesComplement(Expression arg, Token oper)
        {
            Wall.Unary(arg, oper);

            try
            {
                return Expression.OnesComplement(arg);
            }
            catch
            {
                throw new ParseErrorException(MakeInvalidTypeError(oper, arg.Type), ExprString, oper.Location);
            }
        }

        public Expression Condition(Expression arg1, Expression arg2, Expression arg3, Token start, Token oper)
        {
            Wall.OfType<bool>(arg1, start.Location);
            Wall.TypesMatch(arg2, arg3, oper.Location);

            return Expression.Condition(arg1, arg2, arg3);
        }

        private string MakeInvalidTypesError(Token oper, Type type1, Type type2)
        {
            return $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.";
        }

        private string MakeInvalidTypeError(Token oper, Type type)
        {
            return $"Operator '{oper.Value}' cannot be applied to operand of type '{type}'.";
        }
    }
}
