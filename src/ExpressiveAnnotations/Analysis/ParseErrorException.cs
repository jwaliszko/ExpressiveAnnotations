/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     The exception thrown when parse operation detects error in a specified expression.
    /// </summary>
    [Serializable]
    internal class ParseErrorException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseErrorException" />.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="location">The erratic code location.</param>
        public ParseErrorException(string message, Location location)
            : base(message)
        {
            Location = location;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected ParseErrorException(SerializationInfo info, StreamingContext context) // protected for unsealed classes, private for sealed classes
            : base(info, context)
        {
            Location = (Location) info.GetValue("Location", typeof (Location));
        }

        /// <summary>
        ///     Gets the erratic code location related to this error.
        /// </summary>
        public Location Location { get; private set; }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)] // deny creating an object of this type from a data that wasn't created by this serialization code 
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Location", Location, typeof(Location));
            base.GetObjectData(info, context);
        }
    }
}
