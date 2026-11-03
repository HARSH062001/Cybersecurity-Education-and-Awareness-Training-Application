using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CybersecurityTrainingWeb.Services;
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

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var attempt = await ResultUtils.GetLatestFinishedAttempt(_dbContext, userId, moduleId);

        if (attempt is null)
        {
            return RedirectToPage("/Modules");
        }

        if (attempt.Score is int s)
        {
            Score = s;
        }

        IsPassed = await ResultUtils.HasUserPassed(_dbContext, moduleId, userId);
        Answered = attempt.Answers.Count;
        ModuleTitle = ModuleTitles[moduleId];
        ModuleId = moduleId;
        return Page();
    }

    private static readonly string[] ModuleTitles =
    [
        "Cybersecurity Fundamentals", "Phishing and Social Engineering", "Password and Identity Security", "Data Security and Privacy",
        "Network and Internet Security", "Device and Physical Security", "AI Literacy and AI Security", "Incident Reporting and Cyber Hygiene"
    ];
}
