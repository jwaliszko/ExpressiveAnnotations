using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis.LexicalAnalysis
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
            var tokens = new List<string>();
            while (Next())
            {
                tokens.Add(_token);
            }
            return removeEmptyTokens ? tokens.Where(token => !Token.SPACE.Equals(token)).ToArray() : tokens.ToArray();
        }

        private bool Next()
        {
            if (string.IsNullOrEmpty(_expression))
                return false;

            foreach (var def in _tokenDefinitions)
            {
                var matched = def.Matcher.Match(_expression);
                if (matched > 0)
                {
                    _token = def.Token;
                    _expression = _expression.Substring(matched);
                    return true;
                }
            }
            throw new ArgumentException(string.Format("Lexer error. Unexpected token started at \"{0}\".", _expression));
        }
    }
}
