using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Justin.Homepage.Controllers
{
    public class FileController : BaseController
    {
        [HttpGet]
        public IActionResult ShareImg()
        {
            return File("images/share_img.png", "image/png");
        }
    }
}
