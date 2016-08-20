/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ExpressiveAnnotations.MvcUnobtrusive.Caching
{
    /// <summary>
    ///     Persists decomposed expressions parts for entire application instance. Implementation is concurrent and lazy.
    /// </summary>
    internal static class MapCache<TKey, TValue> // http://stackoverflow.com/q/3037203/270315
    {
        private static readonly ConcurrentDictionary<TKey, Lazy<TValue>> _cache = new ConcurrentDictionary<TKey, Lazy<TValue>>(); // why lazy? -> http://stackoverflow.com/q/12611167/270315, https://blogs.endjin.com/2015/10/using-lazy-and-concurrentdictionary-to-ensure-a-thread-safe-run-once-lazy-loaded-collection/

        public static TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) // delegate value factory invocation guaranteed to be atomic
        {
            var lazyResult = _cache.GetOrAdd(
                key,
                k => new Lazy<TValue>(
                    () => valueFactory(k),
                    LazyThreadSafetyMode.ExecutionAndPublication));
            return lazyResult.Value;
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
