using System.Text.Json;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "The DefaultConnection database setting is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
    .UseSqlite(connectionString)
    .UseSeeding((context, _) => QuestionBankSeeder.Seed(context))
);

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Allow local testing; require account confirmation outside Development.
    options.SignIn.RequireConfirmedAccount = !builder.Environment.IsDevelopment();
    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

// JavaScript will send this header when saving section progress.
builder.Services.AddAntiforgery(options =>
    options.HeaderName = "X-CSRF-TOKEN");

var app = builder.Build();

//Seed initial question banks, answers, and options
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    QuestionBankSeeder.Seed(db);
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapProgressEndpoints();

app.Run();