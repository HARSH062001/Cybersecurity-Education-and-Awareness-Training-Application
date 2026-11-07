using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Use the configured SQLite database for accounts, progress and assessments.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "The DefaultConnection database setting is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseSqlite(connectionString)
        .UseSeeding((context, _) => QuestionBankSeeder.Seed(
            context,
            Path.Combine(
                builder.Environment.ContentRootPath,
                "Data",
                "question_banks.json"))));

// Identity manages sign-in, password rules, lockouts and role membership.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Local development allows accounts without an email service.
    // Other environments require a confirmed account.
    options.SignIn.RequireConfirmedAccount =
        !builder.Environment.IsDevelopment();

    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

// Browser requests that change saved progress or assessments send
// their antiforgery token in this header.
builder.Services.AddAntiforgery(options =>
    options.HeaderName = "X-CSRF-TOKEN");

var app = builder.Build();

// Migrations are applied separately with "dotnet ef database update".
// Skip role setup until the database exists. This also lets the test
// factory create its isolated database without modifying another one.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    if (await db.Database.CanConnectAsync())
    {
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        // Create role records once; restarting the app preserves them.
        foreach (var roleName in new[] { "Admin", "ModuleOwner" })
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(
                new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create the {roleName} role: " +
                    string.Join(
                        ", ",
                        result.Errors.Select(error =>
                            error.Description)));
            }
        }

        // Local bootstrap: promote one previously registered account.
        // Read the email from user secrets so it is not committed to Git.
        if (app.Environment.IsDevelopment())
        {
            var adminEmail =
                app.Configuration["AdminBootstrap:Email"];

            if (!string.IsNullOrWhiteSpace(adminEmail))
            {
                var userManager = scope.ServiceProvider
                    .GetRequiredService<
                        UserManager<ApplicationUser>>();

                var admin = await userManager
                    .FindByEmailAsync(adminEmail)
                    ?? throw new InvalidOperationException(
                        "AdminBootstrap:Email does not match " +
                        "a registered account.");

                if (!await userManager.IsInRoleAsync(
                        admin, "Admin"))
                {
                    var result = await userManager
                        .AddToRoleAsync(admin, "Admin");

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            "Could not assign the Admin role: " +
                            string.Join(
                                ", ",
                                result.Errors.Select(error =>
                                    error.Description)));
                    }
                }
            }
        }
    }
}

// Use the normal error page and HTTPS protection outside Development.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Give browser page requests a recovery page while preserving the HTTP status.
// API clients continue receiving their original responses, without HTML wrappers.
app.UseWhen(context =>
    (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)) &&
    !context.Request.Path.StartsWithSegments("/api") &&
    (context.Request.Headers.Accept.Count == 0 ||
     context.Request.GetTypedHeaders().Accept?.Any(value =>
         string.Equals(value.MediaType.Value, "text/html", StringComparison.OrdinalIgnoreCase)) == true),
    pages => pages.UseStatusCodePagesWithReExecute("/Status/{0}"));

app.UseRouting();

// Authentication identifies the user; authorization checks access.
app.UseAuthentication();
app.UseAuthorization();

// Map the website and the endpoints that save learner data.
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapProgressEndpoints();
app.MapAssessmentSubmissionEndpoints();

app.Run();

// Expose the entry point to the integration test project.
public partial class Program { }
