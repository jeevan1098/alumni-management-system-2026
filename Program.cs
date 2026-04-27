using Alumni_Management_System;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
{
});

builder.Services.AddControllersWithViews(options =>
{
    // Replace the generic "The value '' is invalid" with field-specific required messages
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        _ => "This field is required.");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
        name => $"{name} is required.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (value, name) => string.IsNullOrWhiteSpace(value)
            ? $"{name} is required."
            : $"The value '{value}' is not valid for {name}.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
        value => string.IsNullOrWhiteSpace(value)
            ? "This field is required."
            : $"The value '{value}' is not valid.");
    options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(
        () => "Please enter a valid number.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        name => $"{name} must be a valid number.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => "A value is required.");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        SeedData.InitializeAsync(services).Wait();
    }
    catch (Exception err)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(err, "Error occurred seeding database");
    }
}

if (app.Environment.IsDevelopment())
{
    // ? Shows full exception details in browser instead of crashing the debugger
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Identity/Account/Register"))
    {
        context.Response.Redirect("/Account/VerifyJagId");
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var skipPaths = new[] { "/alumni/edit", "/identity/account/logout", "/account/logout", "/identity/account/manage" };
        if (!skipPaths.Any(p => path.StartsWith(p)))
        {
            var userManager = context.RequestServices.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.GetUserAsync(context.User);
            if (user != null && user.IsFirstLogin && context.User.IsInRole(Constants.AlumniRole))
            {
                var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
                var alumni = await dbContext.Alumni.FirstOrDefaultAsync(a => a.UserId == user.Id || a.JagId == user.JagId);
                if (alumni != null)
                {
                    context.Response.Redirect($"/Alumni/Edit/{alumni.AlumniId}");
                    return;
                }
            }
        }
    }
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();