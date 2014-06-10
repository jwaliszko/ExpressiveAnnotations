using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ExpressiveAnnotations.LogicalExpressionsAnalysis.LexicalAnalysis;
using ExpressiveAnnotations.Misc;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis.SyntacticAnalysis
{
    /* EBNF GRAMMAR:
     * 
     * expression => or-exp
     * or-exp     => and-exp [ "||" or-exp ]
     * and-exp    => not-exp [ "&&" and-exp ]
     * not-exp    => [ "!" ] rel-exp
     * rel-exp    => val [ rel-op val ]
     * rel-op     => "==" | "!=" | ">" | ">=" | "<" | "<="
     * val        => "null" | int | float | bool | string | prop | "(" or-exp ")"
     */

    internal sealed class Parser
    {
        private Stack<TokenOutput> Tokens { get; set; }
        private Type ModelType { get; set; }
        private Expression ModelExpression { get; set; }

        public Func<ObjectType, bool> Parse<ObjectType>(string expression)
        {
            ModelType = typeof(ObjectType);
            var param = Expression.Parameter(typeof(ObjectType));
            ModelExpression = param;
            InitTokenizer(expression);
            var expressionTree = ParseExpression();
            var lambda = Expression.Lambda<Func<ObjectType, bool>>(expressionTree, param);
            return lambda.Compile();
        }

        public Func<object, bool> Parse(Type objectType, string expression)
        {
            ModelType = objectType;
            var param = Expression.Parameter(typeof(object));
            ModelExpression = Expression.Convert(param, objectType);
            InitTokenizer(expression);
            var expressionTree = ParseExpression();
            var lambda = Expression.Lambda<Func<object, bool>>(expressionTree, param);
            return lambda.Compile();
        }

        private void InitTokenizer(string expression)
        {
            var lexer = new Lexer();
            Tokens = new Stack<TokenOutput>(lexer.Analyze(expression).Reverse());
        }

        private TokenOutput PeekToken()
        {
            if (Tokens.Any())
                return Tokens.Peek();
            return null;
        }

        private void CrushToken()
        {
            Tokens.Pop();
        }

        private Expression ParseExpression()
        {
            return ParseOrExp();
        }

        private Expression ParseOrExp()
        {
            var arg1 = ParseAndExp();
            if (PeekToken().Token != Token.OR)
                return arg1;
            CrushToken();
            var arg2 = ParseOrExp();
            return Expression.Or(arg1, arg2);
        }

        private Expression ParseAndExp()
        {
            var arg1 = ParseNotExp();
            if (PeekToken().Token != Token.AND)
                return arg1;
            CrushToken();
            var arg2 = ParseAndExp();
            return Expression.And(arg1, arg2);
        }

        private Expression ParseNotExp()
        {
            var isNot = PeekToken().Token == Token.NOT;
            if (isNot)
                CrushToken();
            var arg = ParseRelExp();

            if (isNot)
                return Expression.Not(arg);
            return arg;
        }

        private Expression ParseRelExp()
        {
            var arg1 = ParseVal();
            var token = PeekToken();
            if (!new[] { Token.LT, Token.LE, Token.GT, Token.GE, Token.EQ, Token.NEQ }.Contains(token.Token))
                return arg1;
            CrushToken();
            var arg2 = ParseVal();

            Helper.MakeTypesCompatible(arg1, arg2, out arg1, out arg2);
            switch (token.Token)
            {
                case Token.LT:
                    return Expression.LessThan(arg1, arg2);
                case Token.LE:
                    return Expression.LessThanOrEqual(arg1, arg2);
                case Token.GT:
                    return Expression.GreaterThan(arg1, arg2);
                case Token.GE:
                    return Expression.GreaterThanOrEqual(arg1, arg2);
                case Token.EQ:
                    return Expression.Equal(arg1, arg2);
                case Token.NEQ:
                    return Expression.NotEqual(arg1, arg2);
                default:
                    throw new InvalidOperationException();
            }
        }

        private Expression ParseVal()
        {
            if (PeekToken().Token == Token.LEFT_BRACKET)
            {
                CrushToken();
                var arg = ParseOrExp();
                if (PeekToken().Token != Token.RIGHT_BRACKET)
                    throw new InvalidOperationException();
                CrushToken();
                return arg;
            }

            if (PeekToken().Token == Token.NULL)
            {
                CrushToken();
                return Expression.Constant(null);
            }

            var value = ParseIntLiteral() ?? ParseFloatLiteral() ?? ParseBoolLiteral() ?? ParseStringLiteral() ?? ParseProp();
            if (value == null)
                throw new InvalidOperationException();
            return value;
        }

        private Expression ParseIntLiteral()
        {
            var token = PeekToken();
            if (PeekToken().Token == Token.INT)
            {
                CrushToken();
                return Expression.Constant(token.Value, typeof(int));
            }
            return null;
        }

        private Expression ParseFloatLiteral()
        {
            var token = PeekToken();
            if (PeekToken().Token == Token.FLOAT)
            {
                CrushToken();
                return Expression.Constant(token.Value, typeof(float));
            }
            return null;
        }

        private Expression ParseBoolLiteral()
        {
            var token = PeekToken();
            if (PeekToken().Token == Token.BOOL)
            {
                CrushToken();
                return Expression.Constant(token.Value, typeof(bool));
            }
            return null;
        }

        private Expression ParseStringLiteral()
        {
            var token = PeekToken();
            if (PeekToken().Token == Token.INT)
            {
                CrushToken();
                return Expression.Constant(token.Value, typeof(string));
            }
            return null;
        }

        private Expression ParseProp()
        {
            var token = PeekToken();
            if (PeekToken().Token == Token.PROPERTY)
            {
                CrushToken();
                var propName = token.Value.ToString();
                var props = propName.Split('.');
                var type = ModelType;
                var expr = ModelExpression;
                foreach (var prop in props)
                {
                    var pi = type.GetProperty(prop);
                    if (pi == null)
                        throw new ArgumentException(string.Format("Dynamic extraction interrupted. Field {0} not found.", prop), propName);
                    expr = Expression.Property(expr, pi);
                    type = pi.PropertyType;
                }
                return expr;
            }
            return null;
        }
    }
}

