/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    /// Contains the current state dump of ongoing parse operation.
    /// </summary>
    public class ParseState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseState" /> class.
        /// </summary>
        public ParseState()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseState" /> class.
        /// </summary>
        /// <param name="context">The state dump of parse operation.</param>
        public ParseState(ParseState context)
        {
            Expression = context.Expression;
            Line = context.Line;
            Column = context.Column;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseState" /> class.
        /// </summary>
        /// <param name="expression">The remaining expression.</param>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        public ParseState(string expression, int line, int column)
        {
            Expression = expression;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Gets or sets the remaining expression.
        /// </summary>
        public string Expression { get; set; }

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
