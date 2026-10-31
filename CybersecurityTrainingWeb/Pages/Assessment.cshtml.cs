using System.Text.Json;
using CybersecurityTrainingWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CybersecurityTrainingWeb.Pages;

public class AssessmentModel : PageModel
{
    public int ModuleId { get; private set; }
    public string Bank { get; private set; } = "A";
    public string ModuleTitle { get; private set; } = "Cybersecurity Fundamentals";
    public List<QuestionDto> Questions { get; private set; } = [];

    private readonly ApplicationDbContext _dbContext;

    public AssessmentModel(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> OnGetAsync(int moduleId = 1, string? bank = null)
    {
        //returns to module page if no bank is found
        if (moduleId is < 1 or > 8) return RedirectToPage("/Modules");
        if (bank is null || bank.Length != 1 || bank[0] is < 'A' or > 'C') bank = ((char)('A' + Random.Shared.Next(3))).ToString();

        //Takes an instance of a question bank from the database
        var realBank = await _dbContext.AssessmentBanks
            .Where(b => b.ModuleId == moduleId)
            .Where(b => b.IsPublished)
            .Where(b => b.Code == bank.ToUpperInvariant())
            .OrderByDescending(b => b.Version)
            .AsNoTracking()
            .Include(b => b.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync();

        //returns to module page if no bank is found
        if (realBank is null) return RedirectToPage("/Modules");

        ModuleId = moduleId;
        Bank = bank.ToUpperInvariant();
        ModuleTitle = ModuleTitles[moduleId - 1];

        Questions = realBank.Questions
            .OrderBy(q => q.Number)
            .Select(q => new QuestionDto
            {
                Id = $"m{moduleId}{bank.ToLowerInvariant()}q{q.Number}",
                Number = q.Number,
                Text = q.Text,
                Type = q.Type,
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

    private static readonly string[] ModuleTitles =
    [
        "Cybersecurity Fundamentals", "Phishing and Social Engineering", "Password and Identity Security", "Data Security and Privacy",
        "Network and Internet Security", "Device and Physical Security", "AI Literacy and AI Security", "Incident Reporting and Cyber Hygiene"
    ];

    public sealed class BankDto { public int ModuleId { get; set; } public string Bank { get; set; } = ""; public List<QuestionDto> Questions { get; set; } = []; }
    public sealed class QuestionDto { public string Id { get; set; } = ""; public int Number { get; set; } public string Text { get; set; } = ""; public string Type { get; set; } = "multipleChoice"; public List<OptionDto> Options { get; set; } = []; }
    public sealed class OptionDto { public string Code { get; set; } = ""; public string Text { get; set; } = ""; }
}
