namespace ExpressiveAnnotations.BooleanExpressionsAnalyser.LexicalAnalysis
{
    internal sealed class TokenDefinition
    {
        public RegexMatcher Matcher { get; private set; }
        public object Token { get; private set; }

        public TokenDefinition(string regex, object token)
        {
            Matcher = new RegexMatcher(regex);
            Token = token;
        }
    }
}
