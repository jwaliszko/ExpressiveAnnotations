using System;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    /// Contains a set of predefined methods.
    /// </summary>
    internal static class Toolchain
    {        
        /// <summary>
        /// Registers methods for expressions.
        /// </summary>
        /// <param name="parser">Parser.</param>
        public static void RegisterMethods(this Parser parser)
        {
            parser.AddFunction("Today", () => DateTime.Today);
            parser.AddFunction<string, string>("Trim", text => text != null ? text.Trim() : null);
            parser.AddFunction<string, string, int>("CompareOrdinal", (strA, strB) => String.CompareOrdinal(strA, strB));
        }
    }
}
