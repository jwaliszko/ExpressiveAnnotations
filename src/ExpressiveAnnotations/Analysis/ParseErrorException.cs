/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    /// The exception thrown when parse operation detects error in specified expression.
    /// </summary>
    internal class ParseErrorException: Exception
    {
        /// <summary>
        /// Gets erratic code location related to this error.
        /// </summary>
        public Location Location { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseErrorException" />.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="location">The erratic code location.</param>
        public ParseErrorException(string message, Location location)
            : base(message)
        {
            Location = location;
        }
    }
}
