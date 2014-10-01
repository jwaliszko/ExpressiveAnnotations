/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    /// Token definition.
    /// </summary>
    public sealed class Token
    {
        /// <summary>
        /// Gets the token type.
        /// </summary>
        public TokenType Type { get; private set; }
        /// <summary>
        /// Gets the token value.
        /// </summary>        
        public object Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="type">The token identifier.</param>
        /// <param name="value">The token value.</param>
        public Token(TokenType type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
