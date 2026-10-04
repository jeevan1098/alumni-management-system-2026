using Alumni_Management_System;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Alumni_Management_System.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation for now
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    // Forgot Password and other lookups expect one account per email.
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Configure application cookie for login redirects
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Redirect default Identity registration to custom registration
builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
{
    // This will be handled by middleware
});

builder.Services.AddControllersWithViews(options =>
{
    // A blank number/date box would otherwise say "The value '' is invalid.";
    // fields with their own message replace this (ModelStateMessages).
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Please fill in this field.");
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


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Built-in Identity pages this app replaces or doesn't allow. Deleting your own
// account or changing your email here would bypass the alumni records linked
// by JAG ID, and two-factor is the app's own email-code setup
// (TwoFactorSettingsController), not the built-in authenticator pages.
var blockedIdentityPages = new (string Path, string RedirectTo)[]
{
    ("/Identity/Account/Register", "/Account/VerifyJagId"),
    ("/Identity/Account/ExternalLogin", "/Identity/Account/Login"),
    ("/Identity/Account/ConfirmEmailChange", "/Identity/Account/Manage"),
    ("/Identity/Account/Manage/Email", "/Identity/Account/Manage"),
    ("/Identity/Account/Manage/PersonalData", "/Identity/Account/Manage"),
    ("/Identity/Account/Manage/DownloadPersonalData", "/Identity/Account/Manage"),
    ("/Identity/Account/Manage/DeletePersonalData", "/Identity/Account/Manage"),
    ("/Identity/Account/Manage/ExternalLogins", "/Identity/Account/Manage"),
    ("/Identity/Account/Manage/TwoFactorAuthentication", "/TwoFactorSettings"),
    ("/Identity/Account/Manage/EnableAuthenticator", "/TwoFactorSettings"),
    ("/Identity/Account/Manage/ResetAuthenticator", "/TwoFactorSettings"),
    ("/Identity/Account/Manage/Disable2fa", "/TwoFactorSettings"),
    ("/Identity/Account/Manage/GenerateRecoveryCodes", "/TwoFactorSettings"),
    ("/Identity/Account/Manage/ShowRecoveryCodes", "/TwoFactorSettings"),
};
app.Use(async (context, next) =>
{
    foreach (var (path, redirectTo) in blockedIdentityPages)
    {
        if (context.Request.Path.StartsWithSegments(path))
        {
            context.Response.Redirect(redirectTo);
            return;
        }
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Middleware to redirect first-time Alumni users to complete their profile
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        // Skip redirect for certain paths
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var skipPaths = new[] { "/alumni/edit", "/identity/account/logout", "/account/logout", "/identity/account/manage", "/account/completesetup", "/account/changetemppassword" };

        if (!skipPaths.Any(p => path.StartsWith(p)))
        {
            var userManager = context.RequestServices.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.GetUserAsync(context.User);

            // An admin reset this account's password (forgot-password
            // request) - highest priority, since nothing else matters until
            // they're off the temporary password.
            if (user != null && user.MustChangePassword)
            {
                context.Response.Redirect("/Account/ChangeTempPassword");
                return;
            }

            if (user != null && user.IsFirstLogin && context.User.IsInRole(Constants.AlumniRole))
            {
                var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
                var alumni = await dbContext.Alumni.FirstOrDefaultAsync(a => a.JagId == user.JagId);

                if (alumni != null)
                {
                    context.Response.Redirect($"/Alumni/Edit/{alumni.AlumniId}");
                    return;
                }
            }

            // Admin/Staff accounts created with a placeholder username +
            // temp password must pick their own username/password before
            // going any further.
            if (user != null && user.IsFirstLogin &&
                (context.User.IsInRole(Constants.AdminRole) || context.User.IsInRole(Constants.StaffRole)))
            {
                context.Response.Redirect("/Account/CompleteSetup");
                return;
            }

            // 2FA is opt-in for every role (Admin/Staff included) - see
            // TwoFactorSettingsController. No forced-enrollment redirect here;
            // a future per-user "force 2FA" flag would plug in at this point.
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
