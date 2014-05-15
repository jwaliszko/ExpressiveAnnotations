using System.Web.Mvc;
using ExpressiveAnnotations.MvcWebSample.Models;

namespace ExpressiveAnnotations.MvcWebSample.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var model = new Query
            {
                GoAbroad = true, 
                Country = "Poland", 
                NextCountry = "Other", 
                SportType = "Extreme"
            };
            return View("Home", model);
        }

        [HttpPost]
        public ActionResult Index(Query model)
        {
            if (ModelState.IsValid)
                ViewBag.Success = "Success";            
            return View("Home", model);
        }
    }
}
