using Microsoft.AspNetCore.Mvc;

namespace FreeNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Request.Path == "/FreeNest/Administrator")
                return RedirectPermanent("/FreeNest/Administrator/");
            return View();
        }
    }
}
