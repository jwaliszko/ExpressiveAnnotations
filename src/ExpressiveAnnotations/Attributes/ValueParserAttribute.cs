/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    ///     Provides a hint for client-side script pointing at parser name, which should be used for DOM field value deserialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ValueParserAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueParserAttribute" /> class.
        /// </summary>
        /// <param name="parserName">Name of the parser.</param>
        public ValueParserAttribute(string parserName)
        {
            ParserName = parserName;
        }

        /// <summary>
        ///     Gets the name of the parser.
        /// </summary>
        public string ParserName { get; private set; }
    }
}
