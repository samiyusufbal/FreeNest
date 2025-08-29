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
        private readonly string _hashKey;
        private readonly string _adminUrl;

        private const int CookieExpireDays = 3;
        private const int AvatarMaxSize = 300;
        private const string AvatarUploadFolder = "wwwroot/uploads/avatars";

        public AuthController(IServiceProvider serviceProvider, IMapper mapper, IConfiguration configuration)
            : base(serviceProvider)
        {
            _mapper = mapper;
            _configuration = configuration;
            _hashKey = _configuration.GetSection("DefaultValues").GetValue<string>("HasherVal") ?? "79H^W$*5f0604^7$$#^!3F";
            _adminUrl = $"/{_configuration["AdminUrl"]}/";
        }

        [AllowAnonymous]
        public IActionResult Login() => View();

        [AllowAnonymous, HttpPost]
        public IActionResult Login([FromForm] UserLoginModel userLogin, [FromForm] string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(userLogin.Email) || string.IsNullOrWhiteSpace(userLogin.Password))
                return HandleError("Email or password cannot be left blank!!", userLogin);

            if (!ValidationHelper.IsValidEmail(userLogin.Email))
                return HandleError("Invalid email format.", userLogin);

            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            var dbUser = db.Users.FirstOrDefault(u => u.DeletedAt == null && u.Email == userLogin.Email);
            if (dbUser is null)
                return HandleError("No such user found!", userLogin);

            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(_hashKey, dbUser.PasswordHash, userLogin.Password);

            if (result != PasswordVerificationResult.Success)
                return HandleError("Password verification failed!", userLogin);

            var sessionUser = _mapper.Map<UserSessionModel>(dbUser);
            SignInUser(sessionUser);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return Redirect(_adminUrl);
        }

        [AllowAnonymous]
        public IActionResult Register() => View(new UserRegisterModel());

        [AllowAnonymous, HttpPost]
        public IActionResult Register([FromForm] UserRegisterModel userRegister)
        {
            if (!ModelState.IsValid)
                return HandleError("Verification error!", userRegister);

            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            if (db.Users.Any(u => u.DeletedAt == null &&
                (u.Email == userRegister.Email || u.Username == userRegister.Username)))
            {
                return HandleError("This email or username is already registered!", userRegister);
            }

            if (!ValidationHelper.IsValidEmail(userRegister.Email))
                return HandleError("Invalid email format.", userRegister);

            string? avatarUrl = SaveAvatar(userRegister.AvatarFile);
            if (userRegister.AvatarFile != null && avatarUrl is null)
                return HandleError("Please upload a valid image file!", userRegister);

            var newUser = _mapper.Map<User>(userRegister);
            newUser.PasswordHash = new PasswordHasher<string>().HashPassword(_hashKey, userRegister.Password);
            newUser.AvatarUrl = avatarUrl;
            newUser.CreatedAt = newUser.UpdatedAt = DateTime.UtcNow;
            newUser.UserPermission = "user";

            db.Users.Add(newUser);
            db.SaveChanges();

            SignInUser(_mapper.Map<UserSessionModel>(newUser));

            TempData["returnMessage"] = new ReturnMessage()
                .CreateMessage("Account created successfully! Welcome to FreeNest.", ReturnMessageType.Success)
                .ScriptTxt;

            return Redirect(_adminUrl);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("UserLoginCookie");
            return Redirect($"{_adminUrl}Auth/Login");
        }

        [Authorize]
        public IActionResult EditProfile(string status = null)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            var user = db.Users.FirstOrDefault(b => b.Id == userId);
            if (user == null) return Redirect("/404");

            var response = new ReturnMessage<UserProfileDtoModel>
            {
                model = _mapper.Map<UserProfileDtoModel>(user)
            };

            if (status == "success")
                response.CreateMessage("Successful!", ReturnMessageType.Success);
            else if (!string.IsNullOrEmpty(status))
                response.CreateMessage(System.Web.HttpUtility.UrlDecode(status), ReturnMessageType.Error);

            return View(response);
        }

        [HttpPost]
        public IActionResult EditProfile([FromForm] UserProfileDtoModel form)
        {
            if (!ModelState.IsValid)
                return HandleError("Verification error!", form);

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using var scope = _serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            var user = db.Users.FirstOrDefault(u => u.Id == userId && u.DeletedAt == null);
            if (user is null) return RedirectWithStatus("NOT FOUND!");

            if (!ValidationHelper.IsValidEmail(form.Email))
                return RedirectWithStatus("Incorrect email address!");

            if (db.Users.Any(u => u.Id != userId && u.Email == form.Email))
                return RedirectWithStatus("This email already exists!");

            _mapper.Map(form, user);
            user.UpdatedAt = DateTime.UtcNow;

            string? avatarUrl = SaveAvatar(form.AvatarFile, user.AvatarUrl);
            if (form.AvatarFile != null && avatarUrl is null)
                return RedirectWithStatus("Please upload a valid image file!");
            user.AvatarUrl = avatarUrl ?? user.AvatarUrl;

            if (!string.IsNullOrEmpty(form.PasswordHash))
            {
                if (form.PasswordHash.Length < 8)
                    return RedirectWithStatus("Password must be at least 8 characters!");

                user.PasswordHash = new PasswordHasher<string>().HashPassword(_hashKey, form.PasswordHash);
            }

            db.Users.Update(user);
            db.SaveChanges();

            return Redirect($"{_adminUrl}Auth/Logout");
        }

        // --- Private Helpers ---
        private void SignInUser(UserSessionModel user)
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
                ExpiresUtc = DateTimeOffset.Now.AddDays(CookieExpireDays),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.Now
            };

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private string? SaveAvatar(IFormFile? file, string? oldFilePath = null)
        {
            if (file is null || file.Length == 0) return null;
            if (!ImageHelper.IsValidImage(file.FileName)) return null;

            Directory.CreateDirectory(AvatarUploadFolder);

            if (!string.IsNullOrEmpty(oldFilePath))
                ImageHelper.RemoveFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldFilePath.TrimStart('/')));

            using var stream = file.OpenReadStream();
            var savedFileName = ImageHelper.SaveImage(stream, file.FileName, AvatarUploadFolder, AvatarMaxSize);

            return savedFileName.StartsWith("/") ? savedFileName : "/" + savedFileName;
        }


        private IActionResult HandleError(string message, object model)
        {
            TempData["returnMessage"] = new ReturnMessage().CreateMessage(message).ScriptTxt;
            return View(model);
        }
        private IActionResult HandleError(string message, UserProfileDtoModel model)
        {
            var response = new ReturnMessage<UserProfileDtoModel>
            {
                model = model
            };
            response.CreateMessage(message, ReturnMessageType.Error);
            return View(response);
        }

        private IActionResult RedirectWithStatus(string status) =>
            Redirect($"{_adminUrl}/Auth/EditProfile/?status=" + System.Web.HttpUtility.UrlEncode(status));
    }
}
