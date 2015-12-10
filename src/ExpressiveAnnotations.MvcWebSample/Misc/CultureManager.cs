using System;
using System.Globalization;
using System.Web;

namespace ExpressiveAnnotations.MvcWebSample.Misc
{
    public class CultureManager
    {
        static CultureManager()
        {
            Instance = new CultureManager();
        }

        private CultureManager()
        {
        }

        public static CultureManager Instance { get; }

        public void Save(string lang, HttpContextBase httpContext)
        {
            var culture = CultureInfo.CreateSpecificCulture(lang);
            SetValueToCookie(culture, httpContext);
        }

        public CultureInfo Load(HttpContextBase httpContext)
        {
            var culture = GetValueFromCookie(httpContext);
            if (culture != null) 
                return culture;

            culture = CultureInfo.CreateSpecificCulture("en"); // force default culture to be "en"
            SetValueToCookie(culture, httpContext);
            return culture;
        }

        private CultureInfo GetValueFromCookie(HttpContextBase httpContext)
        {
            var cookie = httpContext.Request.Cookies.Get("expressiv.mvcwebsample.culture");
            return cookie != null ? CultureInfo.CreateSpecificCulture(cookie.Value) : null;
        }

        private void SetValueToCookie(CultureInfo culture, HttpContextBase httpContext)
        {
            var cookie = new HttpCookie("expressiv.mvcwebsample.culture", culture.Name) { Expires = DateTime.Now.AddMonths(1) };
            httpContext.Response.SetCookie(cookie);
        }
    }
}
