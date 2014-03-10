namespace ExpressiveAnnotations.BooleanExpressionAnalysis.LexicalAnalysis
{
    public sealed class PostfixLexer
    {
        private readonly Lexer _lexer;

        public PostfixLexer()
        {
            var defs = new[]
                {
                    new TokenDefinition(@"true", Token.TRUE),
                    new TokenDefinition(@"false", Token.FALSE),
                    new TokenDefinition(@"&&", Token.AND),
                    new TokenDefinition(@"\|\|", Token.OR),
                    new TokenDefinition(@"\!", Token.NOT),
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
