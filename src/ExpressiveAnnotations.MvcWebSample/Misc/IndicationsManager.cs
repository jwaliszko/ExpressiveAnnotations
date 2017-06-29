using System;
using System.Web;

namespace ExpressiveAnnotations.MvcWebSample.Misc
{
    public class IndicationsManager
    {
        static IndicationsManager()
        {
            Instance = new IndicationsManager();
        }

        private IndicationsManager()
        {
        }

        public static IndicationsManager Instance { get; }

        public void Save(string value, HttpContextBase httpContext)
        {
            SetValueToCookie(value, httpContext);
        }

        public string Load(HttpContextBase httpContext)
        {
            var indication = GetValueFromCookie(httpContext);
            if (indication != null)
                return indication;

            indication = "asterisks"; // asterisks by default
            SetValueToCookie(indication, httpContext);
            return indication;
        }

        private string GetValueFromCookie(HttpContextBase httpContext)
        {
            var cookie = httpContext.Request.Cookies.Get("expressiv.mvcwebsample.indication");
            return cookie?.Value;
        }

        private void SetValueToCookie(string value, HttpContextBase httpContext)
        {
            var cookie = new HttpCookie("expressiv.mvcwebsample.indication", value) { Expires = DateTime.Now.AddMonths(1) };
            httpContext.Response.SetCookie(cookie);
        }
    }
}