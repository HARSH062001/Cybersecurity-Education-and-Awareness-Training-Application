using System.Security.Cryptography;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CybersecurityTrainingWeb.Utils;

namespace CybersecurityTrainingWeb.Pages;

[Authorize]
public class AssessmentModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AssessmentModel(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _configuration = configuration;
    }

    public long AttemptId { get; private set; }
    public int ModuleId { get; private set; }
    public string Bank { get; private set; } = "";
    public string ModuleTitle { get; private set; } = "Cybersecurity Fundamentals";
    public int PassingPercentage => ResultUtils.AssessmentRules.GetPassingPercentage(_configuration);
    public List<QuestionDto> Questions { get; private set; } = [];

    private static readonly string[] RequiredSections =
        ["purpose", "outcomes", "terms", "concepts", "workplace", "behaviours", "review"];

    public async Task<IActionResult> OnGetAsync(int moduleId = 1, long? attemptId = null)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        if (attemptId is not null)
        {
            moduleId = await _dbContext.AssessmentAttempts.AsNoTracking()
                .Where(a => a.Id == attemptId && a.UserId == userId &&
                            a.Status == AssessmentAttemptStatus.InProgress)
                .Select(a => a.Bank.ModuleId)
                .SingleOrDefaultAsync();
            if (moduleId == 0) return NotFound();
        }

        if (moduleId is < 1 or > 8) return RedirectToPage("/Modules");

        var completedSections = await _dbContext.SectionProgressEntries.AsNoTracking()
            .Where(p => p.UserId == userId && p.ModuleId == moduleId &&
                        RequiredSections.Contains(p.SectionKey))
            .Select(p => p.SectionKey)
            .Distinct()
            .CountAsync();
        if (completedSections != RequiredSections.Length)
        {
            TempData["AssessmentNotice"] = "Complete all seven learning sections before starting the assessment.";
            return RedirectToPage("/Module", new { id = moduleId });
        }

        var passingPercentage = PassingPercentage;
        var alreadyPassed = await ResultUtils.HasUserPassed(
            _dbContext, moduleId, userId, passingPercentage);
        if (alreadyPassed)
        {
            TempData["AssessmentNotice"] = "You have already passed this module assessment.";
            return RedirectToPage("/Module", new { id = moduleId });
        }

        AssessmentAttempt? attempt;
        if (attemptId is not null)
        {
            attempt = await _dbContext.AssessmentAttempts.AsNoTracking()
                .SingleOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId &&
                                           a.Status == AssessmentAttemptStatus.InProgress);
            if (attempt is null) return NotFound();
        }
        else
        {
            attempt = await _dbContext.AssessmentAttempts.AsNoTracking()
                .Where(a => a.UserId == userId && a.Status == AssessmentAttemptStatus.InProgress &&
                            a.Bank.ModuleId == moduleId)
                .OrderByDescending(a => a.StartedAtUtc)
                .FirstOrDefaultAsync();

            if (attempt is null)
            {
                var published = await _dbContext.AssessmentBanks.AsNoTracking()
                    .Where(b => b.ModuleId == moduleId && b.IsPublished)
                    .Select(b => new { b.Id, b.Code, b.Version })
                    .ToListAsync();

                var choices = published.GroupBy(b => b.Code)
                    .Select(g => g.OrderByDescending(b => b.Version).First())
                    .ToList();
                if (choices.Count == 0) return RedirectToPage("/Modules");

                var bankHistory = await _dbContext.AssessmentAttempts.AsNoTracking()
                    .Where(a => a.UserId == userId && a.Bank.ModuleId == moduleId)
                    .OrderBy(a => a.StartedAtUtc)
                    .Select(a => a.Bank.Code)
                    .ToListAsync();
                var preferredCode = AttemptUtils.PickBank(bankHistory);
                var selected = choices.FirstOrDefault(b => b.Code == preferredCode)
                    ?? choices[RandomNumberGenerator.GetInt32(choices.Count)];
                attempt = new AssessmentAttempt
                {
                    UserId = userId,
                    AssessmentBankId = selected.Id,
                    Status = AssessmentAttemptStatus.InProgress
                };
                _dbContext.AssessmentAttempts.Add(attempt);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToPage("/Assessment", new { attemptId = attempt.Id });
        }

        var realBank = await _dbContext.AssessmentBanks.AsNoTracking()
            .Include(b => b.Questions)
            .ThenInclude(q => q.Options)
            .SingleOrDefaultAsync(b => b.Id == attempt.AssessmentBankId);
        if (realBank is null) return NotFound();

        Response.Headers.CacheControl = "no-store";
        AttemptId = attempt.Id;
        ModuleId = realBank.ModuleId;
        Bank = realBank.Code;
        ModuleTitle = ModuleTitles[ModuleId - 1];
        Questions = realBank.Questions.OrderBy(q => q.Number)
            .Select(q => new QuestionDto
            {
                Id = $"m{ModuleId}{Bank.ToLowerInvariant()}q{q.Number}",
                Number = q.Number,
                Text = q.Text,
                Type = q.Type,
                Options = q.Options.OrderBy(o => o.Code)
                    .Select(o => new OptionDto { Code = o.Code, Text = o.Text })
                    .ToList()
            }).ToList();
        return Page();
    }

    private static readonly string[] ModuleTitles =
    [
        "Cybersecurity Fundamentals", "Phishing and Social Engineering", "Password and Identity Security", "Data Security and Privacy",
        "Network and Internet Security", "Device and Physical Security", "AI Literacy and AI Security", "Incident Reporting and Cyber Hygiene"
    ];

    public sealed class QuestionDto
    {
        public string Id { get; set; } = "";
        public int Number { get; set; }
        public string Text { get; set; } = "";
        public string Type { get; set; } = "multipleChoice";
        public List<OptionDto> Options { get; set; } = [];
    }

    public sealed class OptionDto
    {
        public string Code { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
