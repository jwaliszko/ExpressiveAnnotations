using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis.LexicalAnalysis
{
    internal sealed class Tokenizer
    {
        private string[] Patterns { get; set; }
        private string Expression { get; set; }
        private string Token { get; set; }

        public Tokenizer(string[] patterns)
        {
            Patterns = patterns;
        }

        public string[] Analyze(string expression)
        {
            var tokens = new List<string>();
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

            foreach (var pattern in Patterns)
            {
                var regex = new Regex(string.Format("^{0}", pattern));
                var match = regex.Match(Expression);
                var value = match.Value;
                if (value.Any())
                {
                    Token = value;
                    Expression = Expression.Substring(Token.Length);
                    return true;
                }
            }

            throw new ArgumentException(string.Format("Lexer error. Unexpected token started at {0}.", Expression));
        }
    }
}
