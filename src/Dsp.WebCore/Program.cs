using Dsp.Data;
using Dsp.Data.Entities;
using Dsp.Services;
using Dsp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();
builder.Services.AddHttpForwarder();

// Add services to the container.
builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var appConfig = builder.Configuration;
var connectionString = appConfig.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DspDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentity<User, Role>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredUniqueChars = 1;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<DspDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<ILaundryService, LaundryService>();
builder.Services.AddScoped<IMealService, MealService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<ISoberService, SoberService>();
builder.Services.AddScoped<IStatusService, StatusService>();

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSystemWebAdapters();


app.MapAreaControllerRoute("Alumni_default", "Alumni", "Alumni/{controller}/{action=Index}/{id?}");

app.MapAreaControllerRoute("Scholarships_default", "Scholarships", "Scholarships/{controller}/{action=Index}/{id?}");

app.MapAreaControllerRoute("School_default", "School", "School/{controller}/{action=Index}/{id?}");

app.MapAreaControllerRoute("Members_default", "Members", "Members/{controller}/{action=Index}/{id?}");

app.MapAreaControllerRoute("Service_default", "Service", "Service/{controller}/{action=Index}/{id?}");

app.MapAreaControllerRoute("Sobers_default", "Sobers", "Sobers/{controller}/{action=Index}/{id?}");

app.MapAreaControllerRoute("Admin_default", "Admin", "Admin/{controller}/{action=Index}/{id?}");

app.MapAreaControllerRoute("House_default", "House", "House/{controller}/{action=Index}/{id?}");

app.MapAreaControllerRoute("Kitchen_default", "Kitchen", "Kitchen/{controller}/{action=Index}/{id?}");

app.MapAreaControllerRoute("Laundry_default", "Laundry", "Laundry/{controller}/{action=Index}/{id?}");

//app.MapRazorPages();

//app.MapDefaultControllerRoute();

//app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.MapControllerRoute(
      name: "HomeActionOnly",
      pattern: "{action}",
      defaults: new { controller = "Home", action = "Index" });

app.Run();
