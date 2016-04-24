using Microsoft.AspNet.Mvc;

namespace Justin.Homepage.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult Error()
        {
            return StatusCode("500");
        }
        public IActionResult NotFound()
        {
            return StatusCode("404");
        }
        public IActionResult StatusCode(string code)
        {
            ViewBag.StatusCode = code;

            return View("StatusCode");
        }
    }
}
