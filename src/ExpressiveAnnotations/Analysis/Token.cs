namespace ExpressiveAnnotations.Analysis
{
    public sealed class Token
    {
        public TokenId Id { get; private set; }
        public object Value { get; private set; }

        public Token(TokenId token, object value)
        {
            Id = token;
            Value = value;
        }
    }
}
