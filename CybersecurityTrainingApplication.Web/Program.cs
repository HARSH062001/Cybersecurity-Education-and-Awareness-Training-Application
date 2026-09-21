using CybersecurityTrainingApplication.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TrainingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TrainingDatabase")));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TrainingDbContext>();
    await TrainingSeedData.InitialiseAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseStaticFiles();

var learningSectionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "purpose", "outcomes", "terms", "concepts", "workplace", "behaviours", "review"
};

string GetLearnerKey(HttpContext context)
{
    const string cookieName = "training_learner_id";
    if (context.Request.Cookies.TryGetValue(cookieName, out var existing) && Guid.TryParseExact(existing, "N", out _)) return existing;
    var learnerKey = Guid.NewGuid().ToString("N");
    context.Response.Cookies.Append(cookieName, learnerKey, new CookieOptions
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        IsEssential = true,
        Secure = context.Request.IsHttps,
        MaxAge = TimeSpan.FromDays(365)
    });
    return learnerKey;
}

app.MapGet("/api/progress/modules/{moduleId:int}", async (int moduleId, HttpContext context, TrainingDbContext db) =>
{
    var learnerKey = GetLearnerKey(context);
    var completedSections = await db.ModuleSectionProgress
        .Where(x => x.LearnerKey == learnerKey && x.ModuleId == moduleId)
        .OrderBy(x => x.Id)
        .Select(x => x.SectionKey)
        .ToListAsync();
    return Results.Ok(new { moduleId, completedSections });
});

app.MapPost("/api/progress/modules/{moduleId:int}", async (int moduleId, ProgressUpdate update, HttpContext context, TrainingDbContext db) =>
{
    if (moduleId is < 1 or > 8 || !learningSectionKeys.Contains(update.SectionKey)) return Results.BadRequest(new { message = "Unknown learning section." });
    var learnerKey = GetLearnerKey(context);
    var existing = await db.ModuleSectionProgress.FirstOrDefaultAsync(x => x.LearnerKey == learnerKey && x.ModuleId == moduleId && x.SectionKey == update.SectionKey);
    if (update.Completed && existing is null)
    {
        db.ModuleSectionProgress.Add(new() { LearnerKey = learnerKey, ModuleId = moduleId, SectionKey = update.SectionKey.ToLowerInvariant(), CompletedAtUtc = DateTimeOffset.UtcNow });
    }
    else if (!update.Completed && existing is not null)
    {
        db.ModuleSectionProgress.Remove(existing);
    }
    await db.SaveChangesAsync();
    var completedSections = await db.ModuleSectionProgress.Where(x => x.LearnerKey == learnerKey && x.ModuleId == moduleId).Select(x => x.SectionKey).ToListAsync();
    return Results.Ok(new { moduleId, completedSections });
});

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

public sealed record ProgressUpdate(string SectionKey, bool Completed);
