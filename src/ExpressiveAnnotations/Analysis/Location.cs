/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    /// Contains the location information related to some arbitrary data within associated text block.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public Location(Location location)
        {
            Line = location.Line;
            Column = location.Column;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location" /> class.
        /// </summary>        
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        public Location(int line, int column)
        {
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        public int Column { get; set; }
    }
}
