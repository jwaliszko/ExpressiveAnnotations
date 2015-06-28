/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections;
using System.Web;

namespace ExpressiveAnnotations.MvcUnobtrusive
{
    /// <summary>
    ///     Stores arbitrary data for the current HTTP request.
    /// </summary>
    internal class RequestStorage
    {
        private static IDictionary Items
        {
            get
            {
                if (HttpContext.Current == null)
                    throw new ApplicationException("HttpContext not available.");
                return HttpContext.Current.Items; // location that could be used throughtout the entire HTTP request lifetime
            }                                     // (contrary to a session, this one exists only within the period of a single request).
        }

        public static T Get<T>(string key)
        {
            return Items[key] == null
                ? default(T)
                : (T) Items[key];
        }

        public static void Set<T>(string key, T value)
        {
            Items[key] = value;
        }
    }
}
