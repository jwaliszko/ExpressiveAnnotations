using System.Threading.Tasks;
using System.Web.Http;
using ExpressiveAnnotations.MvcWebSample.Models;

namespace ExpressiveAnnotations.MvcWebSample.Controllers
{
    [System.Web.Http.RoutePrefix("api/Default")]
    public class DefaultController : ApiController
    {
        // POST api/Default/Save
        [System.Web.Http.Route("Save")]
        public async Task<IHttpActionResult> Save(Query model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await Task.Delay(1);
            return Ok();
        }
    }
}
