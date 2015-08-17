using System;
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
                SportType = "Extreme",
                AgreeForContact = false,
                LatestSuggestedReturnDate = DateTime.Today.AddMonths(1)
            };

            ViewBag.Success = TempData["Success"];

            return View("Home", model);
        }

        [HttpPost]
        public ActionResult Index(Query model)
        {
            if (ModelState.IsValid)
            {
                TempData["Success"] = "Query successfully submitted";
                return RedirectToAction("Index"); // PRG to avoid subsequent form submission attempts on page refresh (http://en.wikipedia.org/wiki/Post/Redirect/Get)
            }

            return View("Home", model);
        }

        [HttpGet]
        public JsonResult IsUnique(string email)
        {
            System.Threading.Thread.Sleep(1000);
            return Json(email != "a@b.c", JsonRequestBehavior.AllowGet);
        }
    }
}
