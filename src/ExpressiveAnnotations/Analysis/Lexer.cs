/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     Performs the lexical analysis of a specified logical expression.
    /// </summary>
    /// <remarks>
    ///     Type is thread safe.
    /// </remarks>
    public sealed class Lexer
    {
        private readonly object _locker = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Lexer" /> class.
        /// </summary>
        public Lexer()
        {
            // regex special characters .$^{[(|)*+?\ should be escaped, if needed (in a character class any character except ^-]\ is a literal)
            // https://msdn.microsoft.com/en-us/library/4edbef7e(v=vs.110).aspx, http://www.regular-expressions.info/refcharclass.html
            var patterns = new Dictionary<TokenType, string>
            {
                {TokenType.AND, @"&&"},
                {TokenType.OR, @"\|\|"},
                {TokenType.LEFT_BRACKET, @"\("},
                {TokenType.RIGHT_BRACKET, @"\)"},
                {TokenType.GE, @">="},
                {TokenType.LE, @"<="},
                {TokenType.GT, @">"},
                {TokenType.LT, @"<"},
                {TokenType.EQ, @"=="},
                {TokenType.NEQ, @"!="},
                {TokenType.NOT, @"!"},
                {TokenType.NULL, @"null"},
                {TokenType.COMMA, @","},
                {TokenType.INC, @"\+{2}"}, // Despite the fact our language does not support ++ and -- prefix/postfix operations yet, these unary tokens are explicitly designated as illegal. We're detecting them to prevent unification which is done for consecutive plus or minus operators (e.g. + + - - +-+- => +).
                {TokenType.DEC, @"-{2}"},  // Unification of such unary operators breaks compatibility - such mixed 1++ + 2 operations are illegal in both C# and JavaScript (since we control C# side there is not pain here, but JavaScript would fail since we're sending raw expressions to client-side).
                {TokenType.ADD, @"\+"},
                {TokenType.SUB, @"-"},
                {TokenType.MUL, @"\*"},
                {TokenType.DIV, @"/"},
                {TokenType.FLOAT, @"(?:(?:[0-9]+[eE][+-]?[0-9]+)|(?:[0-9]*\.[0-9]+(?:[eE][+-]?[0-9]+)?))"}, // 1e5, 1.0, 0.3e-2
                {TokenType.INT, @"[0-9]+"},
                {TokenType.BOOL, @"(?:true|false)"},
                {TokenType.STRING, @"(['])(?:\\\1|.)*?\1"}, // '1234', 'John\'s cat'
                {TokenType.FUNC, @"[_\p{L}]+(?:(?:(?:\[[0-9]+\])?\.[_\p{L}])?[_\p{L}\p{N}]*)*(?:\[[0-9]+\])?"} // field, field.field, arr[0], func(...), see also http://www.fileformat.info/info/unicode/category/index.htm, https://msdn.microsoft.com/en-us/library/aa664670.aspx
            };

            RegexMap = patterns.ToDictionary(
                kvp => kvp.Key,
                kvp => new Regex($"^{kvp.Value}")); // in general, for compiled version of regular expressions their construction and initialization time is amortized out over many runs
        }                                           // in case of EA library interpreted version is preferred over compiled one - runs count is low (logical expressions provided to attributes aren't long that much)

        private Token Token { get; set; }
        private Location Location { get; set; }
        private string Expression { get; set; }
        private IDictionary<TokenType, Regex> RegexMap { get; set; }

        /// <summary>
        ///     Analyzes a specified logical expression and extracts a sequence of tokens.
        /// </summary>
        /// <param name="expression">The logical expression.</param>
        /// <returns>
        ///     A sequence of extracted tokens.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">expression;Expression not provided.</exception>
        public IEnumerable<Token> Analyze(string expression)
        {
            lock (_locker)
            {
                if (expression == null)
                    throw new ArgumentNullException(nameof(expression), "Expression not provided.");

                Location = new Location(line: 1, column: 1);
                Expression = expression;

                var tokens = new List<Token>();
                while (Next())
                {
                    tokens.Add(Token);
                }

                // once we've reached the end of the string, EOF token is returned - thus, parser's lookahead does not have to worry about running out of tokens
                tokens.Add(new Token(TokenType.EOF, string.Empty, new Location(Location)));
                return tokens;
            }
        }

        private bool Next()
        {
            int line, column;
            Expression = Expression.TrimStart(out line, out column);
            Location.Line += line;
            Location.Column = line > 0 ? column : Location.Column + column;

            if (Expression.Length == 0)
                return false;

            foreach (var kvp in RegexMap)
            {
                var regex = kvp.Value;
                var match = regex.Match(Expression);
                if (!match.Success) 
                    continue;
                
                var value = match.Value;
                Token = new Token(kvp.Key, ConvertTokenValue(kvp.Key, value), new Location(Location));

                Expression = Expression.Substring(value.Length, out line, out column);
                Location.Line += line;
                Location.Column = line > 0 ? column : Location.Column + column;
                return true;
            }
            throw new ParseErrorException("Invalid token.", new Location(Location));
        }

        private object ConvertTokenValue(TokenType type, string value)
        {
            switch (type)
            {
                case TokenType.NULL:
                    return null;
                case TokenType.INT:
                    return int.Parse(value, CultureInfo.InvariantCulture);
                case TokenType.FLOAT:
                    return double.Parse(value, CultureInfo.InvariantCulture); // By default, treat real numeric literals as 64-bit floating binary point values (as C#
                case TokenType.BOOL:                                          // does, gives better precision than float). What's more, InvariantCulture means no matter
                    return bool.Parse(value);                                 // the current culture, dot is always accepted in double literal to be succesfully parsed.
                case TokenType.STRING:
                    return ParseStringLiteral(value);
                default:
                    return value;
            }
        }

        private string ParseStringLiteral(string value)
        {
            //System.Diagnostics.Debug.WriteLine(value); // when looking at expression in debugger, take into account that debugger prints control characters, while output does not
            var unescaped = value.Substring(1, value.Length - 2)
                .Replace(@"\'", "'") // remove backslash escape character when it is placed in front of single quote character 
                                     // (when such a quote is used internally inside string literal, e.g. John\'s cat => changed to John's cat)
                .Replace(@"\n", Environment.NewLine) // in our language \n represents new line for current environment
                .Replace(@"\r", "\r")
                .Replace(@"\t", "\t")
                .Replace(@"\\", "\\");
            return unescaped;
            //System.Diagnostics.Debug.WriteLine(unescaped);
        }
    }
}
