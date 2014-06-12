using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.Analysis.LexicalAnalysis
{
    /// <summary>
    /// Performs basic lexical analysis of provided expression based on given token patterns.
    /// </summary>
    internal sealed class Tokenizer
    {
        private string[] Patterns { get; set; }
        private string Expression { get; set; }
        private string Token { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer" /> class.
        /// </summary>
        /// <param name="patterns">The token patterns.</param>
        public Tokenizer(string[] patterns)
        {
            Patterns = patterns;
        }

        /// <summary>
        /// Analyzes the specified expression and extracts the array of tokens.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>Array of extracted tokens.</returns>
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

            throw new ArgumentException(string.Format("Tokenizer error. Unexpected token started at {0}.", Expression));
        }
    }
}
