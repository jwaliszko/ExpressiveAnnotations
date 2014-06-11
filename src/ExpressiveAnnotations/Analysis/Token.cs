namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    /// Token definition.
    /// </summary>
    public sealed class Token
    {
        /// <summary>
        /// Gets the token identifier.
        /// </summary>
        public TokenId Id { get; private set; }
        /// <summary>
        /// Gets the token value.
        /// </summary>        
        public object Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="token">The token identifier.</param>
        /// <param name="value">The token value.</param>
        public Token(TokenId token, object value)
        {
            Id = token;
            Value = value;
        }
    }
}
