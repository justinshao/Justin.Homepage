using Microsoft.AspNet.Mvc;

namespace Justin.Homepage.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
