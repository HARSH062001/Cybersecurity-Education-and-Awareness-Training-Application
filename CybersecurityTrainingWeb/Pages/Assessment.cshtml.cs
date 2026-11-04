using System.Security.Cryptography;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Pages;

[Authorize]
public class AssessmentModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssessmentModel(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public long AttemptId { get; private set; }
    public int ModuleId { get; private set; }
    public string Bank { get; private set; } = "";
    public string ModuleTitle { get; private set; } = "Cybersecurity Fundamentals";
    public List<QuestionDto> Questions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int moduleId = 1, long? attemptId = null)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

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
            if (moduleId is < 1 or > 8) return RedirectToPage("/Modules");

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

                var selected = choices[RandomNumberGenerator.GetInt32(choices.Count)];
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
