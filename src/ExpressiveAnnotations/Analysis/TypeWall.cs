/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     Demands types compatibility for operations.
    /// </summary>
    internal class TypeWall
    {
        public TypeWall(string expression)
        {
            Expr = expression;
        }

        private string Expr { get; set; }

        public void LOr(Expression arg1, Expression arg2, Token oper)
        {
            LOrLAnd(arg1, arg2, oper);
        }

        public void LAnd(Expression arg1, Expression arg2, Token oper)
        {
            LOrLAnd(arg1, arg2, oper);
        }

        private void LOrLAnd(Expression arg1, Expression arg2, Token oper)
        {
            AssertArgsNotNullLiterals(arg1, arg2, oper);
            if (!(arg1.Type.IsBool() && arg2.Type.IsBool()))
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{arg1.Type}' and '{arg2.Type}'.",
                    Expr, oper.Location);
        }

        public void BOr(Expression arg1, Expression arg2, Token oper)
        {
            BOrBAndXor(arg1, arg2, oper);
        }

        public void Xor(Expression arg1, Expression arg2, Token oper)
        {
            BOrBAndXor(arg1, arg2, oper);
        }

        public void BAnd(Expression arg1, Expression arg2, Token oper)
        {
            BOrBAndXor(arg1, arg2, oper);
        }

        private void BOrBAndXor(Expression arg1, Expression arg2, Token oper)
        {
            AssertArgsNotNullLiterals(arg1, arg2, oper);
            if (!(arg1.Type.IsBool() && arg2.Type.IsBool())
                && !(arg1.Type.IsInteger() && arg2.Type.IsInteger()))
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{arg1.Type}' and '{arg2.Type}'.",
                    Expr, oper.Location);
        }

        public void Eq(Expression arg1, Expression arg2, Type type1, Type type2, Token oper)
        {
            if (arg1.Type != arg2.Type
                && !arg1.IsNullLiteral() && !arg2.IsNullLiteral()
                && !arg1.Type.IsObject() && !arg2.Type.IsObject())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.", Expr, oper.Location);

            if (type1.IsNonNullableValueType() && arg2.IsNullLiteral())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and 'null'.", Expr, oper.Location);
            if (arg1.IsNullLiteral() && type2.IsNonNullableValueType())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type 'null' and '{type2}'.", Expr, oper.Location);
        }

        public void Rel(Expression arg1, Expression arg2, Type type1, Type type2, Token oper)
        {
            if (arg1.Type != arg2.Type
                && !arg1.IsNullLiteral() && !arg2.IsNullLiteral()
                && !arg1.Type.IsObject() && !arg2.Type.IsObject())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.", Expr, oper.Location);

            AssertArgsNotNullLiterals(arg1, type1, arg2, type2, oper);
            if (!(arg1.Type.IsNumeric() && arg2.Type.IsNumeric())
                && !(arg1.Type.IsDateTime() && arg2.Type.IsDateTime())
                && !(arg1.Type.IsTimeSpan() && arg2.Type.IsTimeSpan()))
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.", Expr, oper.Location);
        }

        public void Shift(Expression arg1, Expression arg2, Token oper)
        {
            AssertArgsNotNullLiterals(arg1, arg2, oper);
            if (!(arg1.Type.IsInteger() && arg2.Type.IsInteger()))
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{arg1.Type}' and '{arg2.Type}'.", Expr, oper.Location);
        }

        public void Add(Expression arg1, Expression arg2, Type type1, Type type2, Token oper)
        {
            if (!arg1.Type.IsString() && !arg2.Type.IsString())
            {
                AssertArgsNotNullLiterals(arg1, type1, arg2, type2, oper);
                if (!(arg1.Type.IsNumeric() && arg2.Type.IsNumeric())
                    && !(arg1.Type.IsDateTime() && arg2.Type.IsDateTime())
                    && !(arg1.Type.IsTimeSpan() && arg2.Type.IsTimeSpan())
                    && !(arg1.Type.IsDateTime() && arg2.Type.IsTimeSpan()))
                    throw new ParseErrorException(
                        $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.", Expr, oper.Location);
            }

            switch (oper.Type)
            {
                case TokenType.ADD:
                    if (arg1.Type.IsDateTime() && arg2.Type.IsDateTime())
                        throw new ParseErrorException(
                            $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.", Expr, oper.Location);
                    break;
                default:
                    Debug.Assert(oper.Type == TokenType.SUB);
                    AssertArgsNotNullLiterals(arg1, type1, arg2, type2, oper);
                    if (arg1.Type.IsString() && arg2.Type.IsString())
                        throw new ParseErrorException(
                            $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.", Expr, oper.Location);
                    break;
            }
        }

        public void Mul(Expression arg1, Expression arg2, Token oper)
        {
            if (!(arg1.Type.IsNumeric() && arg2.Type.IsNumeric()))
            {
                AssertArgsNotNullLiterals(arg1, arg2, oper);
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{arg1.Type}' and '{arg2.Type}'.", Expr, oper.Location);
            }
        }

        public void Unary(Expression arg, Token oper)
        {
            if (arg.IsNullLiteral())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operand of type 'null'.", Expr, oper.Location);
            if (oper.Type == TokenType.L_NOT && !arg.Type.IsBool())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operand of type '{arg.Type}'.", Expr, oper.Location);
            if (oper.Type == TokenType.B_NOT && !arg.Type.IsBool() && !arg.Type.IsInteger())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operand of type '{arg.Type}'.", Expr, oper.Location);
            if (new[] {TokenType.ADD, TokenType.SUB}.Contains(oper.Type) && !arg.Type.IsNumeric() && !arg.Type.IsTimeSpan())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operand of type '{arg.Type}'.", Expr, oper.Location);
        }

        public void TypesMatch(Expression arg1, Expression arg2, Location loc)
        {
            if (arg1.Type != arg2.Type)
                throw new ParseErrorException(
                    "Argument types must match.", Expr, loc);
        }

        public void OfType<T>(Expression arg, Location loc)
        {
            if (arg.Type != typeof (T))
                throw new ParseErrorException(
                    $"Argument must be of type '{typeof (T)}'.", Expr, loc);
        }

        private void AssertArgsNotNullLiterals(Expression arg1, Expression arg2, Token oper)
        {
            AssertArgsNotNullLiterals(arg1, arg1.Type, arg2, arg2.Type, oper);
        }

        private void AssertArgsNotNullLiterals(Expression arg1, Type type1, Expression arg2, Type type2, Token oper)
        {
            if (arg1.IsNullLiteral() && arg2.IsNullLiteral())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type 'null' and 'null'.", Expr, oper.Location);
            if (!arg1.IsNullLiteral() && arg2.IsNullLiteral())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and 'null'.", Expr, oper.Location);
            if (arg1.IsNullLiteral() && !arg2.IsNullLiteral())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type 'null' and '{type2}'.", Expr, oper.Location);
        }
    }
}
