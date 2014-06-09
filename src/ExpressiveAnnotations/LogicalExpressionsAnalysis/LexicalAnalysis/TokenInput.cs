namespace ExpressiveAnnotations.LogicalExpressionsAnalysis.LexicalAnalysis
{
    internal sealed class TokenInput
    {
        public string Pattern { get; private set; }
        public Token Token { get; private set; }

        public TokenInput(string pattern, Token token)
        {
            Pattern = pattern;
            Token = token;
        }
    }
}
