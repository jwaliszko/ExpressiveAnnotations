/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    internal static class Helper
    {
        public static string GetCoarseType(Type type)
        {
            if (type.IsDateTime())
                return "datetime";
            if (type.IsNumeric())
                return "numeric";
            if (type.IsString())
                return "string";
            if (type.IsBool())
                return "bool";
            if (type.IsGuid())
                return "guid";

            return "object";
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // code that deals with disposables should be consistent (and classes should be resilient to multiple Dispose() calls)
        public static string ToJson(this object data)
        {
            var stringBuilder = new StringBuilder();
            var jsonSerializer = new JsonSerializer();
            using (var stringWriter = new StringWriter(stringBuilder))
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                jsonSerializer.Serialize(jsonTextWriter, data);
                return stringBuilder.ToString();
            }
        }
    }
}
