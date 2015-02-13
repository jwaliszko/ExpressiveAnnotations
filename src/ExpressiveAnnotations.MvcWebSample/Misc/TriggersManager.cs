using System;
using System.Web;

namespace ExpressiveAnnotations.MvcWebSample.Misc
{
    public class TriggersManager
    {
        private static readonly TriggersManager _instance = new TriggersManager();

        private TriggersManager()
        {
        }

        public static TriggersManager Instance
        {
            get { return _instance; }
        }

        public void Save(string events, HttpContextBase httpContext)
        {
            SetValueToCookie(events, httpContext);
        }

        public string Load(HttpContextBase httpContext)
        {
            var value = GetValueFromCookie(httpContext);
            if (value != null) 
                return value;

            value = "change paste keyup";
            SetValueToCookie(value, httpContext);
            return value;
        }

        private string GetValueFromCookie(HttpContextBase httpContext)
        {
            var cookie = httpContext.Request.Cookies.Get("expressiv.mvcwebsample.triggers");
            return cookie != null ? cookie.Value : null;
        }

        private void SetValueToCookie(string events, HttpContextBase httpContext)
        {
            var cookie = new HttpCookie("expressiv.mvcwebsample.triggers", events) {Expires = DateTime.Now.AddMonths(1)};
            httpContext.Response.SetCookie(cookie);
        }
    }
}
