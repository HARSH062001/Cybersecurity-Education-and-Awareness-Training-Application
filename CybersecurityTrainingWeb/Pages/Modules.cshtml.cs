using System.Text.Json;
using System.Security.Claims;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Pages;

public class ModulesModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ModulesModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<ModuleCard> Modules { get; private set; } = [];

    public HashSet<int> EnrolledModuleIds { get; private set; } = [];

    public async Task OnGetAsync()
    {
        LoadModules();

        // Only show enrolments belonging to the signed-in learner.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return;

        EnrolledModuleIds = await _db.ModuleEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.UserId == userId)
            .Select(enrollment => enrollment.ModuleId)
            .ToHashSetAsync();
    }

    // Saves an individual learner's enrolment in one module.
    public async Task<IActionResult> OnPostEnrollAsync(int moduleId)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Redirect(
                $"/Identity/Account/Login?ReturnUrl=%2FModules");
        }

        var availableModules = ReadModuleRecords();
        if (!availableModules.Any(module => module.Id == moduleId))
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Forbid();

        var alreadyEnrolled = await _db.ModuleEnrollments
            .AnyAsync(enrollment =>
                enrollment.UserId == userId &&
                enrollment.ModuleId == moduleId);

        if (!alreadyEnrolled)
        {
            _db.ModuleEnrollments.Add(new ModuleEnrollment
            {
                UserId = userId,
                ModuleId = moduleId,
                EnrolledAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["EnrollmentMessage"] =
                $"You are enrolled in Module {moduleId}.";
        }
        else
        {
            TempData["EnrollmentMessage"] =
                $"You are already enrolled in Module {moduleId}.";
        }

        return RedirectToPage();
    }

    private void LoadModules()
    {
        Modules = ReadModuleRecords()
            .OrderBy(record => record.Id)
            .Select(record => new ModuleCard(
                record.Id,
                record.Title.Replace($"Module {record.Id} – ", ""),
                record.Blocks.FirstOrDefault(
                    block => block.Kind == "paragraph")?.Text
                    ?? "Build practical cybersecurity knowledge and safer workplace habits."))
            .ToList();
    }

    private List<ModuleRecord> ReadModuleRecords()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "module-content.json");

        if (!System.IO.File.Exists(path))
            return [];

        return JsonSerializer.Deserialize<List<ModuleRecord>>(
            System.IO.File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];
    }

    public sealed class ModuleRecord
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<ModuleBlock> Blocks { get; set; } = [];
    }

    public sealed class ModuleBlock
    {
        public string Kind { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public record ModuleCard(int Number, string Title, string Summary);
}