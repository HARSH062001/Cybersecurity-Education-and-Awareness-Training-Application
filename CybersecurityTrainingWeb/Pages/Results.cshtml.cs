using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CybersecurityTrainingWeb.Utils;
namespace CybersecurityTrainingWeb.Pages;

[Authorize]
public class ResultsModel : PageModel
{
    public int Answered { get; set; }
    public string ModuleTitle { get; set; }
    public int Score { get; set; }
    public bool IsPassed { get; set; }
    public bool HasAttempted { get; set; }
    public int ModuleId { get; set; }
    public List<CorrectedQuestionDto> Questions { get; private set; } = [];
    public int TotalQuestions { get; set; } = 10;


    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ResultsModel(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync(
        int moduleId)
    {
        // Get user info
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        // try to get user's latest attempt on the module
        var attempt = await ResultUtils.GetLatestFinishedAttempt(_dbContext, userId, moduleId);

        if (attempt is null)
        {
            return RedirectToPage("/Modules"); // redirect to modules page if no completed attempt
        }

        if (attempt.Score is int s)
        {
            Score = s;
        }

        IsPassed = await ResultUtils.HasUserPassed(_dbContext, moduleId, userId);
        Answered = attempt.Answers.Count;
        ModuleTitle = ModuleTitles[moduleId - 1];
        ModuleId = moduleId;

        var bank = attempt.Bank;

        //Make answer dictionary (key: question id, value: the answer the user did)
        var answers = attempt.Answers.ToDictionary(
            attempt => attempt.AssessmentQuestionId,
            attempt => attempt.SelectedOptionCode
        );

        //Implement showing of question results
        Questions = attempt.Bank.Questions
            .OrderBy(q => q.Number)
            .Select(q => new CorrectedQuestionDto
            {
                Id = $"m{moduleId}{bank.Code}q{q.Number}",
                Number = q.Number,
                Text = q.Text,
                Type = q.Type,
                DatabaseId = q.Id,
                Options = q.Options.Select(o => new OptionDto
                {
                    Code = o.Code,
                    Text = o.Text,
                    DidUserAnswer = answers[q.Id] == o.Code,
                    IsCorrectAnswer = q.CorrectOptionCode == o.Code
                }).OrderBy(o => o.Code)
                .ToList()
            })
            .ToList();
        return Page();
    }

    private static readonly string[] ModuleTitles =
    [
        "Cybersecurity Fundamentals", "Phishing and Social Engineering", "Password and Identity Security", "Data Security and Privacy",
        "Network and Internet Security", "Device and Physical Security", "AI Literacy and AI Security", "Incident Reporting and Cyber Hygiene"
    ];

    public sealed class CorrectedQuestionDto
    {
        public string Id { get; set; } = "";
        public int Number { get; set; }
        public string Text { get; set; } = "";
        public string Type { get; set; } = "multipleChoice";
        public List<OptionDto> Options { get; set; } = [];
        public long DatabaseId { get; set; } = 0;
    }
    public sealed class OptionDto
    {
        public string Code { get; set; } = "";
        public string Text { get; set; } = "";
        public bool IsCorrectAnswer { get; set; }
        public bool DidUserAnswer { get; set; }
    }

}
