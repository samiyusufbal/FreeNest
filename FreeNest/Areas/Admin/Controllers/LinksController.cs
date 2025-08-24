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
        private readonly string urlPath;
        private readonly IconHelper _iconHelper;
        public LinksController(IServiceProvider serviceProvider, IMapper mapper, IConfiguration configuration, IconHelper iconHelper) : base(serviceProvider)
        {
            _mapper = mapper;
            _configuration = configuration;
            urlPath = $"/{_configuration["AdminUrl"]}/Links/";
            _iconHelper = iconHelper;
        }

        public IActionResult Index()
        {
            LinkViewModel model = new();
            JsonSerializerOptions options = new() { ReferenceHandler = ReferenceHandler.IgnoreCycles };

            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                model.JsonList = JsonSerializer.Serialize(dBContext.Links
                    .Where(w => w.UserId == int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value))
                    .OrderBy(a => a.Order)
                    .ToList(), options);
            }

            return View(model);
        }

        public IActionResult Add()
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                var model = new LinkDtoModel();

                var userId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var maxOrder = dBContext.Links
                    .Where(l => l.UserId == userId)
                    .Max(l => (int?)l.Order) ?? 0;

                model.Order = maxOrder + 1;

                ViewBag.IconList = _iconHelper.GetIcons();

                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Add([FromForm] LinkDtoModel form)
        {
            ReturnMessage returnMessage = new();

            if (!ModelState.IsValid)
                returnMessage.CreateMessage("Please fill in the required fields!");
            else
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
                    {

                        var IsAvailable = dBContext.Links.Where(b => b.Id.Equals(form.Id)).FirstOrDefault();

                        if (IsAvailable is null)
                        {
                            var model = _mapper.Map<Link>(form);
                            model.UserId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                            dBContext.Links.Add(model);
                            dBContext.SaveChanges();

                            returnMessage.CreateMessage("Successfully added!", ReturnMessageType.Success);
                        }
                        else
                            returnMessage.CreateMessage("Already added!");
                    }
                }
                catch (Exception ex)
                {
                    returnMessage.CreateMessage("Error: " + ex.Message);
                }

            }
            TempData["returnMessage"] = returnMessage.ScriptTxt;

            return Redirect($"{urlPath}Add/");
        }

        public IActionResult Edit(int Id)
        {
            ReturnMessage returnMessage = new();

            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                var model = dBContext.Links.Where(b => b.Id == Id).FirstOrDefault();

                if (model is null)
                    return Redirect("/404");

                var models = _mapper.Map<LinkDtoModel>(model);
                if (models is not null)
                {
                    ViewBag.IconList = _iconHelper.GetIcons();
                    return View(models);
                }

            }
            return View();
        }

        [HttpPost]
        public IActionResult Edit([FromForm] LinkDtoModel form)
        {
            ReturnMessage returnMessage = new();

            if (!ModelState.IsValid)
                returnMessage.CreateMessage("Please fill in the required fields!");
            else
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
                    {
                        var model = dBContext.Links.Where(b => b.Id == form.Id).FirstOrDefault();

                        if (model is not null)
                        {
                            var IsSameEntry = dBContext.Links.Where(b => b.Id == form.Id).FirstOrDefault();
                            var formModel = form;

                            if (IsSameEntry is null)
                            {
                                form.UserId = model.UserId;
                                model.UpdatedAt = DateTime.UtcNow;
                                _mapper.Map(form, model);
                                dBContext.Links.Update(model);
                                dBContext.SaveChanges();

                                returnMessage.CreateMessage("Successfully", ReturnMessageType.Success);
                            }
                            else
                                returnMessage.CreateMessage("There is already this kind of record.", ReturnMessageType.Success);
                        }
                        else
                            returnMessage.CreateMessage("Not found");
                    }
                }
                catch (Exception ex)
                {
                    returnMessage.CreateMessage("Error: " + ex.Message);
                }
            }

            TempData["returnMessage"] = returnMessage.ScriptTxt;

            return Redirect($"{urlPath}Edit/{form.Id}");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(int[] ids, int[] orders)
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                try
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        var link = await dBContext.Links.FindAsync(ids[i]);
                        if (link != null)
                        {
                            link.Order = orders[i];
                            link.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    await dBContext.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }

        public JsonResult Delete(int Id)
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                var model = dBContext.Links.Where(b => b.Id == Id).FirstOrDefault();

                if (model is not null)
                {
                    dBContext.Remove(model);
                    dBContext.SaveChanges();
                    return new JsonResult("success");
                }
                else
                    return new JsonResult("error");
            }
        }

    }
}
