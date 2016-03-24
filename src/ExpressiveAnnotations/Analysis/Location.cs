/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     Contains the location information related to some arbitrary data within associated text block.
    ///     <para>
    ///         Used for pointing exact parsing error location in specified expression.
    ///     </para>
    /// </summary>
    [Serializable]
    public class Location
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Location" /> class.
        /// </summary>
        /// <param name="line">The line number.</param>
        /// <param name="column">The column number.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Line number should be positive.
        /// or
        /// Column number should be positive.
        /// </exception>
        public Location(int line, int column)
        {
            if (line < 1)
                throw new ArgumentOutOfRangeException(nameof(line), "Line number should be positive.");
            if (column < 1)
                throw new ArgumentOutOfRangeException(nameof(column), "Column number should be positive.");

            Line = line;
            Column = column;
        }

        /// <summary>
        ///     Gets or sets the line number.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        ///     Gets or sets the column number.
        /// </summary>
        public int Column { get; set; }
    }
}
