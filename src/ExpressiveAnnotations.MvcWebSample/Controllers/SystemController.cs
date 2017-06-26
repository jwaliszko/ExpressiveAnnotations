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

        [HttpPost]
        public JsonResult SetTriggers(string events)
        {
            TriggersManager.Instance.Save(events, HttpContext);
            return Json(new {success = true});
        }

        [HttpPost]
        public JsonResult SetVerbosity(bool value)
        {
            VerbosityManager.Instance.Save(value, HttpContext);
            return Json(new {success = true});
        }
    }
}