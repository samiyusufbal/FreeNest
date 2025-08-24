using DATA;
using FreeNest.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FreeNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : BaseController
    {
        public UsersController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public IActionResult Index([FromQuery] string filter = null)
        {
            UserViewModel model = new();
            JsonSerializerOptions options = new() { ReferenceHandler = ReferenceHandler.IgnoreCycles };

            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                if (filter is "all")
                {
                    model.JsonList = JsonSerializer.Serialize(dBContext.Users
                        .OrderBy(a => a.Id)
                        .ToList(), options);
                }
                else if (filter is "deleted")
                {
                    model.JsonList = JsonSerializer.Serialize(dBContext.Users
                        .Where(a => a.DeletedAt != null)
                        .OrderBy(a => a.Id)
                        .ToList(), options);
                }
                else
                {
                    model.JsonList = JsonSerializer.Serialize(dBContext.Users
                        .Where(a => a.DeletedAt == null)
                        .OrderBy(a => a.Id)
                        .ToList(), options);
                }
            }
            return View(model);
        }
        public JsonResult Delete(int Id)
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                var model = dBContext.Users.Where(b => b.Id == Id).FirstOrDefault();

                if (model is not null)
                {
                    model.DeletedAt = DateTime.Now;
                    dBContext.Update(model);
                    dBContext.SaveChanges();
                    return new JsonResult("success");
                }
                else
                    return new JsonResult("error");
            }
        }
    }
}