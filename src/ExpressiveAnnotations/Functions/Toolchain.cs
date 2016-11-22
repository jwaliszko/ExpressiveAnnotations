/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations.Functions
{
    internal static class ParserHelper
    {
        /// <summary>
        ///     Registers built-in methods for expressions parser.
        /// </summary>
        /// <param name="parser">The parser instance.</param>
        public static void RegisterToolchain(this Parser parser)
        {
            parser.RegisterFunctionsProvider(Toolchain.Instance);
        }
    }

    /// <summary>
    ///     Contains a set of predefined methods.
    /// </summary>
    /// <seealso cref="IFunctionsProvider" />
    /// <seealso cref="IFunctionsManager" />
    /// <seealso cref="IFunctionsProvider" />
    public class Toolchain : IFunctionsManager, IFunctionsProvider
    {
        /// <summary>
        ///     Delegate taking variable number of arguments.
        /// </summary>
        /// <typeparam name="TElement">The type of the argument.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="args">The comma-separated list of arguments or an array of arguments.</param>
        /// <returns>The result.</returns>
        public delegate TResult ParamsDelegate<in TElement, out TResult>(params TElement[] args);

        private Toolchain()
        {
            FuncManager = new FunctionsManager();

            AddFunction("Now", () => DateTime.Now);
            AddFunction("Today", () => DateTime.Today);
            AddFunction<string, DateTime>("ToDate", dateString => DateTime.Parse(dateString));
            AddFunction<int, int, int, DateTime>("Date", (year, month, day) => new DateTime(year, month, day));
            AddFunction<int, int, int, int, int, int, DateTime>("Date", (year, month, day, hour, minute, second) => new DateTime(year, month, day, hour, minute, second));
            AddFunction<int, int, int, int, TimeSpan>("TimeSpan", (days, hours, minutes, seconds) => new TimeSpan(days, hours, minutes, seconds));
            AddFunction<string, int>("Length", str => str != null ? str.Length : 0);
            AddFunction<string, string>("Trim", str => str != null ? str.Trim() : null);
            AddFunction<string, string, string>("Concat", (strA, strB) => string.Concat(strA, strB));
            AddFunction<string, string, string, string>("Concat", (strA, strB, strC) => string.Concat(strA, strB, strC));
            AddFunction<string, string, int>("CompareOrdinal", (strA, strB) => string.Compare(strA, strB, StringComparison.Ordinal) > 0 ? 1 : string.Compare(strA, strB, StringComparison.Ordinal) < 0 ? -1 : 0);
            AddFunction<string, string, int>("CompareOrdinalIgnoreCase", (strA, strB) => string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) > 0 ? 1 : string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) < 0 ? -1 : 0);
            AddFunction<string, string, bool>("StartsWith", (str, prefix) => str != null && prefix != null && str.StartsWith(prefix));
            AddFunction<string, string, bool>("StartsWithIgnoreCase", (str, prefix) => str != null && prefix != null && str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            AddFunction<string, string, bool>("EndsWith", (str, suffix) => str != null && suffix != null && str.EndsWith(suffix));
            AddFunction<string, string, bool>("EndsWithIgnoreCase", (str, suffix) => str != null && suffix != null && str.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
            AddFunction<string, string, bool>("Contains", (str, substr) => str != null && substr != null && str.Contains(substr));
            AddFunction<string, string, bool>("ContainsIgnoreCase", (str, substr) => str != null && substr != null && str.ToLower().Contains(substr.ToLower()));
            AddFunction<string, bool>("IsNullOrWhiteSpace", str => string.IsNullOrWhiteSpace(str));
            AddFunction<string, bool>("IsDigitChain", str => str != null && Regex.IsMatch(str, @"^[0-9]+$"));
            AddFunction<string, bool>("IsNumber", str => str != null && Regex.IsMatch(str, @"^[+-]?(?:(?:[0-9]+)|(?:[0-9]+[eE][+-]?[0-9]+)|(?:[0-9]*\.[0-9]+(?:[eE][+-]?[0-9]+)?))$")); // +/- lexer float
            AddFunction<string, bool>("IsEmail", str => str != null && Regex.IsMatch(str, @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$")); // taken from HTML5 specification: http://www.w3.org/TR/html5/forms.html#e-mail-state-(type=email)
            AddFunction<string, bool>("IsPhone", str => str != null && Regex.IsMatch(str, @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$")); // taken from PhoneAttribute implementation: https://referencesource.microsoft.com/#System.ComponentModel.DataAnnotations/DataAnnotations/PhoneAttribute.cs
            AddFunction<string, bool>("IsUrl", str => str != null && Regex.IsMatch(str, @"^(?:(?:https?|ftp):\/\/)(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:\/\S*)?$", RegexOptions.IgnoreCase)); // contributed by Diego Perini: https://gist.github.com/dperini/729294 (https://mathiasbynens.be/demo/url-regex)
            AddFunction<string, string, bool>("IsRegexMatch", (str, regex) => str != null && regex != null && Regex.IsMatch(str, regex));
            AddFunction<string, Guid>("Guid", str => new Guid(str));
            AddFunction("Min", (Expression<ParamsDelegate<double, double>>)(items => items.Min()));
            AddFunction("Max", (Expression<ParamsDelegate<double, double>>)(items => items.Max()));
            AddFunction("Sum", (Expression<ParamsDelegate<double, double>>)(items => items.Sum()));
            AddFunction("Average", (Expression<ParamsDelegate<double, double>>)(items => items.Average()));
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static Toolchain Instance { get; } = new Toolchain();

        private FunctionsManager FuncManager { get; }

        /// <summary>
        ///     Loads complately new provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public void Recharge(IFunctionsProvider provider)
        {
            FuncManager.Recharge(provider);
        }

        /// <summary>
        ///     Register function of arbitrary signature for the parser.
        /// </summary>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction(string name, LambdaExpression func)
        {
            FuncManager.AddFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TResult>(string name, Expression<Func<TResult>> func)
        {
            FuncManager.AddFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TResult>(string name, Expression<Func<TArg1, TResult>> func)
        {
            FuncManager.AddFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TResult>(string name, Expression<Func<TArg1, TArg2, TResult>> func)
        {
            FuncManager.AddFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TArg3">Third argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TArg3, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TResult>> func)
        {
            FuncManager.AddFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TArg3">Third argument.</typeparam>
        /// <typeparam name="TArg4">Fourth argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TArg3, TArg4, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TArg4, TResult>> func)
        {
            FuncManager.AddFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TArg3">Third argument.</typeparam>
        /// <typeparam name="TArg4">Fourth argument.</typeparam>
        /// <typeparam name="TArg5">Fifth argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> func)
        {
            FuncManager.AddFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TArg3">Third argument.</typeparam>
        /// <typeparam name="TArg4">Fourth argument.</typeparam>
        /// <typeparam name="TArg5">Fifth argument.</typeparam>
        /// <typeparam name="TArg6">Sixth argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> func)
        {
            FuncManager.AddFunction(name, func);
        }

        /// <summary>
        ///     Gets functions for the <see cref="ExpressiveAnnotations.Analysis.Parser" />.
        /// </summary>
        /// <returns>
        ///     Registered functions.
        /// </returns>
        public IDictionary<string, IList<LambdaExpression>> GetFunctions()
        {
            return FuncManager.GetFunctions();
        }
    }
}
