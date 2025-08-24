using AutoMapper;
using DATA;
using DATA.Enums;
using DATA.Models;
using FreeNest.Helpers;
using FreeNest.Models;
using FreeNest.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreeNest.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class AuthController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly string key = "79H^W$*5f0604^7$$#^!3F";
        private readonly string urlPath;

        public AuthController(IServiceProvider serviceProvider, IMapper mapper, IConfiguration configuration) : base(serviceProvider)
        {
            _mapper = mapper;
            _configuration = configuration;
            key = _configuration.GetSection("DefaultValues").GetValue<string>("HasherVal") ?? key;
            urlPath = $"/{_configuration["AdminUrl"]}/";
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromForm] UserLoginModel userLogin, [FromForm] string returnUrl)
        {
            ReturnMessage returnMessage = new();
            UserSessionModel user = new();

            var passwordHasher = new PasswordHasher<string>();

            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                var model = dBContext.Users
                    .Where(u => u.DeletedAt == null && u.Email == userLogin.Email)
                    .FirstOrDefault();

                if (userLogin.Email != null && userLogin.Password != null)
                {
                    if (ValidationHelper.IsValidEmail(userLogin.Email))
                    {
                        if (model != null)
                        {
                            if (passwordHasher.VerifyHashedPassword(key, model.PasswordHash, userLogin.Password) == PasswordVerificationResult.Success)
                            {
                                this._mapper.Map(model, user);
                                if (user.FullName != null)
                                {
                                    var claims = new List<Claim>
                                    {
                                        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                        new(ClaimTypes.Role, user.UserPermission)
                                    };

                                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                    var authProperties = new AuthenticationProperties
                                    {
                                        AllowRefresh = true,
                                        ExpiresUtc = DateTimeOffset.Now.AddDays(3),
                                        IsPersistent = true,
                                        IssuedUtc = DateTimeOffset.Now
                                    };

                                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                        return Redirect(returnUrl);

                                    return Redirect(urlPath);
                                }
                            }
                            else
                                returnMessage.CreateMessage("Password verification failed!");
                        }
                        else
                            returnMessage.CreateMessage("No such user found!");
                    }
                    else
                        returnMessage.CreateMessage("Please check the type of email.");
                }
                else
                    returnMessage.CreateMessage("Email or password cannot be left blank!!");
            }

            TempData["returnMessage"] = returnMessage.ScriptTxt;
            return View(userLogin);
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new UserRegisterModel());
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register([FromForm] UserRegisterModel userRegister)
        {
            ReturnMessage returnMessage = new();
            var passwordHasher = new PasswordHasher<string>();

            if (!ModelState.IsValid)
            {
                returnMessage.CreateMessage("Verification error!");
                TempData["returnMessage"] = returnMessage.ScriptTxt;
                return View(userRegister);
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                try
                {
                    var existingUser = dBContext.Users
                        .Where(u => u.DeletedAt == null && (u.Email == userRegister.Email || u.Username == userRegister.Username))
                        .FirstOrDefault();

                    if (existingUser != null)
                    {
                        if (existingUser.Email == userRegister.Email)
                            returnMessage.CreateMessage("This email address is already registered!");
                        else
                            returnMessage.CreateMessage("This username is already taken!");

                        TempData["returnMessage"] = returnMessage.ScriptTxt;
                        return View(userRegister);
                    }

                    if (!ValidationHelper.IsValidEmail(userRegister.Email))
                    {
                        returnMessage.CreateMessage("Please check the type of email.");
                        TempData["returnMessage"] = returnMessage.ScriptTxt;
                        return View(userRegister);
                    }

                    string? avatarPath = null;
                    if (userRegister.AvatarFile != null && userRegister.AvatarFile.Length > 0)
                    {
                        if (ImageHelper.IsValidImage(userRegister.AvatarFile.FileName))
                        {
                            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                            Directory.CreateDirectory(uploadsPath);

                            using (var stream = userRegister.AvatarFile.OpenReadStream())
                            {
                                var savedFileName = ImageHelper.SaveImage(stream, userRegister.AvatarFile.FileName, uploadsPath, 300); // 300px max avatar
                                avatarPath = $"/uploads/avatars/{savedFileName}";
                            }
                        }
                        else
                        {
                            returnMessage.CreateMessage("Please upload a valid image file (jpg, jpeg, png, gif)!");
                            TempData["returnMessage"] = returnMessage.ScriptTxt;
                            return View(userRegister);
                        }
                    }

                    var newUser = _mapper.Map<User>(userRegister);

                    // Override specific properties
                    newUser.PasswordHash = passwordHasher.HashPassword(key, userRegister.Password);
                    newUser.AvatarUrl = avatarPath;
                    newUser.Theme = userRegister.Theme ?? "system";
                    newUser.Language = userRegister.Language ?? "en";
                    newUser.CreatedAt = DateTime.UtcNow;
                    newUser.UpdatedAt = DateTime.UtcNow;
                    newUser.UserPermission = "user";
                    newUser.IsEmailVerified = false;
                    newUser.DeletedAt = null;

                    dBContext.Users.Add(newUser);
                    dBContext.SaveChanges();

                    var userSession = _mapper.Map<UserSessionModel>(newUser);
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
                        new(ClaimTypes.Role, newUser.UserPermission)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.Now.AddDays(3),
                        IsPersistent = true,
                        IssuedUtc = DateTimeOffset.Now
                    };

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    returnMessage.CreateMessage("Account created successfully! Welcome to FreeNest.", ReturnMessageType.Success);
                    TempData["returnMessage"] = returnMessage.ScriptTxt;

                    return Redirect(urlPath);
                }
                catch (Exception ex)
                {
                    returnMessage.CreateMessage($"An error occurred while creating your account: {ex.Message}");
                    TempData["returnMessage"] = returnMessage.ScriptTxt;
                    return View(userRegister);
                }
            }
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("UserLoginCookie");
            return Redirect($"{urlPath}Auth/Login");
        }


        [Authorize]
        public IActionResult EditProfile(string status = null)
        {
            int userId = int.Parse(HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            ReturnMessage<UserProfileDtoModel> formResponse = new();

            using (var scope = _serviceProvider.CreateScope())
            using (var dBContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
            {
                var model = dBContext.Users.Where(b => b.Id == userId).FirstOrDefault();

                if (model is null)
                    return Redirect("/404");

                formResponse.model = _mapper.Map<UserProfileDtoModel>(model);

                if (status is "success")
                    formResponse.CreateMessage("Successful!", ReturnMessageType.Success);
                else if (status is not null)
                    formResponse.CreateMessage(System.Web.HttpUtility.UrlDecode(status), ReturnMessageType.Error);
            }
            return View(formResponse);
        }

        [HttpPost]
        public IActionResult EditProfile([FromForm] UserProfileDtoModel form)
        {
            ReturnMessage<UserProfileDtoModel> formResponse = new();
            int userId = int.Parse(HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier).Value);
            string returnMsg = "";

            if (!ModelState.IsValid)
            {
                formResponse.CreateMessage("Verification error!");
                return View(formResponse);
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                using (var dbContext = scope.ServiceProvider.GetRequiredService<DataDbContext>())
                {
                    var model = dbContext.Users.Where(b => b.Id == userId && b.DeletedAt == null).FirstOrDefault();

                    if (model is not null)
                    {
                        var IsSameEntry = dbContext.Users.Where(a => a.Id != userId && a.Email.Equals(form.Email)).FirstOrDefault();
                        if (ValidationHelper.IsValidEmail(form.Email))
                        {
                            formResponse.model = form;

                            if (IsSameEntry is null)
                            {
                                var currentAvatarUrl = model.AvatarUrl;

                                _mapper.Map(form, model);

                                model.UpdatedAt = DateTime.UtcNow;

                                if (form.AvatarFile != null && form.AvatarFile.Length > 0)
                                {
                                    if (ImageHelper.IsValidImage(form.AvatarFile.FileName))
                                    {
                                        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                                        Directory.CreateDirectory(uploadsPath);

                                        if (!string.IsNullOrEmpty(currentAvatarUrl))
                                        {
                                            var oldAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", currentAvatarUrl.TrimStart('/'));
                                            ImageHelper.RemoveFile(oldAvatarPath);
                                        }

                                        using (var stream = form.AvatarFile.OpenReadStream())
                                        {
                                            var savedFileName = ImageHelper.SaveImage(stream, form.AvatarFile.FileName, uploadsPath, 300);
                                            model.AvatarUrl = $"/uploads/avatars/{savedFileName}";
                                        }
                                    }
                                    else
                                    {
                                        returnMsg = "Please upload a valid image file!";
                                        return Redirect($"/{urlPath}/Auth/EditProfile/?status=" + System.Web.HttpUtility.UrlEncode(returnMsg));
                                    }
                                }
                                else
                                {
                                    model.AvatarUrl = currentAvatarUrl;
                                }

                                if (form.PasswordHash is not null)
                                {
                                    if (form.PasswordHash.Length < 8)
                                    {
                                        returnMsg = "The password must be longer than 8 characters!";
                                        return Redirect($"/{urlPath}/Auth/EditProfile/?status=" + System.Web.HttpUtility.UrlEncode(returnMsg));
                                    }
                                    else
                                    {
                                        var passwordHasher = new PasswordHasher<string>();
                                        model.PasswordHash = passwordHasher.HashPassword(key, form.PasswordHash);
                                    }
                                }

                                dbContext.Users.Update(model);
                                dbContext.SaveChanges();

                                return Redirect($"{urlPath}/Auth/Logout");
                            }
                            else
                                returnMsg = "This entry already exists!";
                        }
                        else
                            returnMsg = "Incorrect email address!";
                    }
                    else
                        returnMsg = "NOT FOUND!";

                    return Redirect($"/{urlPath}/Auth/EditProfile/?status=" + System.Web.HttpUtility.UrlEncode(returnMsg));
                }
            }
            catch (Exception e)
            {
                return Redirect($"/{urlPath}/Auth/EditProfile/?status=" + System.Web.HttpUtility.UrlEncode(e.Message));
            }
        }
    }
}