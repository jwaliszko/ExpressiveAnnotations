using System.Web.Mvc;
using ExpressiveAnnotations.MvcWebSample.Models;
using System;

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
                LatestSuggestedReturnDate = DateTime.Today.AddMonths(1),
                Today = DateTime.Today,
                ContactDetails = new Contact()
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
