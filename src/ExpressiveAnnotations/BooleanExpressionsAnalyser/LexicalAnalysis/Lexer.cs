using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressiveAnnotations.BooleanExpressionsAnalyser.LexicalAnalysis
{
    internal sealed class Lexer
    {
        private readonly TokenDefinition[] _tokenDefinitions;
        private string _token;
        private string _expression;

        public Lexer(TokenDefinition[] tokenDefinitions)
        {
            _tokenDefinitions = tokenDefinitions;
        }

        public string[] Analyze(string expression, bool removeEmptyTokens)
        {
            _expression = expression;
            var tokens = new Stack<string>();
            while (Next())
            {
                tokens.Push(_token);
            }
            var result = tokens.Reverse().ToArray();
            return removeEmptyTokens ? result.Where(t => (new RegexMatcher(@"\s")).Match(t) == 0).ToArray() : result;
        }

        private bool Next()
        {
            if (string.IsNullOrEmpty(_expression))
            {
                return false;
            }

            foreach (var def in _tokenDefinitions)
            {
                var matched = def.Matcher.Match(_expression);
                if (matched > 0)
                {
                    _token = _expression.Substring(0, matched);
                    _expression = _expression.Substring(matched);
                    return true;
                }
            }
            throw new ArgumentException(string.Format("Lexer error. Unexpected token started at \"{0}\".", _expression));
        }
    }
}
