using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Pages;

// Restrict learner progress and assessment records to administrators.
[Authorize(Roles = "Admin")]
public class AdminProgressModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public AdminProgressModel(ApplicationDbContext db)
    {
        _db = db;
    }

    // Data displayed by the admin progress page.
    public List<LearnerProgress> Learners { get; private set; } = [];

    public async Task OnGetAsync()
    {
        // Load account details without tracking because this page is read-only.
        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .Select(user => new
            {
                user.Id,
                user.Email,
                user.FullName
            })
            .ToListAsync();

        // Count each learner's saved learning-section completions.
        var progress = await _db.SectionProgressEntries
            .AsNoTracking()
            .GroupBy(entry => entry.UserId)
            .Select(group => new
            {
                UserId = group.Key,
                CompletedSections = group.Count()
            })
            .ToDictionaryAsync(item => item.UserId);

        // Load completed assessment scores for the administrator to review.
        var attempts = await _db.AssessmentAttempts
            .AsNoTracking()
            .Include(attempt => attempt.Bank)
            .Where(attempt =>
                attempt.Status == AssessmentAttemptStatus.Completed)
            .OrderByDescending(attempt => attempt.StartedAtUtc)
            .Select(attempt => new AttemptProgress(
                attempt.UserId,
                attempt.Bank.ModuleId,
                attempt.Score,
                attempt.TotalQuestions,
                attempt.StartedAtUtc))
            .ToListAsync();

        // Combine account, progress, and assessment data for display.
        Learners = users.Select(user => new LearnerProgress
        {
            Email = user.Email ?? "(no email)",
            Name = string.IsNullOrWhiteSpace(user.FullName)
                ? user.Email ?? "Learner"
                : user.FullName,
            CompletedSections = progress.TryGetValue(
                user.Id, out var savedProgress)
                    ? savedProgress.CompletedSections
                    : 0,
            CompletedAttempts = attempts
                .Where(attempt => attempt.UserId == user.Id)
                .Select(attempt => new AttemptSummary(
                    attempt.ModuleId,
                    attempt.Score,
                    attempt.TotalQuestions,
                    attempt.StartedAtUtc))
                .ToList()
        }).ToList();

        // Learner records should not be stored by browser caches.
        Response.Headers.CacheControl = "no-store";
    }

    // Summary of one learner's account and training activity.
    public sealed class LearnerProgress
    {
        public string Email { get; init; } = "";
        public string Name { get; init; } = "";
        public int CompletedSections { get; init; }
        public List<AttemptSummary> CompletedAttempts { get; init; } = [];
    }

    // Assessment result details shown for each learner.
    public sealed record AttemptSummary(
        int ModuleId,
        int? Score,
        int? TotalQuestions,
        DateTime StartedAtUtc);

    // Internal projection used to load attempt data from the database.
    private sealed record AttemptProgress(
        string UserId,
        int ModuleId,
        int? Score,
        int? TotalQuestions,
        DateTime StartedAtUtc);
}