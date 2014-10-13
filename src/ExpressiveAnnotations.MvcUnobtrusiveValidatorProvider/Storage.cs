/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections;
using System.Web;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    /// <summary>
    ///     Stores arbitrary data for the current request.
    /// </summary>
    internal class Storage
    {
        private static IDictionary Items
        {
            get
            {
                if (HttpContext.Current == null)
                    throw new ApplicationException("HttpContext not available.");
                return HttpContext.Current.Items;
            }
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
