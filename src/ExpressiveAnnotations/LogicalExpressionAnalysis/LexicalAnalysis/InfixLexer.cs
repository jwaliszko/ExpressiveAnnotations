namespace ExpressiveAnnotations.LogicalExpressionAnalysis.LexicalAnalysis
{
    public sealed class InfixLexer
    {
        private readonly Lexer _lexer;

        public InfixLexer()
        {
            var defs = new[]
                {
                    new TokenDefinition(@"true", Token.TRUE),
                    new TokenDefinition(@"false", Token.FALSE),
                    new TokenDefinition(@"&&", Token.AND),
                    new TokenDefinition(@"\|\|", Token.OR),
                    new TokenDefinition(@"\!", Token.NOT),
                    new TokenDefinition(@"\(", Token.LEFT),
                    new TokenDefinition(@"\)", Token.RIGHT),
                    new TokenDefinition(@"\s", Token.SPACE)
                };
            _lexer = new Lexer(defs);
        }

        public string[] Analyze(string expression, bool removeEmptyTokens)
        {
            return _lexer.Analyze(expression, removeEmptyTokens);
        }
    }
}