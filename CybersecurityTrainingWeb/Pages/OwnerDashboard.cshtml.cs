using System.Security.Claims;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Pages;

// Only users with the ModuleOwner role can view this dashboard.
[Authorize(Roles = "ModuleOwner")]
public class OwnerDashboardModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public OwnerDashboardModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<ModuleReport> Modules { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var ownerUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(ownerUserId))
        {
            Modules = [];
            return;
        }

        // Restrict all report queries to modules assigned to this owner.
        var moduleIds = await _db.ModuleOwnerAssignments
            .AsNoTracking()
            .Where(assignment => assignment.OwnerUserId == ownerUserId)
            .Select(assignment => assignment.ModuleId)
            .Distinct()
            .ToListAsync();

        var progress = await _db.SectionProgressEntries
            .AsNoTracking()
            .Where(entry => moduleIds.Contains(entry.ModuleId))
            .Select(entry => new ProgressRow(
                entry.ModuleId,
                entry.UserId))
            .ToListAsync();

        var attempts = await _db.AssessmentAttempts
            .AsNoTracking()
            .Where(attempt =>
                moduleIds.Contains(attempt.Bank.ModuleId) &&
                attempt.Status == AssessmentAttemptStatus.Completed)
            .OrderByDescending(attempt => attempt.SubmittedAtUtc)
            .Select(attempt => new AttemptRow(
                attempt.Bank.ModuleId,
                attempt.UserId,
                attempt.User.FullName,
                attempt.User.Email,
                attempt.Score,
                attempt.TotalQuestions,
                attempt.SubmittedAtUtc ?? attempt.StartedAtUtc))
            .ToListAsync();

        Modules = moduleIds
            .OrderBy(id => id)
            .Select(moduleId =>
            {
                var moduleProgress = progress
                    .Where(row => row.ModuleId == moduleId)
                    .ToList();

                var moduleAttempts = attempts
                    .Where(row => row.ModuleId == moduleId)
                    .ToList();

                // Use the same account ID for both sources to avoid duplicates.
                var learnersWithActivity = moduleProgress
                    .Select(row => row.UserId)
                    .Concat(moduleAttempts.Select(row => row.UserId))
                    .Distinct(StringComparer.Ordinal)
                    .Count();

                return new ModuleReport(
                    moduleId,
                    GetModuleTitle(moduleId),
                    learnersWithActivity,
                    moduleProgress.Count,
                    moduleAttempts.Count,
                    moduleAttempts.Take(5).ToList());
            })
            .ToList();

        // Prevent browsers from caching learner progress and assessment results.
        Response.Headers.CacheControl = "no-store";
    }

    // Match these module IDs and titles to Data/module-content.json.
    private static string GetModuleTitle(int moduleId) => moduleId switch
    {
        1 => "Cybersecurity Fundamentals",
        2 => "Phishing and Social Engineering",
        3 => "Password and Identity Security",
        4 => "Data Security and Privacy",
        5 => "Network and Internet Security",
        6 => "Device and Physical Security",
        7 => "AI Literacy and AI Security",
        8 => "Incident Reporting and Cyber Hygiene",
        _ => $"Module {moduleId}"
    };

    public sealed record ModuleReport(
        int ModuleId,
        string Title,
        int LearnersWithActivity,
        int CompletedSections,
        int CompletedAssessments,
        List<AttemptRow> RecentAttempts);

    public sealed record AttemptRow(
        int ModuleId,
        string UserId,
        string? LearnerName,
        string? LearnerEmail,
        int? Score,
        int? TotalQuestions,
        DateTime ActivityAtUtc);

    private sealed record ProgressRow(
        int ModuleId,
        string UserId);
}