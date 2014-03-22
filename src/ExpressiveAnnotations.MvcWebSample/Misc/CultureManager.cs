using System;
using System.Globalization;
using System.Web;

namespace ExpressiveAnnotations.MvcWebSample.Misc
{
    public class CultureManager
    {
        private static readonly CultureManager _instance = new CultureManager();
        private CultureManager()
        {
        }

        public static CultureManager Instance
        {
            get { return _instance; }
        }

        public void Save(string lang, HttpContextBase httpContext)
        {
            var culture = CultureInfo.CreateSpecificCulture(lang);
            SetCultureToCookie(culture, httpContext);
        }

        public CultureInfo Load(HttpContextBase httpContext)
        {
            var culture = GetCultureFromCookie(httpContext);
            if (culture == null)
            {
                culture = GetCultureFromAcceptLangHttpHeader(httpContext);
                culture = culture ?? CultureInfo.CreateSpecificCulture("en");
                SetCultureToCookie(culture, httpContext);
            }
            return culture;
        }

        private CultureInfo GetCultureFromCookie(HttpContextBase httpContext)
        {
            var cookie = httpContext.Request.Cookies.Get("expressiv.mvcwebsample.culture");
            return cookie != null ? CultureInfo.CreateSpecificCulture(cookie.Value) : null;
        }

        private CultureInfo GetCultureFromAcceptLangHttpHeader(HttpContextBase httpContext)
        {
            return httpContext.Request.UserLanguages != null &&
                   httpContext.Request.UserLanguages.Length != 0
                       ? CultureInfo.CreateSpecificCulture(httpContext.Request.UserLanguages[0].Substring(0, 2))
                       : null;
        }

        private void SetCultureToCookie(CultureInfo culture, HttpContextBase httpContext)
        {
            var cookie = new HttpCookie("expressiv.mvcwebsample.culture", culture.Name) { Expires = DateTime.Now.AddMonths(1) };
            httpContext.Response.SetCookie(cookie);
        }
    }
}