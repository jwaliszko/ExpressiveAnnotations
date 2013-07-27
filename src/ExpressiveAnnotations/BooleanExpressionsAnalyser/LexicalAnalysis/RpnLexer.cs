namespace ExpressiveAnnotations.BooleanExpressionsAnalyser.LexicalAnalysis
{
    public sealed class RpnLexer
    {
        private readonly Lexer _lexer;

        public RpnLexer()
        {
            var defs = new[]
                {
                    new TokenDefinition(@"true", "TRUE"),
                    new TokenDefinition(@"false", "FALSE"),
                    new TokenDefinition(@"&&", "AND"),
                    new TokenDefinition(@"\|\|", "OR"),
                    new TokenDefinition(@"\!", "NOT"),
                    new TokenDefinition(@"\s", "SPACE")
                };
            _lexer = new Lexer(defs);
        }

        public string[] Analyze(string expression, bool removeEmptyTokens)
        {
            return _lexer.Analyze(expression, removeEmptyTokens);
        }
    }
}
