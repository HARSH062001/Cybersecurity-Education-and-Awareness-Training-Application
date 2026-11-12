// Loads and handles server-side data for the learner’s own assessment attempts.
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CybersecurityTrainingWeb.Utils;

namespace CybersecurityTrainingWeb.Pages;

[Authorize]
public class AssessmentHistoryModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AssessmentHistoryModel(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _configuration = configuration;
    }

    public List<AttemptSummary> Attempts { get; private set; } = [];
    public HashSet<int> PassedModuleIds { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var attempts = await _dbContext.AssessmentAttempts
            .AsNoTracking()
            .Include(attempt => attempt.Bank)
            .Include(attempt => attempt.Answers)
            .Where(attempt => attempt.UserId == userId)
            .OrderByDescending(attempt => attempt.StartedAtUtc)
            .ToListAsync();

        string[] moduleTitles =
        [
            "Cybersecurity Fundamentals",
            "Phishing and Social Engineering",
            "Password and Identity Security",
            "Data Security and Privacy",
            "Network and Internet Security",
            "Device and Physical Security",
            "AI Literacy and AI Security",
            "Incident Reporting and Cyber Hygiene"
        ];

        Attempts = attempts.Select(attempt => new AttemptSummary
        {
            Id = attempt.Id,
            ModuleId = attempt.Bank.ModuleId,
            ModuleTitle = attempt.Bank.ModuleId >= 1 &&
                          attempt.Bank.ModuleId <= moduleTitles.Length
                ? moduleTitles[attempt.Bank.ModuleId - 1]
                : $"Module {attempt.Bank.ModuleId}",
            StartedAtUtc = attempt.StartedAtUtc,
            Status = attempt.Status,
            Score = attempt.Score,
            TotalQuestions = attempt.TotalQuestions,
            SavedAnswerCount = attempt.Answers.Count(answer =>
                !string.IsNullOrWhiteSpace(answer.SelectedOptionCode))
        }).ToList();

        var passingPercentage = ResultUtils.AssessmentRules.GetPassingPercentage(_configuration);
        PassedModuleIds = attempts
            .Where(attempt => attempt.Status == AssessmentAttemptStatus.Completed &&
                              ResultUtils.AssessmentRules.IsPassed(
                                  attempt.Score, attempt.TotalQuestions, passingPercentage))
            .Select(attempt => attempt.Bank.ModuleId)
            .ToHashSet();

        Response.Headers.CacheControl = "no-store";
        return Page();
    }

    public sealed class AttemptSummary
    {
        public long Id { get; init; }
        public int ModuleId { get; init; }
        public string ModuleTitle { get; init; } = "";
        public DateTime StartedAtUtc { get; init; }
        public AssessmentAttemptStatus Status { get; init; }
        public int? Score { get; init; }
        public int? TotalQuestions { get; init; }
        public int SavedAnswerCount { get; init; }
    }
}
