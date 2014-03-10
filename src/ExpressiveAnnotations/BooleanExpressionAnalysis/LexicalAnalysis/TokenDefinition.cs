namespace ExpressiveAnnotations.BooleanExpressionAnalysis.LexicalAnalysis
{
    internal sealed class TokenDefinition
    {
        public RegexMatcher Matcher { get; private set; }
        public string Token { get; private set; }

        public TokenDefinition(string pattern, string token)
        {
            Matcher = new RegexMatcher(pattern);
            Token = token;            
        }
    }
}
