using Microsoft.AspNetCore.Mvc;

namespace FreeNest.Controllers
{
    public class BaseController : Controller
    {
        public readonly IServiceProvider _serviceProvider;
        public BaseController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }
}