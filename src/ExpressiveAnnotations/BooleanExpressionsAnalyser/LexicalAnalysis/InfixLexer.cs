namespace ExpressiveAnnotations.BooleanExpressionsAnalyser.LexicalAnalysis
{
    public sealed class InfixLexer
    {
        private readonly Lexer _lexer;

        public InfixLexer()
        {
            var defs = new[]
                {
                    new TokenDefinition(@"true", "TRUE"),
                    new TokenDefinition(@"false", "FALSE"),
                    new TokenDefinition(@"&&", "AND"),
                    new TokenDefinition(@"\|\|", "OR"),
                    new TokenDefinition(@"\!", "NOT"),
                    new TokenDefinition(@"\(", "LEFT"),
                    new TokenDefinition(@"\)", "RIGHT"),
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