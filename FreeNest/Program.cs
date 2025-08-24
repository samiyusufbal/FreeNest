using DATA;
using FreeNest.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MapperProfile>());

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataDbContext>(options =>
    options.UseMySql(
        connection,
        ServerVersion.AutoDetect(connection)
    ));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(cookieOptions =>
    {
        cookieOptions.LoginPath = $"/{builder.Configuration["AdminURL"]}/Auth/Login";
        cookieOptions.Cookie.Name = "UserLoginCookie";
        cookieOptions.AccessDeniedPath = $"/{builder.Configuration["AdminURL"]}/Auth/Login";
        cookieOptions.ReturnUrlParameter = "ReturnUrl";

        cookieOptions.ExpireTimeSpan = TimeSpan.FromDays(2);
        cookieOptions.SlidingExpiration = true;
        cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        cookieOptions.Cookie.HttpOnly = true;
        cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
builder.Services.AddMemoryCache();

app.UseEndpoints(endpoints =>
{
    endpoints.MapAreaControllerRoute(
        name: "Admin",
        areaName: "Admin",
        pattern: "FreeNest/Administrator/{controller=Home}/{action=Index}/{id?}/"
        );
    endpoints.MapDefaultControllerRoute();
});

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DataDbContext>();
    context.Database.SetCommandTimeout(300);
    context.Database.EnsureCreated();
}

app.Run();