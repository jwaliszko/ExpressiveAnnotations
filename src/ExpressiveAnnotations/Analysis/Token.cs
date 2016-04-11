/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     Token definition.
    /// </summary>
    public class Token
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Token" /> class.
        /// </summary>
        /// <param name="type">The token type.</param>
        /// <param name="value">The token value.</param>
        /// <param name="rawValue">The token raw value.</param>
        /// <param name="location">The token location within a specified expression.</param>
        public Token(TokenType type, object value, string rawValue, Location location)
        {
            Type = type;
            Value = value;
            RawValue = rawValue;
            Location = location;
        }

        /// <summary>
        ///     Gets the token type.
        /// </summary>
        public TokenType Type { get; private set; }

        /// <summary>
        ///     Gets the token value (converted to appropriate type).
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        ///     Gets the token raw value (not converted expression string).
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        ///     Gets or sets the token location within a specified expression.
        /// </summary>
        public Location Location { get; private set; }
    }
}
