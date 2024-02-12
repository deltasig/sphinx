using Dsp.Data;
using Dsp.Data.Entities;
using Dsp.Services;
using Dsp.Services.Interfaces;
using Dsp.WebCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();
//builder.Services.AddHttpForwarder();

// Add services to the container.
//builder.Services
//    .AddMvcCore(options => options.EnableEndpointRouting = false);
builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var appConfig = builder.Configuration;
var connectionString = appConfig.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DspDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentity<User, IdentityRole<int>>(options =>
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
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<ISoberService, SoberService>();
builder.Services.AddScoped<IUserService, UserService>();

// Authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, OneOfManyMemberStatusHandler>();
builder.Services.AddScoped<IAuthorizationHandler, OneOfManyMemberAppointmentHandler>();

// Configure authorization policies
builder.Services.AddAuthorization(options =>
      options.AddPolicy("Member",
      policy => policy.Requirements.Add(new MemberStatusRequirement("New", "Neophyte", "Active", "Alumnus"))));
builder.Services.AddAuthorization(options =>
      options.AddPolicy("NewMember",
      policy => policy.Requirements.Add(new MemberStatusRequirement("New"))));
builder.Services.AddAuthorization(options =>
      options.AddPolicy("ActiveMember",
      policy => policy.Requirements.Add(new MemberStatusRequirement("Active"))));
builder.Services.AddAuthorization(options =>
      options.AddPolicy("AlumnusMember",
      policy => policy.Requirements.Add(new MemberStatusRequirement("New"))));
builder.Services.AddAuthorization(options =>
      options.AddPolicy("Affiliate",
      policy => policy.Requirements.Add(new MemberStatusRequirement("Affiliate"))));


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
else
{
    app.UseDeveloperExceptionPage();
    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapAreaControllerRoute("Admin_default", "Admin", "Admin/{controller}/{action=Index}/{id?}");
app.MapAreaControllerRoute("Alumni_default", "Alumni", "Alumni/{controller}/{action=Index}/{id?}");
app.MapAreaControllerRoute("House_default", "House", "House/{controller}/{action=Index}/{id?}");
app.MapAreaControllerRoute("Kitchen_default", "Kitchen", "Kitchen/{controller}/{action=Index}/{id?}");
app.MapAreaControllerRoute("Laundry_default", "Laundry", "Laundry/{controller}/{action=Index}/{id?}");
app.MapAreaControllerRoute("Members_default", "Members", "Members/{controller}/{action=Index}/{id?}");
app.MapAreaControllerRoute("Scholarships_default", "Scholarships", "Scholarships/{controller}/{action=Index}/{id?}");
app.MapAreaControllerRoute("School_default", "School", "School/{controller}/{action=Index}/{id?}");
app.MapAreaControllerRoute("Service_default", "Service", "Service/{controller}/{action=Index}/{id?}");
app.MapAreaControllerRoute("Sobers_default", "Sobers", "Sobers/{controller}/{action=Index}/{id?}");

//app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.MapControllerRoute(
      name: "HomeActionOnly",
      pattern: "{action}",
      defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
