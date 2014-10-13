/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     Token definition.
    /// </summary>
    public sealed class Token
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Token" /> class.
        /// </summary>
        /// <param name="type">The token type.</param>
        /// <param name="value">The token value.</param>
        /// <param name="location">The token location within a specified expression.</param>
        public Token(TokenType type, object value, Location location)
        {
            Type = type;
            Value = value;
            Location = location;
        }

        /// <summary>
        ///     Gets the token type.
        /// </summary>
        public TokenType Type { get; private set; }

        /// <summary>
        ///     Gets the token value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        ///     Gets or sets the token location within a specified expression.
        /// </summary>
        public Location Location { get; private set; }
    }
}
