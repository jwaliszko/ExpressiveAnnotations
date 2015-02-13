using System.Web.Mvc;
using ExpressiveAnnotations.MvcWebSample.Misc;

namespace ExpressiveAnnotations.MvcWebSample.Controllers
{
    public class SystemController : Controller
    {
        public ActionResult SetCulture(string lang, string returnUrl)
        {
            CultureManager.Instance.Save(lang, HttpContext);
            return Redirect(returnUrl);
        }

        public ActionResult SetValidation(string type, string returnUrl)
        {
            ValidationManager.Instance.Save(type, HttpContext);
            return Redirect(returnUrl);
        }

        public ActionResult SetTriggers(string events, string returnUrl)
        {
            TriggersManager.Instance.Save(events, HttpContext);
            return Redirect(returnUrl);
        }
    }
}