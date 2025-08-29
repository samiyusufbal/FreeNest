using DATA;
using FreeNest.Models;
using FreeNest.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FreeNest.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public IActionResult Index(string? username)
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                if (string.IsNullOrEmpty(username))
                {
                    UserViewModel allUsersModel = new();
                    allUsersModel.Users = dBContext.Users.Include(i => i.Links).ToList();

                    return View("HomePage", allUsersModel);
                }

                LinkViewModel model = new();

                var userModel = dBContext.Users.Where(b => b.Username == username).FirstOrDefault();
                if (userModel is null)
                    return Redirect("/404");

                model.User = userModel;
                model.Links = dBContext.Links.Where(b => b.UserId == userModel.Id).OrderBy(a => a.Order).ToList();

                return View("Index", model);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
