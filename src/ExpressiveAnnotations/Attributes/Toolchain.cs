using System;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    /// Contains a set of useful tool methods.
    /// </summary>
    internal static class Toolchain
    {        
        /// <summary>
        /// Registers utility methods for parser, to be later used inside expressions.
        /// </summary>
        /// <param name="parser">Parser.</param>
        public static void Supplement(Parser parser)
        {
            parser.AddFunction("Today", () => DateTime.Today);
            parser.AddFunction<string, string>("Trim", text => text.Trim());
            parser.AddFunction<string, string, int>("CompareOrdinal", (strA, strB) => String.CompareOrdinal(strA, strB));
        }
    }
}
