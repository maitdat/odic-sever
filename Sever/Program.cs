using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sever;
using Sever.Entity;
using Sever.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IAuthCodeRepository, AuthCodeRepository>();

var app = builder.Build();

// Seed users
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var alice = new User { UserName = "alice", Email = "alice@example.com" };
    var bob = new User { UserName = "bob", Email = "bob@example.com" };

    if (userManager.FindByNameAsync(alice.UserName).Result == null)
    {
        userManager.CreateAsync(alice, "Password123!").Wait();
    }

    if (userManager.FindByNameAsync(bob.UserName).Result == null)
    {
        userManager.CreateAsync(bob, "Password123!").Wait();
    }
}

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
app.UseAuthorization();

app.MapGet("/.well-known/openid-configuration", () =>
    Results.File(Path.Combine(builder.Environment.ContentRootPath, "OidcDiscovery", "openid-configuration.json"), contentType: "application/json"));

app.MapGet("/.well-known/jwks.json", () =>
    Results.File(Path.Combine(builder.Environment.ContentRootPath, "OidcDiscovery", "jwks.json"), contentType: "application/json"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
