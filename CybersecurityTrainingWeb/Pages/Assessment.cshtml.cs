using System.Text.Json;
using CybersecurityTrainingWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CybersecurityTrainingWeb.Utils;

namespace CybersecurityTrainingWeb.Pages;

[Authorize]
public class AssessmentModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager; //get user information
    public int ModuleId { get; private set; }
    public string Bank { get; private set; } = "A";
    public string ModuleTitle { get; private set; } = "Cybersecurity Fundamentals";
    public List<QuestionDto> Questions { get; private set; } = [];

    private readonly ApplicationDbContext _dbContext;

    [BindProperty]
    public long AttemptId { get; set; } = 0;

    // Maps the question id to the answer code (A,B,C or D)
    [BindProperty]
    public Dictionary<long, string> Answers { get; set; } = new();

    public AssessmentModel(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    //TODO: Refactor variable names as they are very confusing (bank, code, id, etc)
    public async Task<IActionResult> OnGetAsync(int moduleId = 1)
    {
        // Get id of the user logged in
        var userId = _userManager.GetUserId(User);
        if (userId is null) // Just in case the user is not logged in (should not happen)
        {
            return Unauthorized();
        }
        //returns to module page if no bank is found
        if (moduleId is < 1 or > 8) return RedirectToPage("/Modules");

        // Check whether user has any existing attempts,
        // If not, create a new attempt and assign bank based on attempt history
        var assessmentAttempt = AttemptUtils.GetOrCreateAttempt(_dbContext, userId, moduleId);
        var bankId = assessmentAttempt.AssessmentBankId;
        AttemptId = assessmentAttempt.Id;

        // Takes the question bank from the database
        // Since a question bank can be updated, match the version of the bank
        // with the bank version in the AssessmentAttempt (b.Id == bankId)
        var realBank = await _dbContext.AssessmentBanks
            .Where(b => b.ModuleId == moduleId)
            .Where(b => b.IsPublished)
            .Where(b => b.Id == bankId)
            .OrderByDescending(b => b.Version)
            .AsNoTracking()
            .Include(b => b.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync();

        //returns to module page if no bank is found
        if (realBank is null) return RedirectToPage("/Modules");

        ModuleId = moduleId;
        Bank = realBank.Code;
        ModuleTitle = ModuleTitles[moduleId - 1];

        Questions = realBank.Questions
            .OrderBy(q => q.Number)
            .Select(q => new QuestionDto
            {
                Id = $"m{moduleId}{realBank.Code}q{q.Number}",
                Number = q.Number,
                Text = q.Text,
                Type = q.Type,
                DatabaseId = q.Id,
                Options = q.Options.Select(o => new OptionDto
                {
                    Code = o.Code,
                    Text = o.Text
                }).OrderBy(o => o.Code)
                .ToList()
            })
            .ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Get id of the user logged in
        var userId = _userManager.GetUserId(User);
        if (userId is null) // Just in case the user is not logged in (should not happen)
        {
            return Unauthorized();
        }

        //get attempt from database and validate it
        var assessmentAttempt = await AttemptUtils.ValidateAndMarkAttempt(_dbContext, userId, AttemptId, Answers);

        if (assessmentAttempt is null)
        {
            return BadRequest("Invalid assessment attempt or answers.");
        }
        return RedirectToPage("/Results", new { moduleId = assessmentAttempt.Bank.ModuleId });
    }
    private static readonly string[] ModuleTitles =
    [
        "Cybersecurity Fundamentals", "Phishing and Social Engineering", "Password and Identity Security", "Data Security and Privacy",
        "Network and Internet Security", "Device and Physical Security", "AI Literacy and AI Security", "Incident Reporting and Cyber Hygiene"
    ];

    public sealed class BankDto { public int ModuleId { get; set; } public string Bank { get; set; } = ""; public List<QuestionDto> Questions { get; set; } = []; }
    public sealed class QuestionDto
    {
        public string Id { get; set; } = "";
        public int Number { get; set; }
        public string Text { get; set; } = "";
        public string Type { get; set; } = "multipleChoice";
        public List<OptionDto> Options { get; set; } = [];
        public long DatabaseId { get; set; } = 0;
    }
    public sealed class OptionDto { public string Code { get; set; } = ""; public string Text { get; set; } = ""; }
}
