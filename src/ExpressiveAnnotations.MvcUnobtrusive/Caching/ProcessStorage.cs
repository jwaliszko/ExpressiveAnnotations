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
    ///     Stores arbitrary key-value pairs for the entire lifespan of the application (until the pool recycles).
    /// </summary>
    /// <remarks>
    ///     Type is thread safe (atomic invocation of the value factory func guaranteed, implementation uses concurrent dictionary with lazy value access).
    /// </remarks>
    internal static class ProcessStorage<TKey, TValue> // note about static generics: http://stackoverflow.com/q/3037203/270315
    {
        private static readonly ConcurrentDictionary<TKey, Lazy<TValue>> _cache = new ConcurrentDictionary<TKey, Lazy<TValue>>(); // why lazy? -> http://stackoverflow.com/q/12611167/270315

        public static TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) // delegate value factory invocation guaranteed to be atomic
        {
            var lazyResult = _cache.GetOrAdd(
                key,
                k => new Lazy<TValue>(
                    () => valueFactory(k),
                    LazyThreadSafetyMode.ExecutionAndPublication));
            return lazyResult.Value; /* From http://bit.ly/2b8E1AS: If multiple concurrent threads try to call GetOrAdd with the same key at once, multiple Lazy objects may be
                                      * created but these are cheap, and all but one will be thrown away. The return Lazy object will be the same across all threads, and the
                                      * first one to call the Value property will run the expensive delegate method, whilst the other threads are locked, waiting for the result. */
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
        public IDictionary<string, object> EnumsMap { get; set; }
        public IEnumerable<string> MethodsList { get; set; }
        public IDictionary<string, string> ParsersMap { get; set; }
    }
}
