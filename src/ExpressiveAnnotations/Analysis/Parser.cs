using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressiveAnnotations.Analysis
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

    public sealed class Parser
    {
        private Stack<Token> Tokens { get; set; }
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
            Tokens = new Stack<Token>(lexer.Analyze(expression).Reverse());
        }

        private bool PeekToken(out Token token)
        {
            token = Tokens.Any() ? Tokens.Peek() : null;
            return token != null;
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
            Token token;
            if (!PeekToken(out token) || token.Id != TokenId.OR)
                return arg1;
            CrushToken();
            var arg2 = ParseOrExp();
            return Expression.Or(arg1, arg2);
        }

        private Expression ParseAndExp()
        {
            var arg1 = ParseNotExp();
            Token token;
            if (!PeekToken(out token) || token.Id != TokenId.AND)
                return arg1;
            CrushToken();
            var arg2 = ParseAndExp();
            return Expression.And(arg1, arg2);
        }

        private Expression ParseNotExp()
        {
            Token token;
            if (!PeekToken(out token) || token.Id != TokenId.NOT)
                return ParseRelExp();
            CrushToken();
            return Expression.Not(ParseRelExp());
        }

        private Expression ParseRelExp()
        {
            var arg1 = ParseVal();
            Token token;
            if (!PeekToken(out token) || !new[] { TokenId.LT, TokenId.LE, TokenId.GT, TokenId.GE, TokenId.EQ, TokenId.NEQ }.Contains(token.Id))
                return arg1;
            CrushToken();
            var arg2 = ParseVal();

            Helper.MakeTypesCompatible(arg1, arg2, out arg1, out arg2);
            switch (token.Id)
            {
                case TokenId.LT:
                    return Expression.LessThan(arg1, arg2);
                case TokenId.LE:
                    return Expression.LessThanOrEqual(arg1, arg2);
                case TokenId.GT:
                    return Expression.GreaterThan(arg1, arg2);
                case TokenId.GE:
                    return Expression.GreaterThanOrEqual(arg1, arg2);
                case TokenId.EQ:
                    return Expression.Equal(arg1, arg2);
                case TokenId.NEQ:
                    return Expression.NotEqual(arg1, arg2);
                default:
                    throw new InvalidOperationException();
            }
        }

        private Expression ParseVal()
        {
            Token token;
            if (!PeekToken(out token))
                throw new InvalidOperationException();

            if (token.Id == TokenId.LEFT_BRACKET)
            {
                CrushToken();
                var arg = ParseOrExp();
                if (!PeekToken(out token) || token.Id != TokenId.RIGHT_BRACKET)
                    throw new InvalidOperationException();
                CrushToken();
                return arg;
            }

            CrushToken();
            switch (token.Id)
            {
                case TokenId.NULL:
                    return Expression.Constant(null);
                case TokenId.INT:
                    return Expression.Constant(token.Value, typeof(int));
                case TokenId.FLOAT:
                    return Expression.Constant(token.Value, typeof(float));
                case TokenId.BOOL:
                    return Expression.Constant(token.Value, typeof(bool));
                case TokenId.STRING:
                    return Expression.Constant(token.Value, typeof(string));
                case TokenId.PROPERTY:
                    return ExtractProp(token.Value.ToString());
                default:
                    throw new InvalidOperationException();
            }
        }

        private Expression ExtractProp(string name)
        {
            var props = name.Split('.');
            var type = ModelType;
            var expr = ModelExpression;
            foreach (var prop in props)
            {
                var pi = type.GetProperty(prop);
                if (pi == null)
                    throw new ArgumentException(string.Format("Dynamic extraction interrupted. Field {0} not found.", prop), name);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            return expr;
        }
    }
}
