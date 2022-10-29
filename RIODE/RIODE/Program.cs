using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RIODE.DAL;
using RIODE.Models;
using RIODE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
builder.Services.AddDbContext<RiodeContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("default"));
});
builder.Services.AddIdentity<AppUser, IdentityRole>(con => {
    con.Password.RequiredLength = 8;
    con.Password.RequireNonAlphanumeric = false;
    con.User.RequireUniqueEmail = true;
    con.Lockout.MaxFailedAccessAttempts = 5;
    con.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    con.Lockout.AllowedForNewUsers = false;
}).AddDefaultTokenProviders()
  .AddEntityFrameworkStores<RiodeContext>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<LayoutService>();
builder.Services.AddHttpContextAccessor();

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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

