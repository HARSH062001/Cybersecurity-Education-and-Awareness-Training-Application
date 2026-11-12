// Loads and handles server-side data for the signed-in learner’s enrolments and assessment reminders.
using System.Security.Claims;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CybersecurityTrainingWeb.Utils;
namespace CybersecurityTrainingWeb.Pages;

[Authorize]
public class NotificationsModel(ApplicationDbContext db, IConfiguration configuration) : PageModel
{
    public List<TrainingNotice> Notices { get; private set; } = [];
    public async Task OnGetAsync()
    {
        Response.Headers.CacheControl = "no-store";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var modules = await db.ModuleEnrollments.AsNoTracking().Where(e => e.UserId == userId).Select(e => e.ModuleId).ToListAsync();
        var schedules = await db.ModuleSchedules.AsNoTracking().Where(s => modules.Contains(s.ModuleId)).ToListAsync();
        var completed = await db.AssessmentAttempts.AsNoTracking()
            .Where(a => a.UserId == userId && a.Status == AssessmentAttemptStatus.Completed)
            .Select(a => new { a.Bank.ModuleId, a.SubmittedAtUtc, a.Score, a.TotalQuestions }).ToListAsync();
        var passingPercentage = ResultUtils.AssessmentRules.GetPassingPercentage(configuration);
        foreach (var id in modules.Order())
        {
            var schedule = schedules.SingleOrDefault(s => s.ModuleId == id);
            var moduleAttempts = completed.Where(a => a.ModuleId == id).ToList();
            var passed = moduleAttempts.Any(a => ResultUtils.AssessmentRules.IsPassed(
                a.Score, a.TotalQuestions, passingPercentage));
            var last = moduleAttempts.Select(a => a.SubmittedAtUtc).Max();
            DateTime? due = schedule is null ? null : last is null ? schedule.FirstDueAtUtc
                : schedule.RepeatEveryDays == 0 ? null : last.Value.AddDays(schedule.RepeatEveryDays);
            var message = passed ? "Assessment passed. Further attempts are locked."
                : schedule is null ? "You are enrolled. No assessment deadline has been set."
                : last is not null && schedule.RepeatEveryDays == 0 ? "Scheduled assessment completed."
                : due < DateTime.UtcNow ? "Assessment overdue. Complete your assessment."
                : "Assessment scheduled. Complete it by the due date.";
            Notices.Add(new TrainingNotice(id, message, passed ? null : due, passed));
        }
    }
    public sealed record TrainingNotice(int ModuleId, string Message, DateTime? DueAtUtc, bool IsPassed);
}
