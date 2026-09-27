using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using pr3withDB.Models;

var builder = WebApplication.CreateBuilder(args);

//      Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//    
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Force UTF-8 for Razor
builder.Services.AddSingleton(System.Text.Encodings.Web.HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    try
    {
        context.Database.ExecuteSqlRaw("IF COL_LENGTH('Products', 'product_image_url') IS NULL ALTER TABLE Products ADD product_image_url nvarchar(500) NULL");
    }
    catch { }

    try
    {
        context.Database.ExecuteSqlRaw("IF COL_LENGTH('CartItems', 'user_id') IS NULL ALTER TABLE CartItems ADD user_id nvarchar(450) NULL");
        context.Database.ExecuteSqlRaw("ALTER TABLE CartItems ALTER COLUMN cart_ID int NULL");
    }
    catch { }

    try
    {
        context.Database.ExecuteSqlRaw(@"
            BEGIN TRY
                ALTER TABLE Users ALTER COLUMN user_password_hash nvarchar(255) NOT NULL;
            END TRY
            BEGIN CATCH
                ALTER TABLE Users DROP COLUMN user_password_hash;
                ALTER TABLE Users ADD user_password_hash nvarchar(255) NOT NULL DEFAULT '';
            END CATCH
        ");
    }
    catch { }

    try
    {
        // Гарантируем существование роли "User"
        if (!context.Roles.Any(r => r.RoleName == "User"))
        {
            context.Roles.Add(new Role { RoleName = "User" });
            context.SaveChanges();
        }

        // Гарантируем существование роли "Admin"
        if (!context.Roles.Any(r => r.RoleName == "Admin"))
        {
            context.Roles.Add(new Role { RoleName = "Admin" });
            context.SaveChanges();
        }

        var adminRole = context.Roles.First(r => r.RoleName == "Admin");
        var adminUser = context.Users.FirstOrDefault(u => u.UserLogin == "admin");
        if (adminUser == null)
        {
            context.Users.Add(new User
            {
                UserLogin = "admin",
                UserPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                RoleId = adminRole.IdRoles,
                UserCreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }
        else
        {
            adminUser.UserPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
            adminUser.RoleId = adminRole.IdRoles;
            context.SaveChanges();
        }
    }
    catch { }
}
// Middleware to force Content-Type header for HTML views
app.Use((context, next) => 
{ 
    if (!context.Request.Path.StartsWithSegments("/api") && !context.Request.Path.StartsWithSegments("/swagger"))
    {
        context.Response.Headers.ContentType = "text/html; charset=utf-8"; 
    }
    return next(); 
});

// Swagger Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API v1");
    c.RoutePrefix = "swagger";
});

//   
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllers();

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

//  :      Products
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();