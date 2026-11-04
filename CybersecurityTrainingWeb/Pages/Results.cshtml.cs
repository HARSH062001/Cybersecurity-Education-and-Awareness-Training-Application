using CybersecurityTrainingWeb.Data;
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

    public ResultsModel(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public long AttemptId { get; private set; }
    public int ModuleId { get; private set; }
    public string ModuleTitle { get; private set; } = "";
    public int Score { get; private set; }
    public int TotalQuestions { get; private set; }
    public int Answered { get; private set; }

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

                var correctOption = question.Options.FirstOrDefault(
                    option => option.Code == question.CorrectOptionCode);

                return new QuestionResult
                {
                    Number = question.Number,
                    QuestionText = question.Text,
                    SelectedOptionCode = selectedOption?.Code,
                    SelectedOptionText = selectedOption?.Text,
                    CorrectOptionCode = correctOption?.Code,
                    CorrectOptionText = correctOption?.Text,
                    IsCorrect = answer?.IsCorrect == true,
                    Explanation = question.Explanation
                };
            })
            .ToList();

        Response.Headers.CacheControl = "no-store";

        AttemptId = attempt.Id;
        ModuleId = attempt.Bank.ModuleId;
        ModuleTitle = moduleTitles[ModuleId - 1];
        Score = attempt.Score.Value;
        TotalQuestions = attempt.TotalQuestions.Value;
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
        public string? CorrectOptionCode { get; init; }
        public string? CorrectOptionText { get; init; }
        public bool IsCorrect { get; init; }
        public string? Explanation { get; init; }
    }
}