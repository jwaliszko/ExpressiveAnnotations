/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ExpressiveAnnotations.MvcUnobtrusive
{
    /// <summary>
    ///     Persists decomposed expressions parts for entire application instance.
    /// </summary>
    internal static class MapCache
    {
        private static readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

        public static CacheItem GetOrAdd(string key, Func<string, CacheItem> func)
        {
            return _cache.GetOrAdd(key, func);
        }

        public static void Clear()
        {
            _cache.Clear();
        }
    }

    internal class CacheItem
    {
        public IDictionary<string, string> FieldsMap { get; set; }
        public IDictionary<string, object> ConstsMap { get; set; }
        public IDictionary<string, string> ParsersMap { get; set; }
    }
}
