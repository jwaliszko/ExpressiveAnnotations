using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    /// Performs lexical analysis of provided logical expression.
    /// </summary>
    public sealed class Lexer
    {
        private Token Token { get; set; }
        private string Expression { get; set; }
        private IDictionary<TokenId, string> RegexMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class.
        /// </summary>
        public Lexer()
        {
            RegexMap = new Dictionary<TokenId, string>
            {
                {TokenId.AND, @"&&"},
                {TokenId.OR, @"\|\|"},
                {TokenId.LEFT_BRACKET, @"\("},
                {TokenId.RIGHT_BRACKET, @"\)"},
                {TokenId.GE, @">="},
                {TokenId.LE, @"<="},
                {TokenId.GT, @">"},
                {TokenId.LT, @"<"},
                {TokenId.EQ, @"=="},
                {TokenId.NEQ, @"!="},
                {TokenId.NOT, @"\!"},
                {TokenId.NULL, @"null"},
                {TokenId.FLOAT, @"[-+]?\d*\.\d+([eE][-+]?\d+)?"},
                {TokenId.INT, @"[-+]?\d+"},
                {TokenId.BOOL, @"(true|false)"},
                {TokenId.STRING, @"([""'])(?:\\\1|.)*?\1"},
                {TokenId.PROPERTY, @"[a-zA-Z]+([\.]*[a-zA-Z0-9]*)*"}
            };
        }

        /// <summary>
        /// Analyzes the specified logical expression and extracts the array of tokens.
        /// </summary>
        /// <param name="expression">The logical expression.</param>
        /// <returns>Array of extracted tokens.</returns>
        public Token[] Analyze(string expression)
        {
            var tokens = new List<Token>();
            if (string.IsNullOrEmpty(expression))
                return tokens.ToArray();

            Expression = expression;
            while (Next())
                tokens.Add(Token);
            return tokens.ToArray();
        }

        private bool Next()
        {
            Expression = Expression.Trim();
            if (string.IsNullOrEmpty(Expression))
                return false;

            foreach (var kvp in RegexMap)
            {
                var regex = new Regex(string.Format("^{0}", kvp.Value));
                var match = regex.Match(Expression);
                var value = match.Value;
                if (value.Any())
                {
                    Token = new Token(kvp.Key, ConvertTokenValue(kvp.Key, value));
                    Expression = Expression.Substring(value.Length);
                    return true;
                }
            }
            throw new ArgumentException(string.Format("Lexer error. Unexpected token started at {0}.", Expression));
        }

        private object ConvertTokenValue(TokenId token, string value)
        {
            switch (token)
            {
                case TokenId.NULL:
                    return null;
                case TokenId.INT:
                    return int.Parse(value);
                case TokenId.FLOAT:
                    return float.Parse(value);
                case TokenId.BOOL:
                    return bool.Parse(value);
                case TokenId.STRING:
                    return value.Substring(1, value.Length - 2);
                default:
                    return value;
            }
        }
    }
}
