// Loads and handles server-side data for authorised enrolment and assessment scheduling.
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Pages;

[Authorize(Roles = "Admin,ModuleOwner")]
public class TrainingManagementModel(ApplicationDbContext db) : PageModel
{
    public List<int> Modules { get; private set; } = [];
    public List<ModuleSchedule> Schedules { get; private set; } = [];
    public List<ModuleEnrollment> Enrollments { get; private set; } = [];
    [BindProperty, Range(1, 8)] public int ModuleId { get; set; }
    [BindProperty, Required, EmailAddress] public string LearnerEmail { get; set; } = "";
    [BindProperty] public DateTime? DueAtUtc { get; set; }
    [BindProperty, Range(0, 3650)] public int RepeatEveryDays { get; set; }
    [TempData] public string? Notice { get; set; }

    // Always check ownership on the server, including forged form submissions.
    private Task<bool> CanManage(int id) => User.IsInRole("Admin")
        ? Task.FromResult(id is >= 1 and <= 8)
        : db.ModuleOwnerAssignments.AnyAsync(a => a.ModuleId == id &&
            a.OwnerUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

    public async Task OnGetAsync()
    {
        Response.Headers.CacheControl = "no-store";
        Modules = User.IsInRole("Admin") ? Enumerable.Range(1, 8).ToList()
            : await db.ModuleOwnerAssignments.Where(a => a.OwnerUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(a => a.ModuleId).Distinct().OrderBy(id => id).ToListAsync();
        Schedules = await db.ModuleSchedules.AsNoTracking().Where(s => Modules.Contains(s.ModuleId)).ToListAsync();
        Enrollments = await db.ModuleEnrollments.AsNoTracking().Include(e => e.User)
            .Where(e => Modules.Contains(e.ModuleId)).OrderBy(e => e.ModuleId).ToListAsync();
    }

    public async Task<IActionResult> OnPostEnrollAsync()
    {
        if (!await CanManage(ModuleId)) return Forbid();
        ModelState.Remove(nameof(DueAtUtc));
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        var email = LearnerEmail.Trim().ToUpperInvariant();
        var learner = await db.Users.SingleOrDefaultAsync(u => u.NormalizedEmail == email);
        if (learner is null) { ModelState.AddModelError(nameof(LearnerEmail), "No registered account has that email."); await OnGetAsync(); return Page(); }
        if (!await db.ModuleEnrollments.AnyAsync(e => e.UserId == learner.Id && e.ModuleId == ModuleId))
        {
            db.ModuleEnrollments.Add(new ModuleEnrollment { UserId = learner.Id, ModuleId = ModuleId });
            await db.SaveChangesAsync();
        }
        Notice = "Learner enrolled. The module now appears in their training notifications.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostScheduleAsync()
    {
        if (!await CanManage(ModuleId)) return Forbid();
        ModelState.Remove(nameof(LearnerEmail));
        if (DueAtUtc is null) ModelState.AddModelError(nameof(DueAtUtc), "Enter the first due date in UTC.");
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        var schedule = await db.ModuleSchedules.SingleOrDefaultAsync(s => s.ModuleId == ModuleId);
        if (schedule is null) { schedule = new ModuleSchedule { ModuleId = ModuleId }; db.ModuleSchedules.Add(schedule); }
        schedule.FirstDueAtUtc = DateTime.SpecifyKind(DueAtUtc!.Value, DateTimeKind.Utc);
        schedule.RepeatEveryDays = RepeatEveryDays;
        await db.SaveChangesAsync();
        Notice = "Schedule saved. Enrolled learners can see the updated deadline in notifications.";
        return RedirectToPage();
    }
}
