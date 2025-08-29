using AutoMapper;
using DATA;
using DATA.Enums;
using DATA.Models;
using FreeNest.Helpers;
using FreeNest.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FreeNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LinksController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IconHelper _iconHelper;
        private readonly string _urlPath;

        public LinksController(IServiceProvider serviceProvider, IMapper mapper, IConfiguration configuration, IconHelper iconHelper)
            : base(serviceProvider)
        {
            _mapper = mapper;
            _configuration = configuration;
            _iconHelper = iconHelper;
            _urlPath = $"/{_configuration["AdminUrl"]}/Links/";
        }

        public IActionResult Index()
        {
            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            var userId = GetCurrentUserId();
            var links = db.Links
                .Where(w => w.UserId == userId)
                .OrderBy(a => a.Order)
                .ToList();

            var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };

            var model = new LinkViewModel
            {
                JsonList = JsonSerializer.Serialize(links, options)
            };

            return View(model);
        }

        public IActionResult Add()
        {
            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            var userId = GetCurrentUserId();
            var maxOrder = db.Links.Where(l => l.UserId == userId).Max(l => (int?)l.Order) ?? 0;

            var model = new LinkDtoModel { Order = maxOrder + 1 };
            SetIconList();

            return View(model);
        }

        [HttpPost]
        public IActionResult Add([FromForm] LinkDtoModel form)
        {
            if (!ModelState.IsValid)
                return HandleError("Please fill in the required fields!", $"{_urlPath}Add/");

            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            try
            {
                var exists = db.Links.Any(b => b.Id == form.Id);
                if (exists)
                    return HandleError("Already added!", $"{_urlPath}Add/");

                var model = _mapper.Map<Link>(form);
                model.UserId = GetCurrentUserId();

                db.Links.Add(model);
                db.SaveChanges();

                return HandleSuccess("Successfully added!", $"{_urlPath}Add/");
            }
            catch (Exception ex)
            {
                return HandleError("Error: " + ex.Message, $"{_urlPath}Add/");
            }
        }

        public IActionResult Edit(int id)
        {
            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            var model = db.Links.FirstOrDefault(b => b.Id == id);
            if (model is null)
                return Redirect("/404");

            var dto = _mapper.Map<LinkDtoModel>(model);
            SetIconList();
            return View(dto);
        }

        [HttpPost]
        public IActionResult Edit([FromForm] LinkDtoModel form)
        {
            if (!ModelState.IsValid)
                return HandleError("Please fill in the required fields!", $"{_urlPath}Edit/{form.Id}");

            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            try
            {
                var model = db.Links.FirstOrDefault(b => b.Id == form.Id);
                if (model is null)
                    return HandleError("Not found!", $"{_urlPath}Edit/{form.Id}");

                // Kullanıcı ID'sini değiştirmiyoruz
                form.UserId = model.UserId;
                model.UpdatedAt = DateTime.UtcNow;

                _mapper.Map(form, model);
                db.Links.Update(model);
                db.SaveChanges();

                return HandleSuccess("Successfully updated!", $"{_urlPath}Edit/{form.Id}");
            }
            catch (Exception ex)
            {
                return HandleError("Error: " + ex.Message, $"{_urlPath}Edit/{form.Id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(int[] ids, int[] orders)
        {
            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            try
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    var link = await db.Links.FindAsync(ids[i]);
                    if (link != null)
                    {
                        link.Order = orders[i];
                        link.UpdatedAt = DateTime.UtcNow;
                    }
                }
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            var model = db.Links.FirstOrDefault(b => b.Id == id);
            if (model is null)
                return new JsonResult("error");

            db.Links.Remove(model);
            db.SaveChanges();
            return new JsonResult("success");
        }

        // --- Private Helpers ---
        private int GetCurrentUserId() =>
            int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        private void SetIconList() =>
            ViewBag.IconList = _iconHelper.GetIcons() ?? new List<string>();


        private IActionResult HandleError(string message, string redirectUrl)
        {
            var rm = new ReturnMessage();
            rm.CreateMessage(message);
            TempData["returnMessage"] = rm.ScriptTxt;
            return Redirect(redirectUrl);
        }

        private IActionResult HandleSuccess(string message, string redirectUrl)
        {
            var rm = new ReturnMessage();
            rm.CreateMessage(message, ReturnMessageType.Success);
            TempData["returnMessage"] = rm.ScriptTxt;
            return Redirect(redirectUrl);
        }
    }
}
