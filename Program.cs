using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// EF Core DbContext
builder.Services.AddDbContext<QLHV.Models.QlhvContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Use Session
app.UseSession();

app.UseAuthorization();

// Custom middleware to check authentication and redirect to login if not authenticated
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    
    // Allow access to login, register, and static files without authentication
    if (path == "/account/login" || path == "/account/register" || 
        path.StartsWith("/css/") || path.StartsWith("/js/") || path.StartsWith("/lib/") ||
        path.StartsWith("/images/") || path.StartsWith("/favicon"))
    {
        await next();
        return;
    }
    
    // Check if user is authenticated via session
    var userId = context.Session.GetString("UserId");
    if (string.IsNullOrEmpty(userId))
    {
        // Redirect to login page if not authenticated
        context.Response.Redirect("/Account/Login");
        return;
    }
    
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
