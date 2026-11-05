using CybersecurityTrainingWeb.Data;
using CybersecurityTrainingWeb.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Pages;

[Authorize]
public class ResultsModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public ResultsModel(
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
    public string ModuleTitle { get; private set; } = "";
    public int Score { get; private set; }
    public int TotalQuestions { get; private set; }
    public int Answered { get; private set; }
    public bool IsPassed { get; private set; }
    public int PassingPercentage => ResultUtils.AssessmentRules.GetPassingPercentage(_configuration);

    public List<QuestionResult> Questions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(long attemptId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        // Only a completed attempt belonging to this learner can reveal answers.
        var attempt = await _dbContext.AssessmentAttempts
            .AsNoTracking()
            .Include(a => a.Bank)
                .ThenInclude(b => b.Questions)
                    .ThenInclude(q => q.Options)
            .Include(a => a.Answers)
            .SingleOrDefaultAsync(a =>
                a.Id == attemptId &&
                a.UserId == userId &&
                a.Status == AssessmentAttemptStatus.Completed);

        if (attempt is null ||
            attempt.Score is null ||
            attempt.TotalQuestions is null)
        {
            return NotFound();
        }

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

        if (attempt.Bank.ModuleId < 1 ||
            attempt.Bank.ModuleId > moduleTitles.Length)
        {
            return NotFound();
        }

        var answersByQuestionId = attempt.Answers.ToDictionary(
            answer => answer.AssessmentQuestionId);

        Questions = attempt.Bank.Questions
            .OrderBy(question => question.Number)
            .Select(question =>
            {
                answersByQuestionId.TryGetValue(question.Id, out var answer);

                var selectedOption = question.Options.FirstOrDefault(
                    option => option.Code == answer?.SelectedOptionCode);

                return new QuestionResult
                {
                    Number = question.Number,
                    QuestionText = question.Text,
                    SelectedOptionCode = selectedOption?.Code,
                    SelectedOptionText = selectedOption?.Text,
                    IsCorrect = answer?.IsCorrect == true,
                    Explanation = answer?.IsCorrect == true ? question.Explanation : null
                };
            })
            .ToList();

        Response.Headers.CacheControl = "no-store";

        AttemptId = attempt.Id;
        ModuleId = attempt.Bank.ModuleId;
        ModuleTitle = moduleTitles[ModuleId - 1];
        Score = attempt.Score.Value;
        TotalQuestions = attempt.TotalQuestions.Value;
        IsPassed = ResultUtils.AssessmentRules.IsPassed(
            Score, TotalQuestions, PassingPercentage);
        Answered = attempt.Answers.Count(answer =>
            !string.IsNullOrWhiteSpace(answer.SelectedOptionCode));

        return Page();
    }

    public sealed class QuestionResult
    {
        public int Number { get; init; }
        public string QuestionText { get; init; } = "";
        public string? SelectedOptionCode { get; init; }
        public string? SelectedOptionText { get; init; }
        public bool IsCorrect { get; init; }
        public string? Explanation { get; init; }
    }
}
