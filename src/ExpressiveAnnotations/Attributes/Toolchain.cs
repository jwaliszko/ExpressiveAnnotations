using System;
using System.Text.RegularExpressions;
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
            parser.AddFunction("Now", () => DateTime.Now);
            parser.AddFunction("Today", () => DateTime.Today);
            parser.AddFunction<string, int>("Length", str => str != null ? str.Length : 0);
            parser.AddFunction<string, string>("Trim", str => str != null ? str.Trim() : null);
            parser.AddFunction<string, string, string>("Concat", (strA, strB) => string.Concat(strA, strB));
            parser.AddFunction<string, string, string, string>("Concat", (strA, strB, strC) => string.Concat(strA, strB, strC));
            parser.AddFunction<string, string, int>("CompareOrdinal", (strA, strB) => string.Compare(strA, strB, StringComparison.Ordinal));
            parser.AddFunction<string, string, int>("CompareOrdinalIgnoreCase", (strA, strB) => string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase));
            parser.AddFunction<string, bool>("IsNullOrWhiteSpace", str => string.IsNullOrWhiteSpace(str));
            parser.AddFunction<string, bool>("IsNumber", str => Regex.IsMatch(str, @"^[-+]?\d+$") || Regex.IsMatch(str, @"^[-+]?\d*\.\d+([eE][-+]?\d+)?$"));
        }
    }
}
