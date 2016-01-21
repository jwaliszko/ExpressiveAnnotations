/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations
{
    internal static class MessageFormatter
    {
        private const string _formatItemsRegex = @"({+)[_\p{L}]+(?:(?:\.[_\p{L}])?[_\p{L}\p{N}]*)*(?::(?:n|N))?(}+)"; // {fieldPath[:indicator]}, e.g. {field}, {field.field:n} (field path regex exactly as defined in lexer field)

        public static string FormatString(string input, out IList<FormatItem> items)
        {
            Debug.Assert(input != null);

            items = new List<FormatItem>();
            var matches = Regex.Matches(input, _formatItemsRegex);
            var message = new StringBuilder();

            Match prev = null;
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var item = match.Value;
                var leftBraces = match.Groups[1];
                var rightBraces = match.Groups[2];

                if (leftBraces.Length != rightBraces.Length)
                    throw new FormatException("Input string was not in a correct format.");

                var start = prev != null ? prev.Index + prev.Length : 0;
                var chars = match.Index - start;
                message.Append(input.Substring(start, chars));
                prev = match;

                var added = items.SingleOrDefault(x => x.Body == item);
                if (added != null)
                {
                    message.Append(added.Uuid);
                    continue;
                }

                var length = leftBraces.Length;

                // flatten each pair of braces into single brace in order to escape them (just like string.Format() does)
                var leftBracesFlattened = new string('{', length / 2);
                var rightBracesFlattened = new string('}', length / 2);

                var uuid = Guid.NewGuid();
                var param = item.Substring(length, item.Length - 2 * length);
                var current = new FormatItem
                {
                    Uuid = uuid,
                    Body = item,
                    Constant = length % 2 == 0,
                    FieldPath = param.Contains(":") ? param.Substring(0, param.IndexOf(":", StringComparison.Ordinal)) : param,
                    Indicator = param.Contains(":") ? param.Substring(param.IndexOf(":", StringComparison.Ordinal) + 1) : null,
                    Substitute = $"{leftBracesFlattened}{(length%2 != 0 ? uuid.ToString() : param)}{rightBracesFlattened}" // for odd number of braces, substitute param with respective value (just like string.Format() does)
                };
                items.Add(current);
                message.Append(current.Uuid);
            }

            if(prev != null)            
                message.Append(input.Substring(prev.Index + prev.Length));

            return message.Length > 0 ? message.ToString() : input;
        }
    }

    internal class FormatItem
    {
        public Guid Uuid { get; set; }
        public string Body { get; set; }
        public bool Constant { get; set; }
        public string FieldPath { get; set; }
        public string Indicator { get; set; }
        public string Substitute { get; set; }
    }
}
