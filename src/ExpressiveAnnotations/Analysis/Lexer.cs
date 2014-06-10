using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis.LexicalAnalysis
{
    internal sealed class Lexer
    {
        private TokenInput[] InputTable { get; set; }
        private TokenOutput Output { get; set; }
        private string Expression { get; set; }

        public Lexer()
        {
            InputTable = new[]
            {
                new TokenInput(@"&&", Token.AND),
                new TokenInput(@"\|\|", Token.OR),
                new TokenInput(@"\(", Token.LEFT_BRACKET),
                new TokenInput(@"\)", Token.RIGHT_BRACKET),
                new TokenInput(@">=", Token.GE),
                new TokenInput(@"<=", Token.LE),
                new TokenInput(@">", Token.GT),
                new TokenInput(@"<", Token.LT),
                new TokenInput(@"==", Token.EQ),
                new TokenInput(@"!=", Token.NEQ),
                new TokenInput(@"\!", Token.NOT),
                new TokenInput(@"null", Token.NULL),
                new TokenInput(@"[0-9]+\.[0-9]+", Token.FLOAT),
                new TokenInput(@"[0-9]+", Token.INT),                
                new TokenInput(@"(true|false)", Token.BOOL),
                new TokenInput("\"[^\"]*\"|'[^']*'", Token.STRING),
                new TokenInput(@"[a-zA-Z]+([\.]*[a-zA-Z0-9]*)*", Token.PROPERTY)
            };
        }

        public TokenOutput[] Analyze(string expression)
        {
            var tokens = new List<TokenOutput>();
            if (string.IsNullOrEmpty(expression))
                return tokens.ToArray();

            Expression = expression;
            while (Next())
                tokens.Add(Output);
            return tokens.ToArray();
        }

        private bool Next()
        {
            Expression = Expression.Trim();
            if (string.IsNullOrEmpty(Expression))
                return false;

            foreach (var input in InputTable)
            {
                var regex = new Regex(string.Format("^{0}", input.Pattern));
                var match = regex.Match(Expression);
                var value = match.Value;
                if (value.Any())
                {
                    Output = new TokenOutput(ConvertTokenValue(input.Token, value), input.Token);
                    Expression = Expression.Substring(value.Length);
                    return true;
                }
            }
            throw new ArgumentException(string.Format("Lexer error. Unexpected token started at {0}.", Expression));
        }

        private object ConvertTokenValue(Token token, string value)
        {
            switch (token)
            {
                case Token.NULL:
                    return null;
                case Token.INT:
                    return int.Parse(value);
                case Token.FLOAT:
                    return float.Parse(value);
                case Token.BOOL:
                    return bool.Parse(value);
                case Token.STRING:
                    return value.Substring(1, value.Length - 2);
                default:
                    return value;
            }
        }
    }
}
