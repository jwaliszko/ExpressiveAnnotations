﻿/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations
{
    internal static class MessageFormatter
    {
        private const string _formatItemsRegex = @"({+)[a-zA-Z_]+(?:(?:\.[a-zA-Z_])?[a-zA-Z0-9_]*)*(?::(?:n|N))?(}+)"; // {fieldPath[:indicator]}, e.g. {field}, {field.field:n}

        public static string FormatString(string input, out IList<FormatItem> items)
        {
            items = new List<FormatItem>();
            var matches = Regex.Matches(input, _formatItemsRegex);
            var message = new StringBuilder();

            Match prev = null;
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var arg = match.Value;
                var leftBraces = match.Groups[1];
                var rightBraces = match.Groups[2];

                if (leftBraces.Length != rightBraces.Length)
                    throw new FormatException("Input string was not in a correct format.");

                var start = prev != null ? prev.Index + prev.Length : 0;
                var chars = match.Index - start;
                message.Append(input.Substring(start, chars));
                prev = match;

                if (items.Any(x => x.Body == arg))
                    continue;

                var length = leftBraces.Length;

                // flatten each pair of braces into single brace in order to escape them (just like string.Format() does)
                var leftBracesFlattened = new string('{', length / 2);
                var rightBracesFlattened = new string('}', length / 2);

                var guid = Guid.NewGuid();

                var param = arg.Substring(length, arg.Length - 2 * length);
                items.Add(new FormatItem
                {
                    Id = guid,
                    Body = arg,
                    Constant = length % 2 == 0,
                    FieldPath = param.Contains(":") ? param.Substring(0, param.IndexOf(":", StringComparison.Ordinal)) : param,
                    Indicator = param.Contains(":") ? param.Substring(param.IndexOf(":", StringComparison.Ordinal) + 1) : null,
                    Substitute = string.Format("{0}{1}{2}", leftBracesFlattened, length % 2 != 0 ? guid.ToString() : param, rightBracesFlattened) // for odd number of braces, substitute param with respective value (just like string.Format() does)
                });
                message.Append(guid);
            }

            return message.Length > 0 ? message.ToString() : input;
        }
    }

    internal class FormatItem
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public bool Constant { get; set; }
        public string FieldPath { get; set; }
        public string Indicator { get; set; }
        public string Substitute { get; set; }
    }
}