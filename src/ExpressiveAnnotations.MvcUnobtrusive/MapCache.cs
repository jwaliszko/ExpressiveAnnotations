/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ExpressiveAnnotations.MvcUnobtrusive
{
    internal class MapCache
    {
        private static readonly MapCache _instance = new MapCache();
        private static readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

        private MapCache()
        {
        }

        public static MapCache Instance
        {
            get { return _instance; }
        }

        public CacheItem GetOrAdd(string key, Func<string, CacheItem> func)
        {
            return _cache.GetOrAdd(key, func);
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }

    internal class CacheItem
    {
        public IDictionary<string, string> FieldsMap { get; set; }
        public IDictionary<string, object> ConstsMap { get; set; }
        public IDictionary<string, string> ParsersMap { get; set; }
        public IDictionary<string, Guid> ErrFieldsMap { get; set; }
        public string FormattedErrorMessage { get; set; }
    }
}
