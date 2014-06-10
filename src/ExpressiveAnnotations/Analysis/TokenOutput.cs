namespace ExpressiveAnnotations.LogicalExpressionsAnalysis.LexicalAnalysis
{
    internal sealed class TokenOutput
    {
        public object Value { get; private set; }
        public Token Token { get; private set; }

        public TokenOutput(object value, Token token)
        {
            Value = value;
            Token = token;
        }
    }
}
