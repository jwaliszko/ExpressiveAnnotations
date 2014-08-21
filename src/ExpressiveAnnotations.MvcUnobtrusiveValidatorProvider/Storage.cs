using System;
using System.Collections;
using System.Web;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    /// <summary>
    /// Stores data for the current request.
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
            if (Items[key] == null)
                return default(T);
            return (T)Items[key];
        }

        public static void Set<T>(string key, T value)
        {
            Items[key] = value;
        }
    }
}
