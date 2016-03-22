/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     The exception thrown when parse operation detects error in a specified expression.
    /// </summary>
    [Serializable] // this attribute is not inherited from Exception and must be specified otherwise serialization will fail
    public class ParseErrorException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        public ParseErrorException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ParseErrorException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ParseErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="location">The error location.</param>
        public ParseErrorException(string error, string expression, Location location)
            : base(location.BuildParseError(error, expression))
        {
            Error = error;
            Expression = expression;
            Location = location.Clone();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="location">The error location.</param>        
        /// <param name="innerException">The inner exception.</param>
        public ParseErrorException(string error, string expression, Location location, Exception innerException)
            : base(location.BuildParseError(error, expression), innerException)
        {
            Error = error;
            Expression = expression;
            Location = location.Clone();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" /> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected ParseErrorException(SerializationInfo info, StreamingContext context) // serialization constructor (without it deserialization will fail), protected for unsealed classes, private for sealed classes
            : base(info, context)
        {
            Error = (string) info.GetValue("Error", typeof (string));
            Expression = (string) info.GetValue("Expression", typeof (string));
            Location = (Location) info.GetValue("Location", typeof (Location));
        }

        /// <summary>
        ///     Gets the error message.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        ///     Gets the expression.
        /// </summary>        
        public string Expression { get; private set; }

        /// <summary>
        ///     Gets the error location.
        /// </summary>
        public Location Location { get; private set; }

        /// <summary>
        ///     Gets the object data.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)] // deny creating an object of this type from a data that wasn't created by this serialization code 
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Debug.Assert(info != null);

            info.AddValue("Error", Error, typeof (string));
            info.AddValue("Expression", Expression, typeof (string));
            info.AddValue("Location", Location, typeof (Location));            
            base.GetObjectData(info, context);
        }
    }
}
