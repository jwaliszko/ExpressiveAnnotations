/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    /// The exception that is thrown when parse operation detects error in specified expression.
    /// </summary>
    internal class ParseErrorException: Exception
    {
        /// <summary>
        /// Gets the state dump of parse operation.
        /// </summary>
        public ParseState Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseErrorException" />.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="context">The state dump of parse operation.</param>
        public ParseErrorException(string message, ParseState context) 
            : base(message)
        {
            Context = context;
        }
    }
}
