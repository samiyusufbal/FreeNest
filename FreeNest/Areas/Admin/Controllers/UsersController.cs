using DATA;
using FreeNest.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FreeNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class UsersController : BaseController
    {
        private const string FilterAll = "all";
        private const string FilterDeleted = "deleted";

        public UsersController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public IActionResult Index([FromQuery] string? filter = null)
        {
            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            IQueryable<DATA.Models.User> query = db.Users;

            if (filter == FilterDeleted)
                query = query.Where(u => u.DeletedAt != null);
            else if (filter != FilterAll)
                query = query.Where(u => u.DeletedAt == null);

            var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };

            var model = new UserViewModel
            {
                JsonList = JsonSerializer.Serialize(query.OrderBy(u => u.Id).ToList(), options)
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            var user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                return Json(new { success = false, message = "User not found" });

            user.DeletedAt = DateTime.UtcNow;
            db.Users.Update(user);
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}