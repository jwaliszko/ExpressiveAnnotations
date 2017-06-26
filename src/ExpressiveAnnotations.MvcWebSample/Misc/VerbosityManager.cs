using System;
using System.Web;

namespace ExpressiveAnnotations.MvcWebSample.Misc
{
    public class VerbosityManager
    {
        static VerbosityManager()
        {
            Instance = new VerbosityManager();
        }

        private VerbosityManager()
        {
        }

        public static VerbosityManager Instance { get; }

        public void Save(bool value, HttpContextBase httpContext)
        {
            SetValueToCookie(value, httpContext);
        }

        public bool Load(HttpContextBase httpContext)
        {
            var verbose = GetValueFromCookie(httpContext);
            if (verbose != null)
                return verbose.Value;

            verbose = IsDebug();
            SetValueToCookie(verbose.Value, httpContext);
            return verbose.Value;
        }

        private bool? GetValueFromCookie(HttpContextBase httpContext)
        {
            var cookie = httpContext.Request.Cookies.Get("expressiv.mvcwebsample.verbosity");
            if (cookie == null)
                return null;

            bool result;
            bool.TryParse(cookie.Value, out result);
            return result;
        }

        private void SetValueToCookie(bool value, HttpContextBase httpContext)
        {
            var cookie = new HttpCookie("expressiv.mvcwebsample.verbosity", value.ToString().ToLowerInvariant()) { Expires = DateTime.Now.AddMonths(1) };
            httpContext.Response.SetCookie(cookie);
        }

        private static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}