using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Data;

public static class ProgressEndpoints
{
    private static readonly HashSet<string> ValidSections =
    [
        "purpose",
        "outcomes",
        "terms",
        "concepts",
        "workplace",
        "behaviours",
        "review"
    ];

    public static IEndpointRouteBuilder MapProgressEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var progress = endpoints
            .MapGroup("/api/progress")
            .RequireAuthorization();

        progress.MapGet("/modules/{id:int}", GetModuleProgress);
        progress.MapPost("/modules/{id:int}", SaveModuleProgress);

        return endpoints;
    }

    private static async Task<IResult> GetModuleProgress(
        int id,
        HttpContext http,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        if (id is < 1 or > 8)
        {
            return Results.BadRequest(new { error = "Invalid module." });
        }

        var userId = userManager.GetUserId(http.User);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        http.Response.Headers["Cache-Control"] = "no-store";

        var sections = await ReadCompletedSections(db, userId, id);
        return Results.Ok(new { completedSections = sections });
    }

    private static async Task<IResult> SaveModuleProgress(
        int id,
        ProgressUpdateRequest request,
        HttpContext http,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IAntiforgery antiforgery)
    {
        if (id is < 1 or > 8 ||
            request.SectionKey is null ||
            request.Completed is null ||
            !ValidSections.Contains(request.SectionKey))
        {
            return Results.BadRequest(new { error = "Invalid progress update." });
        }

        // Cookie-authenticated writes need an antiforgery token.
        try
        {
            await antiforgery.ValidateRequestAsync(http);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest(new { error = "Invalid request token." });
        }

        var userId = userManager.GetUserId(http.User);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var existing = await db.SectionProgressEntries.SingleOrDefaultAsync(
            progress => progress.UserId == userId &&
                        progress.ModuleId == id &&
                        progress.SectionKey == request.SectionKey);

        if (request.Completed.Value && existing is null)
        {
            db.SectionProgressEntries.Add(new SectionProgress
            {
                UserId = userId,
                ModuleId = id,
                SectionKey = request.SectionKey,
                CompletedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
        else if (!request.Completed.Value && existing is not null)
        {
            db.SectionProgressEntries.Remove(existing);
            await db.SaveChangesAsync();
        }

        http.Response.Headers["Cache-Control"] = "no-store";

        var sections = await ReadCompletedSections(db, userId, id);
        return Results.Ok(new { completedSections = sections });
    }

    private static Task<string[]> ReadCompletedSections(
        ApplicationDbContext db,
        string userId,
        int moduleId)
    {
        return db.SectionProgressEntries
            .AsNoTracking()
            .Where(progress => progress.UserId == userId &&
                               progress.ModuleId == moduleId)
            .OrderBy(progress => progress.SectionKey)
            .Select(progress => progress.SectionKey)
            .ToArrayAsync();
    }
}

public sealed class ProgressUpdateRequest
{
    public string? SectionKey { get; set; }
    public bool? Completed { get; set; }
}